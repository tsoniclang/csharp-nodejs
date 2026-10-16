using System.Diagnostics;
using System.Globalization;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;
using Tsonic.CSharp.Js;
using Tsonic.CSharp.Runtime;

namespace Tsonic.CSharp.Node;

/// <summary>Runs a compiled module entry in an isolated worker process.</summary>
public sealed partial class Worker : EventEmitter, IDisposable
{
    private const int MaximumQueuedSignals = 1 << 16;
    private static int _nextThreadId;
    private readonly object _stateLock = new();
    private readonly Process _process;
    private readonly WorkerTransport _transport;
    private readonly Task<int> _reader;
    private readonly Queue<WorkerSignal> _signals = new();
    private WorkerSignal? _readerTerminal;
    private bool _closed;
    private bool _refed = true;
    private bool _terminationRequested;
    private bool _processExitObserved;
    private bool _terminalConsumed;
    private int? _exitCode;
    private int _nativeObservers;
    private ulong _nativeConsumption;
    private Exception? _nativeFailure;
    private Exception? _physicalFailure;
    private ulong _nativeFailureCount;
    private int _disposed;

    /// <summary>Starts the selected compiled worker entry with the supplied options.</summary>
    public Worker(string moduleEntryIdentity, WorkerOptions? options = null)
    {
        if (string.IsNullOrEmpty(moduleEntryIdentity))
            throw new ArgumentException("Worker module entry identity cannot be empty.", nameof(moduleEntryIdentity));
        options ??= new WorkerOptions();
        threadId = Interlocked.Increment(ref _nextThreadId);

        using var listener = new TcpListener(IPAddress.Loopback, 0);
        listener.Start(1);
        var endpoint = (IPEndPoint)listener.LocalEndpoint;
        var token = RandomNumberGenerator.GetBytes(worker_threads.AuthenticationTokenBytes);
        var process = CreateWorkerProcess(
            moduleEntryIdentity,
            options,
            endpoint.Port,
            token,
            threadId);
        WorkerTransport? transport = null;
        try
        {
            if (!process.Start())
                throw new InvalidOperationException("Worker process could not be started.");
            var client = listener.AcceptTcpClientAsync()
                .WaitAsync(TimeSpan.FromSeconds(30))
                .GetAwaiter()
                .GetResult();
            transport = new WorkerTransport(client);
            var authentication = transport.Receive() ??
                throw new EndOfStreamException("Worker transport ended before authentication.");
            if (authentication.Kind != WorkerFrameKind.Authenticate ||
                !CryptographicOperations.FixedTimeEquals(authentication.Payload, token))
            {
                throw new InvalidDataException("Worker process authentication failed.");
            }
            transport.Send(
                WorkerFrameKind.WorkerData,
                StructuredClone.Encode(options.workerData));
            transport.Send(
                WorkerFrameKind.EnvironmentData,
                worker_threads.EncodeEnvironmentDataSnapshot());
            _process = process;
            _transport = transport;
            _reader = Task.Run(ReadWorkerFrames);
        }
        catch
        {
            transport?.Dispose();
            try
            {
                if (!process.HasExited)
                    process.Kill(entireProcessTree: true);
                process.WaitForExit();
            }
            catch (InvalidOperationException)
            {
            }
            process.Dispose();
            throw;
        }
        if (Tsonic.CSharp.Js.JsEventLoop.IsRunning)
        {
            try { BindCallbackOwner(); }
            catch
            {
                Dispose();
                throw;
            }
        }
    }

    /// <summary>Gets the worker's process-local thread identity.</summary>
    public int threadId { get; }

    /// <summary>Sends a structured-cloned value to the worker.</summary>
    public void postMessage(TsValue value)
    {
        lock (_stateLock)
        {
            if (_closed)
                throw new InvalidOperationException("Worker is no longer running.");
            _transport.Send(WorkerFrameKind.Message, StructuredClone.Encode(value));
        }
    }

    /// <summary>Terminates the worker and returns its exit code.</summary>
    public Task<int> terminate()
    {
        try
        {
            RequestTermination();
            return ObserveCompletion();
        }
        catch (Exception error) { return Task.FromException<int>(error); }
    }

    private void RequestTermination()
    {
        lock (_stateLock)
        {
            if (_closed || _terminationRequested) return;
            if (_process.HasExited)
            {
                _processExitObserved = true;
                return;
            }
            _terminationRequested = true;
            try { _process.Kill(entireProcessTree: true); }
            catch (InvalidOperationException) when (_process.HasExited)
            {
                _processExitObserved = true;
            }
            catch
            {
                _terminationRequested = false;
                throw;
            }
        }
    }

