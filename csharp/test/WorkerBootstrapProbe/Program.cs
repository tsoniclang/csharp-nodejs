using System.Buffers.Binary;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;
using Tsonic.CSharp.Js;
using Tsonic.CSharp.Node;
using Tsonic.CSharp.Runtime;

internal static class Program
{
    private const long InitialValue = 9_007_199_254_740_993L;

    private static void Main()
    {
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
        Console.WriteLine("{\"public_native_bootstrap\":true,\"exact_int64\":true," +
            "\"preloop_transport\":true,\"retained_transport\":true,\"fresh_epoch\":true}");
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
