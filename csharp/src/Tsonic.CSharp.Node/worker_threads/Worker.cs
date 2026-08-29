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
    private static int _nextThreadId;
    private readonly object _stateLock = new();
    private readonly Process _process;
    private readonly WorkerTransport _transport;
    private bool _closed;
    private bool _refed = true;
    private int? _exitCode;

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
        try
        {
            if (!process.Start())
                throw new InvalidOperationException("Worker process could not be started.");
            var client = listener.AcceptTcpClientAsync()
                .WaitAsync(TimeSpan.FromSeconds(30))
                .GetAwaiter()
                .GetResult();
            var transport = new WorkerTransport(client);
            var authentication = transport.Receive();
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
            ProcessKeepAlive.Acquire();
            _ = Task.Run(ReadWorkerFrames);
            _ = Task.Run(WaitForExit);
        }
        catch
        {
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
        lock (_stateLock)
        {
            if (!_closed && !_process.HasExited)
                _process.Kill(entireProcessTree: true);
        }
        await _process.WaitForExitAsync().ConfigureAwait(false);
        Complete(_process.ExitCode);
        return _process.ExitCode;
    }

    /// <summary>Keeps the process alive while this worker is running.</summary>
    public Worker @ref()
    {
        lock (_stateLock)
        {
            if (!_closed && !_refed)
            {
                _refed = true;
                ProcessKeepAlive.Acquire();
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
                ProcessKeepAlive.Release();
            }
        }
        return this;
    }

    /// <summary>Terminates the worker and releases its process and transport resources.</summary>
    public void Dispose()
    {
        if (!_process.HasExited)
            _process.Kill(entireProcessTree: true);
        _process.WaitForExit();
        Complete(_process.ExitCode);
        _transport.Dispose();
        _process.Dispose();
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
        if (value is Undefined)
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
            if (entryValue is Undefined)
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
                var frame = _transport.Receive();
                switch (frame.Kind)
                {
                    case WorkerFrameKind.Message:
                    {
                        var value = StructuredClone.Decode(frame.Payload);
                        Tsonic.CSharp.Js.JsEventLoop.EnqueueHandleOwned(() => emit("message", value));
                        break;
                    }
                    case WorkerFrameKind.Error:
                    {
                        var error = new InvalidOperationException(
                            Encoding.UTF8.GetString(frame.Payload));
                        Tsonic.CSharp.Js.JsEventLoop.EnqueueHandleOwned(() => emit("error", error));
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
        catch (Exception error) when (
            error is IOException or ObjectDisposedException or SocketException or InvalidDataException)
        {
            if (!IsComplete())
                Tsonic.CSharp.Js.JsEventLoop.EnqueueHandleOwned(() => emit("error", error));
        }
    }

    private async Task WaitForExit()
    {
        await _process.WaitForExitAsync().ConfigureAwait(false);
        Complete(_process.ExitCode);
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
        if (retainedExit)
        {
            try
            {
                Tsonic.CSharp.Js.JsEventLoop.EnqueueReferenced(() => emit("exit", exitCode));
            }
            finally
            {
                ProcessKeepAlive.Release();
            }
        }
        else
        {
            Tsonic.CSharp.Js.JsEventLoop.EnqueueHandleOwned(() => emit("exit", exitCode));
        }
    }
}
