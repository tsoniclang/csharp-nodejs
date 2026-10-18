using System.Buffers.Binary;
using System.Net;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text;
using Tsonic.CSharp.Js;
using Tsonic.CSharp.Node;
using Tsonic.CSharp.Runtime;

internal static partial class Program
{
    private const long InitialValue = 9_007_199_254_740_993L;
    private static string? _selectedControl;
    private static bool _baseConstructionOwner;

    private static bool Selected(string control) => _selectedControl is null || _selectedControl == control;

    private static void Main(string[] arguments)
    {
        if (arguments.Length >= 2 && arguments[0] == "--native-control")
        {
            var selected = arguments[1];
            _selectedControl = selected;
            _baseConstructionOwner = arguments.Length == 3;
            try
            {
                Require(new[] { "construction", "cancellation", "concurrent", "signal-status", "ready-cost",
                    "retained-prefix", "unobserved-errors", "retired-errors", "saturation-private", "default-ref",
                    "saturated-terminate", "saturated-dispose", "owner-saturated-dispose", "rebind", "io-before-exit", "io-after-exit",
                    "delivery-native-worker-partial", "delivery-native-worker-unread", "delivery-native-worker-live-failure", "delivery-native-worker-epipe" }.Contains(selected),
                    "one known independently selected native control");
                VerifyNativePhysicalObservation();
                if (selected.StartsWith("delivery-", StringComparison.Ordinal)) VerifyNativeTerminationAndUnreadPosts();
                Console.WriteLine($"P05 native control {selected}: passed");
            }
            catch (Exception error)
            {
                Console.Error.WriteLine($"P05 native control {selected}: FAILED {error.GetType().Name}: {error.Message}");
                Environment.ExitCode = 1;
            }
            return;
        }
        if (arguments is ["--native-physical-owner-check"])
        {
            VerifyNativePhysicalObservation();
            return;
        }
        if (arguments.Length != 0)
        {
            if (arguments[0] == "--native-signal-baseline")
            {
                using var baseline = new TcpClient(AddressFamily.InterNetwork);
                baseline.Connect(IPAddress.Loopback, int.Parse(arguments[1]));
                baseline.GetStream().WriteByte(1);
                _ = baseline.GetStream().ReadByte();
                return;
            }
            if (arguments[1] is "native-worker-invalid-kind" or "native-worker-invalid-length" or "native-worker-invalid-clone"
                or "native-worker-partial" or "native-worker-unread" or "native-worker-live-failure"
                or "native-worker-pending" or "native-worker-burst" or "native-worker-errors"
                or "native-worker-empty" or "native-worker-burst-errors" or "native-worker-saturated")
            {
                SendInvalidNativeWorkerFrame(arguments);
                return;
            }
            Require(worker_threads.InitializeWorkerProcess(arguments) == "native-worker-tail", "native tail worker entry");
            using var parent = worker_threads.parentPort ?? throw new InvalidOperationException("native tail worker parent port");
            Require(ExactInt64(ReceiveEventually(parent)) == 1, "native tail worker start signal");
            for (var index = 0; index < 256; index++) parent.postMessage(TsValue.from(index));
            return;
        }
        RequireNoLoop();
        using var listener = new TcpListener(IPAddress.Loopback, 0);
        listener.Start(1);
        var token = RandomNumberGenerator.GetBytes(32);
        var firstSubscription = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var firstRetirement = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var secondSubscription = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var peer = Task.Run(() => ServePeer(listener, token, firstSubscription.Task,
            firstRetirement.Task, secondSubscription.Task));
        var endpoint = (IPEndPoint)listener.LocalEndpoint;
        var entry = worker_threads.InitializeWorkerProcess([
            "--tsonic-node-worker-v1", "native-worker-probe", endpoint.Port.ToString(),
            Convert.ToBase64String(token), "19", "native-worker-probe", "--", "argument",
        ]);
        Require(entry == "native-worker-probe", "public bootstrap entry identity");
        Require(!worker_threads.isMainThread && worker_threads.threadId == 19, "public worker identity");
        Require(ExactInt64(worker_threads.workerData) == InitialValue, "exact worker-data carrier");
        Require(ExactInt64(worker_threads.getEnvironmentData("native-key")) == InitialValue + 1,
            "exact native environment-data snapshot");
        Require(Tsonic.CSharp.Node.process.argv.SequenceEqual([
            Tsonic.CSharp.Node.process.execPath, "native-worker-probe", "argument",
        ]), "native process arguments");
        using var port = worker_threads.parentPort ?? throw new InvalidOperationException("parent port missing");
        RequireNoLoop();
        port.postMessage(TsValue.from(InitialValue + 2));
        Require(ExactInt64(ReceiveEventually(port)) == InitialValue + 3, "pre-loop native receive");
        var owner = Environment.CurrentManagedThreadId;
        var firstCalls = 0;
        JsEventLoop.Run(() =>
        {
            port.on<TsValue>("message", value =>
            {
                Require(Environment.CurrentManagedThreadId == owner, "first callback owner");
                Require(ExactInt64(value) == InitialValue + 4, "first callback exact data");
                firstCalls++;
                port.postMessage(TsValue.from(InitialValue + 5));
                port.unref();
            });
            Require(ProcessKeepAlive.HasReferences, "referenced native parent callback owner");
            firstSubscription.SetResult();
            Require(firstCalls == 0, "first callback is deferred");
        });
        Require(firstCalls == 1 && port.listenerCount("message") == 0, "first callbacks retired");
        RequireNoLoop();
        firstRetirement.SetResult();
        Require(ExactInt64(ReceiveEventually(port)) == InitialValue + 6, "post-retirement native receive");
        port.postMessage(TsValue.from(InitialValue + 7));
        port.@ref();
        Require(port.hasRef() && !ProcessKeepAlive.HasReferences, "native ref does not capture an epoch");
        var secondCalls = 0;
        JsEventLoop.Run(() =>
        {
            port.on<TsValue>("message", value =>
            {
                Require(Environment.CurrentManagedThreadId == owner, "second callback owner");
                Require(ExactInt64(value) == InitialValue + 8, "second callback exact data");
                secondCalls++;
                port.unref();
            });
            secondSubscription.SetResult();
        });
        Require(firstCalls == 1 && secondCalls == 1, "only explicit fresh callbacks run");
        Require(port.listenerCount("message") == 0, "second callbacks retired");
        RequireNoLoop();
        port.close();
        peer.WaitAsync(TimeSpan.FromSeconds(10)).GetAwaiter().GetResult();
        Require(!port.hasRef(), "closed native reference released");
        VerifyNativeWorkerTail();
        VerifyNativeTerminationAndUnreadPosts();
        VerifyNativePhysicalObservation();
        Console.WriteLine("{\"public_native_bootstrap\":true,\"exact_int64\":true," +
            "\"preloop_transport\":true,\"retained_transport\":true,\"fresh_epoch\":true}");
    }