    /// <summary>Keeps the process alive while this worker is running.</summary>
    public Worker @ref()
    {
        lock (_stateLock)
        {
            if (!_closed && !_refed)
            {
                _refed = true;
                AcquireReference();
            }
        }
        return this;
    }

    /// <summary>Allows the process to exit while this worker is running.</summary>
    public Worker unref()
    {
        lock (_stateLock)
        {
            if (_refed)
            {
                _refed = false;
                ReleaseReference();
            }
        }
        return this;
    }

    /// <summary>Terminates the worker and releases its process and transport resources.</summary>
    public void Dispose()
    {
        if (Interlocked.Exchange(ref _disposed, 1) != 0) return;
        try
        {
            lock (_stateLock)
                if (_callbackOwner is { } owner) RetireCallbacks(owner);
            RequestTermination();
            ObserveCompletion().GetAwaiter().GetResult();
        }
        finally
        {
            lock (_stateLock)
                if (_callbackOwner is { } owner) RetireCallbacks(owner);
        }
    }

    private static Process CreateWorkerProcess(
        string moduleEntryIdentity,
        WorkerOptions options,
        int port,
        byte[] token,
        int selectedThreadId)
    {
        var processPath = Environment.ProcessPath ??
            throw new InvalidOperationException("The current executable path is unavailable.");
        var commandLine = Environment.GetCommandLineArgs();
        var startInfo = new ProcessStartInfo
        {
            FileName = processPath,
            UseShellExecute = false,
        };
        if (string.Equals(
                Path.GetFileNameWithoutExtension(processPath),
                "dotnet",
                StringComparison.OrdinalIgnoreCase) &&
            commandLine.Length > 0 &&
            string.Equals(Path.GetExtension(commandLine[0]), ".dll", StringComparison.OrdinalIgnoreCase))
        {
            startInfo.ArgumentList.Add(Path.GetFullPath(commandLine[0]));
        }
        startInfo.ArgumentList.Add(worker_threads.WorkerArgumentMarker);
        startInfo.ArgumentList.Add(moduleEntryIdentity);
        startInfo.ArgumentList.Add(port.ToString(CultureInfo.InvariantCulture));
        startInfo.ArgumentList.Add(Convert.ToBase64String(token));
        startInfo.ArgumentList.Add(selectedThreadId.ToString(CultureInfo.InvariantCulture));
        startInfo.ArgumentList.Add(options.name ?? string.Empty);
        startInfo.ArgumentList.Add(worker_threads.WorkerArgumentDelimiter);
        if (options.argv is not null)
        {
            foreach (var argument in options.argv)
                startInfo.ArgumentList.Add(argument);
        }
        ApplyEnvironment(startInfo, options.env);
        return new Process { StartInfo = startInfo };
    }

    private static void ApplyEnvironment(ProcessStartInfo startInfo, TsValue environment)
    {
        var value = environment.unwrap();
        if (value is null)
            return;
        IEnumerable<KeyValuePair<string, object?>> entries = value switch
        {
            TsObject target => target.entries(),
            JSObject target => target.entries().Select(entry =>
                new KeyValuePair<string, object?>(entry.key, entry.value)),
            IReadOnlyDictionary<string, object?> target => target,
            IDictionary<string, object?> target => target,
            _ => throw new TypeError("WorkerOptions.env must be a closed string-valued object."),
        };
        startInfo.Environment.Clear();
        foreach (var entry in entries)
        {
            var entryValue = entry.Value is TsValue typed ? typed.unwrap() : entry.Value;
            if (entryValue is null)
                continue;
            if (entryValue is not string text)
                throw new TypeError("WorkerOptions.env values must be strings or undefined.");
            startInfo.Environment[entry.Key] = text;
        }
    }

