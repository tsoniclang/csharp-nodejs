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
public sealed class Worker : EventEmitter, IDisposable
{
    private Tsonic.CSharp.Js.LoopDispatch _ownerDispatch;
    private Tsonic.CSharp.Js.LoopRegistration _loopRegistration;
    private Tsonic.CSharp.Js.LoopDispatch WorkerDispatch => BackgroundDispatch.Bind(ref _ownerDispatch, NodePhases.Workers);

    private static int _nextThreadId;
    private readonly object _stateLock = new();
    private readonly Process _process;
    private readonly WorkerTransport _transport;
    private readonly Task _reader;
    private readonly Task<int> _completion;
    private bool _closed;
    private bool _refed = true;
    private bool _terminationRequested;
    private int? _exitCode;
    private int _disposed;

    /// <summary>Starts the selected compiled worker entry with the supplied options.</summary>
    public Worker(string moduleEntryIdentity, WorkerOptions? options = null)
    {
        if (string.IsNullOrEmpty(moduleEntryIdentity))
            throw new ArgumentException("Worker module entry identity cannot be empty.", nameof(moduleEntryIdentity));
        _ = WorkerDispatch;
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
        _loopRegistration = WorkerDispatch.Register(static state => ((Worker)state!).Dispose(), this);
        try
        {
            if (!process.Start())
                throw new InvalidOperationException("Worker process could not be started.");
            var client = listener.AcceptTcpClientAsync()
                .WaitAsync(TimeSpan.FromSeconds(30))
                .GetAwaiter()
                .GetResult();
            var transport = new WorkerTransport(client);
            var authentication = transport.Receive() ??
                throw new EndOfStreamException("Worker transport ended before authentication.");
            if (authentication.Kind != WorkerFrameKind.Authenticate ||
                !CryptographicOperations.FixedTimeEquals(authentication.Payload, token))
            {
                transport.Dispose();
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
            WorkerDispatch.Acquire();
            _reader = Task.Run(ReadWorkerFrames);
            _completion = WaitForExit();
        }
        catch
        {
            _loopRegistration.Dispose();
            try
            {
                if (!process.HasExited)
                    process.Kill(entireProcessTree: true);
            }
            catch (InvalidOperationException)
            {
            }
            process.Dispose();
            throw;
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
    public async Task<int> terminate()
    {
        RequestTermination();
        return await _completion.ConfigureAwait(false);
    }

    private void RequestTermination()
    {
        lock (_stateLock)
        {
            if (_closed || _terminationRequested || _process.HasExited) return;
            try { _process.Kill(entireProcessTree: true); }
            catch (InvalidOperationException) when (_process.HasExited) { }
            _terminationRequested = true;
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
                WorkerDispatch.Acquire();
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
                WorkerDispatch.Release();
            }
        }
        return this;
    }

    /// <summary>Terminates the worker and releases its process and transport resources.</summary>
    public void Dispose()
    {
        if (Interlocked.Exchange(ref _disposed, 1) != 0) return;
        RequestTermination();
        _completion.GetAwaiter().GetResult();
        _transport.Dispose();
        _process.Dispose();
        _loopRegistration.Dispose();
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
        return new Process { StartInfo = startInfo, EnableRaisingEvents = true };
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

    private void ReadWorkerFrames()
    {
        try
        {
            while (true)
            {
                if (_transport.Receive() is not { } frame) return;
                switch (frame.Kind)
                {
                    case WorkerFrameKind.Message:
                    {
                        TsValue value;
                        try { value = StructuredClone.Decode(frame.Payload); }
                        catch (Exception error) when (error is IOException or InvalidOperationException)
                        {
                            BackgroundDispatch.Enqueue(WorkerDispatch, () => emit("error", error));
                            break;
                        }
                        BackgroundDispatch.Enqueue(WorkerDispatch, () => emit("message", value));
                        break;
                    }
                    case WorkerFrameKind.Error:
                    {
                        var error = new InvalidOperationException(
                            Encoding.UTF8.GetString(frame.Payload));
                        BackgroundDispatch.Enqueue(WorkerDispatch, () => emit("error", error));
                        break;
                    }
                    case WorkerFrameKind.Close:
                        return;
                    default:
                        throw new InvalidDataException(
                            $"Unexpected worker frame '{frame.Kind}'.");
                }
            }
        }
        catch (Exception error)
        {
            if (!IsComplete())
                BackgroundDispatch.Enqueue(WorkerDispatch, () =>
                {
                    if (!IsExpectedTransportEnd(error)) emit("error", error);
                });
        }
    }

    private async Task<int> WaitForExit()
    {
        await _process.WaitForExitAsync().ConfigureAwait(false);
        var exitCode = _process.ExitCode;
        await _reader.ConfigureAwait(ConfigureAwaitOptions.SuppressThrowing);
        Complete(exitCode);
        return exitCode;
    }

    private bool IsExpectedTransportEnd(Exception error)
    {
        if (error is InvalidDataException) return false;
        if (error is not (IOException or ObjectDisposedException or SocketException)) return false;
        lock (_stateLock) return _terminationRequested || _process.HasExited;
    }

    private bool IsComplete()
    {
        lock (_stateLock) return _closed;
    }

    private void Complete(int exitCode)
    {
        var retainedExit = false;
        lock (_stateLock)
        {
            if (_exitCode.HasValue) return;
            _exitCode = exitCode;
            _closed = true;
            _transport.Dispose();
            if (_refed)
            {
                _refed = false;
                retainedExit = true;
            }
        }
        try
        {
            BackgroundDispatch.Enqueue(WorkerDispatch, () => emit("exit", exitCode));
        }
        finally
        {
            if (retainedExit) WorkerDispatch.Release();
        }
    }
}