    private static void VerifyNativeWorkerTail()
    {
        for (var iteration = 0; iteration < 8; iteration++)
        {
            var messages = new List<int>();
            var failures = new List<Exception>();
            var exits = 0;
            Worker? worker = null;
            try
            {
                JsEventLoop.Run(() =>
                {
                    worker = new Worker("native-worker-tail");
                    worker.on<TsValue>("message", value =>
                    {
                        Require(value.unwrap() is int, "native tail message exact int32 carrier");
                        messages.Add((int)value.unwrap()!);
                    });
                    worker.on<Exception>("error", failures.Add);
                    worker.on<int>("exit", code =>
                    {
                        Require(code == 0, "native tail worker exit status");
                        Require(messages.SequenceEqual(Enumerable.Range(0, 256)), "all native tail messages precede exit");
                        exits++;
                    });
                    worker.postMessage(TsValue.from(1L));
                });
                Require(failures.Count == 0, "orderly native worker EOF creates no synthetic error");
                Require(exits == 1, "native worker completes exactly once");
                Require(messages.Count == 256, "native worker retains its complete terminal burst");
            }
            finally { worker?.Dispose(); }
        }
        foreach (var entry in new[] { "native-worker-invalid-kind", "native-worker-invalid-length", "native-worker-invalid-clone" })
        {
            var failures = new List<Exception>();
            var exits = 0;
            Worker? worker = null;
            try
            {
                JsEventLoop.Run(() =>
                {
                    worker = new Worker(entry);
                    worker.on<Exception>("error", failures.Add);
                    worker.on<int>("exit", code =>
                    {
                        Require(code == 0, "native malformed-peer process status");
                        Require(failures.Count == 1, "one native protocol failure precedes completion");
                        exits++;
                    });
                });
                Require(failures.Count == 1 && (entry == "native-worker-invalid-clone"
                    ? failures[0] is EndOfStreamException : failures[0] is InvalidDataException),
                    "native malformed full header retains its protocol exception");
                Require(exits == 1, "native protocol error releases completion exactly once");
                Require(worker!.terminate().GetAwaiter().GetResult() == 0,
                    "malformed-peer completion remains nonthrowing for terminate");
            }
            finally { worker?.Dispose(); }
        }
    }