    private int ReadWorkerFrames()
    {
        WorkerSignal? terminal = null;
        try
        {
            while (true)
            {
                if (_transport.Receive() is not { } frame) break;
                switch (frame.Kind)
                {
                    case WorkerFrameKind.Message:
                    {
                        TsValue value;
                        try { value = StructuredClone.Decode(frame.Payload); }
                        catch (Exception error) when (error is IOException or InvalidOperationException)
                        {
                            PublishSignal(new(WorkerSignalKind.Error, default, error));
                            break;
                        }
                        PublishSignal(new(WorkerSignalKind.Message, value, null));
                        break;
                    }
                    case WorkerFrameKind.Error:
                    {
                        var error = new InvalidOperationException(
                            Encoding.UTF8.GetString(frame.Payload));
                        PublishSignal(new(WorkerSignalKind.Error, default, error));
                        break;
                    }
                    case WorkerFrameKind.Close:
                        goto ReaderFinished;
                    default:
                        throw new InvalidDataException(
                            $"Unexpected worker frame '{frame.Kind}'.");
                }
            }
        }
        catch (Exception error) when (error is InvalidDataException)
        {
            terminal = new(WorkerSignalKind.Error, default, error);
        }
        catch (Exception error) when (error is IOException or ObjectDisposedException or SocketException)
        {
            terminal = new(WorkerSignalKind.TransportError, default, error);
        }
        catch (Exception error)
        {
            terminal = new(WorkerSignalKind.InternalError, default, error);
        }
    ReaderFinished:
        lock (_stateLock)
        {
            _readerTerminal = terminal;
            if (_callbackOwner is null && _nativeObservers != 0)
                ConsumeUnownedSignals();
            ScheduleDelivery();
        }
        try
        {
            int exitCode;
            try
            {
                if (terminal is { Kind: WorkerSignalKind.InternalError }) RequestTermination();
                _process.WaitForExit();
                exitCode = _process.ExitCode;
            }
            finally
            {
                lock (_stateLock) _closed = true;
                try { _transport.Dispose(); }
                finally { _process.Dispose(); }
            }
            lock (_stateLock)
            {
                _processExitObserved = true;
                _exitCode = exitCode;
                _closed = true;
                if (_callbackOwner is null && _nativeObservers != 0)
                    ConsumeUnownedSignals();
                ScheduleDelivery();
            }
            return exitCode;
        }
        catch (Exception error)
        {
            lock (_stateLock)
            {
                _closed = true;
                _physicalFailure = error;
                ScheduleDelivery();
            }
            throw;
        }
    }

    private void PublishSignal(WorkerSignal signal)
    {
        lock (_stateLock)
        {
            while (_signals.Count == MaximumQueuedSignals &&
                !(_callbackOwner is null && _nativeObservers != 0))
                Monitor.Wait(_stateLock);
            if (_callbackOwner is null && _nativeObservers != 0)
                ConsumeNativeSignal(signal);
            else
                _signals.Enqueue(signal);
            ScheduleDelivery();
        }
    }

    private void ConsumeNativeSignal(WorkerSignal signal)
    {
        _nativeConsumption = checked(_nativeConsumption + 1);
        if (signal.Kind == WorkerSignalKind.Message) return;
        if (SuppressesNativeIo(signal)) return;
        _nativeFailure ??= signal.Error!;
        _nativeFailureCount = checked(_nativeFailureCount + 1);
    }

    private bool SuppressesNativeIo(WorkerSignal signal) =>
        signal.Kind == WorkerSignalKind.TransportError && (_terminationRequested || _processExitObserved);

    private void ConsumeUnownedSignals()
    {
        while (_signals.TryDequeue(out var signal)) ConsumeNativeSignal(signal);
        if (_readerTerminal is { } terminal)
        {
            _readerTerminal = null;
            ConsumeNativeSignal(terminal);
        }
        if (_exitCode.HasValue) _terminalConsumed = true;
        Monitor.PulseAll(_stateLock);
    }

    private Task<int> ObserveCompletion()
    {
        bool observedUnowned;
        ulong before;
        lock (_stateLock)
        {
            _nativeObservers = checked(_nativeObservers + 1);
            before = _nativeConsumption;
            observedUnowned = _callbackOwner is null;
            if (observedUnowned) ConsumeUnownedSignals();
        }
        if (_reader.IsCompleted)
        {
            try
            {
                if (_reader.IsCompletedSuccessfully)
                    CompleteNativeObservation(_reader.Result, before, observedUnowned);
                return _reader;
            }
            catch (Exception error) { return Task.FromException<int>(error); }
            finally { lock (_stateLock) _nativeObservers--; }
        }
        return AwaitPhysicalCompletion(before, observedUnowned);
    }

    private async Task<int> AwaitPhysicalCompletion(ulong before, bool observedUnowned)
    {
        try
        {
            var exitCode = await _reader.ConfigureAwait(false);
            return CompleteNativeObservation(exitCode, before, observedUnowned);
        }
        finally
        {
            lock (_stateLock) _nativeObservers--;
        }
    }

    private int CompleteNativeObservation(int exitCode, ulong before, bool observedUnowned)
    {
        lock (_stateLock)
        {
            if (_callbackOwner is null)
            {
                observedUnowned = true;
                ConsumeUnownedSignals();
            }
            observedUnowned |= _nativeConsumption != before;
            if (observedUnowned && _nativeFailure is { } error)
            {
                if (_nativeFailureCount > 1)
                    error.Data["AdditionalUnobservedWorkerErrors"] = _nativeFailureCount - 1;
                System.Runtime.ExceptionServices.ExceptionDispatchInfo.Capture(error).Throw();
            }
        }
        return exitCode;
    }

    private enum WorkerSignalKind { Message, Error, TransportError, InternalError }
    private readonly record struct WorkerSignal(WorkerSignalKind Kind, TsValue Value, Exception? Error);
}