    private static void SendInvalidNativeWorkerFrame(string[] arguments)
    {
        using var client = new TcpClient(AddressFamily.InterNetwork);
        client.Connect(IPAddress.Loopback, int.Parse(arguments[2]));
        using var stream = client.GetStream();
        WriteFrame(stream, 1, Convert.FromBase64String(arguments[3]));
        Require(ReadFrame(stream).Kind == 2 && ReadFrame(stream).Kind == 3,
            "native malformed-peer handshake remains valid");
        if (arguments[1] == "native-worker-empty")
        {
            WriteFrame(stream, 4, EncodeInt64(InitialValue));
            return;
        }
        if (arguments[1] is "native-worker-pending" or "native-worker-burst" or "native-worker-errors"
            or "native-worker-burst-errors" or "native-worker-saturated")
        {
            using var nativeControl = new TcpClient(AddressFamily.InterNetwork);
            nativeControl.Connect(IPAddress.Loopback, int.Parse(arguments[7]));
            if (arguments[1] == "native-worker-errors")
            {
                WriteFrame(stream, 5, Encoding.UTF8.GetBytes("first native error"));
                WriteFrame(stream, 5, Encoding.UTF8.GetBytes("second native error"));
                WriteFrame(stream, 6, []);
                nativeControl.GetStream().WriteByte(1);
                return;
            }
            nativeControl.GetStream().WriteByte(1);
            if (arguments[1] == "native-worker-pending")
            {
                _ = nativeControl.GetStream().ReadByte();
                return;
            }
            if (arguments[1] == "native-worker-saturated")
                WriteFrame(stream, 5, Encoding.UTF8.GetBytes("public saturated native error"));
            var total = arguments[1] == "native-worker-saturated" ? 1 << 18 : (1 << 17) + 17;
            for (var index = 0; index < total; index++)
            {
                if (arguments[1] == "native-worker-burst-errors" &&
                    (index == 0 || index == 1 << 16 || index == 1 << 17))
                    WriteFrame(stream, 5, Encoding.UTF8.GetBytes($"native error at {index}"));
                WriteFrame(stream, 4, EncodeInt64(index));
            }
            if (arguments[1] == "native-worker-saturated")
            {
                _ = nativeControl.GetStream().ReadByte();
                return;
            }
            client.Client.Shutdown(SocketShutdown.Send);
            return;
        }
        if (arguments[1] == "native-worker-invalid-kind") WriteFrame(stream, 99, []);
        else if (arguments[1] == "native-worker-invalid-length")
        {
            Span<byte> header = stackalloc byte[5];
            header[0] = 4;
            BinaryPrimitives.WriteInt32BigEndian(header[1..], -1);
            stream.Write(header);
            stream.Flush();
        }
        else if (arguments[1] == "native-worker-invalid-clone") WriteFrame(stream, 4, [1]);
        else
        {
            for (var index = 0; index < 10; index++) WriteFrame(stream, 4, EncodeInt64(index));
            if (arguments[1] is "native-worker-partial" or "native-worker-live-failure")
                stream.Write(new byte[] { 4, 0, 0, 0, 8, 7 });
            if (arguments[1] == "native-worker-live-failure") client.Client.Shutdown(SocketShutdown.Send);
            using var control = new TcpClient(AddressFamily.InterNetwork);
            control.Connect(IPAddress.Loopback, int.Parse(arguments[7]));
            control.GetStream().WriteByte(1);
            _ = control.GetStream().ReadByte();
        }
    }

    private static void VerifyNativeTerminationAndUnreadPosts()
    {
        var nativeExit = DirectNativeKillStatus();
        if (!OperatingSystem.IsWindows()) Require(nativeExit == 137, "native SIGKILL status preserves 128 plus signal");
        foreach (var entry in new[] { "native-worker-partial", "native-worker-unread", "native-worker-live-failure", "native-worker-epipe" })
        {
            if (_selectedControl is not null && _selectedControl != $"delivery-{entry}") continue;
            using var listener = new TcpListener(IPAddress.Loopback, 0);
            listener.Start(1);
            var messages = new List<long>();
            var failures = new List<Exception>();
            var exits = 0;
            var liveFailure = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
            Worker? worker = null;
            try
            {
                JsEventLoop.Run(async () =>
                {
                    worker = new Worker(entry == "native-worker-epipe" ? "native-worker-unread" : entry, new WorkerOptions {
                        argv = new JSArray<string> { ((IPEndPoint)listener.LocalEndpoint).Port.ToString() },
                    });
                    worker.on<TsValue>("message", value => messages.Add(ExactInt64(value)));
                    worker.on<Exception>("error", error =>
                    {
                        failures.Add(error);
                        if (entry == "native-worker-live-failure") liveFailure.SetResult();
                    });
                    worker.on<int>("exit", code =>
                    {
                        Require(code == (entry == "native-worker-partial" ? nativeExit : 0),
                            "worker status matches its idiomatic native process status");
                        Require(messages.SequenceEqual(Enumerable.Range(0, 10).Select(value => (long)value)),
                            "all complete native frames precede the terminal event");
                        exits++;
                    });
                    using var control = listener.AcceptTcpClient();
                    control.ReceiveTimeout = 5000;
                    Require(control.GetStream().ReadByte() == 1, "native child reached its transport control boundary");
                    if (entry == "native-worker-partial")
                        Require(await worker.terminate() == nativeExit, "terminate resolves with the native process signal status");
                    else if (entry == "native-worker-live-failure")
                    {
                        await liveFailure.Task;
                        Require(exits == 0, "live native IO is delivered before the peer is released to exit");
                        Require(failures.Count == 1 && failures[0] is EndOfStreamException,
                            "a still-live truncated native transport keeps its precise IO error");
                        control.GetStream().WriteByte(1);
                    }
                    else
                    {
                        if (entry == "native-worker-epipe")
                        {
                            var nativeClient = NativeClient(NativeTransport(worker));
                            nativeClient.Client.Shutdown(SocketShutdown.Send);
                        }
                        for (var index = 0; index < 128; index++) worker.postMessage(TsValue.from(new string('x', 1024)));
                        control.GetStream().WriteByte(1);
                    }
                });
                Require(failures.Count == (entry == "native-worker-live-failure" ? 1 : 0),
                    "only a still-live native transport failure is delivered as an IO error");
                Require(exits == 1, "native worker terminal event is delivered once");
                var terminal = worker!.NativeTerminalSnapshot;
                var unread = terminal.Unread;
                Require(unread == (entry == "native-worker-epipe" ? 128UL : 0UL),
                    "only proven failed EPIPE writes are observable unread posts");
                Require(unread <= 128, "fixture ground truth bounds runtime observability");
                if (_selectedControl is not null)
                    Console.WriteLine($"CH08 native terminal {entry}: socket={terminal.Error}, phase={terminal.Phase}, peer-stopped={terminal.PeerStopped}, unread={unread}");
            }
            finally { worker?.Dispose(); }
        }
    }

    [UnsafeAccessor(UnsafeAccessorKind.Field, Name = "_transport")]
    private static extern ref WorkerTransport NativeTransport(Worker worker);

    [UnsafeAccessor(UnsafeAccessorKind.Field, Name = "_client")]
    private static extern ref TcpClient NativeClient(WorkerTransport transport);

    private static int DirectNativeKillStatus()
    {
        using var listener = new TcpListener(IPAddress.Loopback, 0);
        listener.Start(1);
        var executable = Environment.ProcessPath ?? throw new InvalidOperationException("native process path is available");
        var start = new System.Diagnostics.ProcessStartInfo(executable) { UseShellExecute = false };
        if (string.Equals(Path.GetFileNameWithoutExtension(executable), "dotnet", StringComparison.OrdinalIgnoreCase))
            start.ArgumentList.Add(Path.GetFullPath(Environment.GetCommandLineArgs()[0]));
        start.ArgumentList.Add("--native-signal-baseline");
        start.ArgumentList.Add(((IPEndPoint)listener.LocalEndpoint).Port.ToString());
        using var process = System.Diagnostics.Process.Start(start) ?? throw new InvalidOperationException("native baseline started");
        using var control = listener.AcceptTcpClient();
        control.ReceiveTimeout = 5000;
        Require(control.GetStream().ReadByte() == 1, "native baseline process is running before kill");
        process.Kill(entireProcessTree: true);
        process.WaitForExit();
        return process.ExitCode;
    }

    private static void ServePeer(TcpListener listener, byte[] token, Task firstSubscription,
        Task firstRetirement, Task secondSubscription)
    {
        using var client = listener.AcceptTcpClient();
        client.ReceiveTimeout = 10000;
        client.SendTimeout = 10000;
        using var stream = client.GetStream();
        var authentication = ReadFrame(stream);
        Require(authentication.Kind == 1 && CryptographicOperations.FixedTimeEquals(authentication.Payload, token),
            "native authentication frame and token");
        WriteFrame(stream, 2, EncodeInt64(InitialValue));
        WriteFrame(stream, 3, EncodeEnvironmentData());
        RequireInt64Frame(ReadFrame(stream), InitialValue + 2);
        WriteFrame(stream, 4, EncodeInt64(InitialValue + 3));
        firstSubscription.WaitAsync(TimeSpan.FromSeconds(10)).GetAwaiter().GetResult();
        WriteFrame(stream, 4, EncodeInt64(InitialValue + 4));
        RequireInt64Frame(ReadFrame(stream), InitialValue + 5);
        firstRetirement.WaitAsync(TimeSpan.FromSeconds(10)).GetAwaiter().GetResult();
        WriteFrame(stream, 4, EncodeInt64(InitialValue + 6));
        RequireInt64Frame(ReadFrame(stream), InitialValue + 7);
        secondSubscription.WaitAsync(TimeSpan.FromSeconds(10)).GetAwaiter().GetResult();
        WriteFrame(stream, 4, EncodeInt64(InitialValue + 8));
        var closed = ReadFrame(stream);
        Require(closed.Kind == 6 && closed.Payload.Length == 0, "native close frame");
    }

    private static byte[] EncodeEnvironmentData()
    {
        using var buffer = new MemoryStream();
        using var writer = new BinaryWriter(buffer, Encoding.UTF8, true);
        var key = Encoding.UTF8.GetBytes("native-key");
        var value = EncodeInt64(InitialValue + 1);
        writer.Write(1);
        writer.Write(key.Length);
        writer.Write(key);
        writer.Write(value.Length);
        writer.Write(value);
        return buffer.ToArray();
    }

    private static byte[] EncodeInt64(long value)
    {
        var payload = new byte[10];
        payload[0] = 1;
        payload[1] = 16;
        BinaryPrimitives.WriteInt64LittleEndian(payload.AsSpan(2), value);
        return payload;
    }

    private static void RequireInt64Frame((byte Kind, byte[] Payload) frame, long expected) =>
        Require(frame.Kind == 4 && frame.Payload.AsSpan().SequenceEqual(EncodeInt64(expected)),
            "exact native outgoing message frame");

    private static (byte Kind, byte[] Payload) ReadFrame(NetworkStream stream)
    {
        Span<byte> header = stackalloc byte[5];
        stream.ReadExactly(header);
        var length = BinaryPrimitives.ReadInt32BigEndian(header[1..]);
        Require(length is >= 0 and <= 1024, "probe frame size");
        var payload = new byte[length];
        stream.ReadExactly(payload);
        return (header[0], payload);
    }

    private static void WriteFrame(NetworkStream stream, byte kind, byte[] payload)
    {
        Span<byte> header = stackalloc byte[5];
        header[0] = kind;
        BinaryPrimitives.WriteInt32BigEndian(header[1..], payload.Length);
        stream.Write(header);
        stream.Write(payload);
        stream.Flush();
    }

    private static TsValue ReceiveEventually(MessagePort port)
    {
        var selected = TsValue.undefined();
        Require(SpinWait.SpinUntil(() =>
        {
            selected = worker_threads.receiveMessageOnPort(port);
            return selected.unwrap() is not null;
        }, TimeSpan.FromSeconds(10)), "native receive deadline");
        return selected;
    }

    private static long ExactInt64(TsValue value) => value.unwrap() is long result
        ? result : throw new InvalidOperationException("native int64 carrier changed");

    private static void RequireNoLoop()
    {
        try { JsEventLoop.Capture(); }
        catch (InvalidOperationException error) when (error.Message == "No JavaScript event loop is running on this thread.")
        {
            return;
        }
        throw new InvalidOperationException("native bootstrap unexpectedly has a JavaScript event loop");
    }

    private static void Require(bool condition, string label)
    {
        if (!condition) throw new InvalidOperationException(label);
    }
}
