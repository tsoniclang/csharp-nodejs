using System.Globalization;
using System.Net;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text;
using Tsonic.CSharp.Runtime;

namespace Tsonic.CSharp.Node;

/// <summary>Provides the Node-compatible worker-thread module surface.</summary>
public static class worker_threads
{
    internal const string WorkerArgumentMarker = "--tsonic-node-worker-v1";
    internal const string WorkerArgumentDelimiter = "--";
    internal const int AuthenticationTokenBytes = 32;
    private static readonly object EnvironmentDataLock = new();
    private static readonly Dictionary<string, byte[]> MainEnvironmentData =
        new(StringComparer.Ordinal);
    private static readonly ConditionalWeakTable<object, object> UntransferableValues = new();
    private static readonly object UntransferableMarker = new();
    private static WorkerProcessContext? _context;

    /// <summary>Reports whether the current process is the main worker context.</summary>
    public static bool isMainThread => Volatile.Read(ref _context) is null;

    /// <summary>Gets the current worker identity, or zero for the main context.</summary>
    public static int threadId => Volatile.Read(ref _context)?.ThreadId ?? 0;

    /// <summary>Gets the current worker's structured-cloned startup value.</summary>
    public static TsValue workerData =>
        Volatile.Read(ref _context)?.WorkerData ?? TsValue.undefined();

    /// <summary>Gets the current worker's parent message port.</summary>
    public static MessagePort? parentPort => Volatile.Read(ref _context)?.ParentPort;

    /// <summary>Synchronously receives the next queued message from a port.</summary>
    public static TsValue receiveMessageOnPort(MessagePort port)
    {
        ArgumentNullException.ThrowIfNull(port);
        return port.receiveMessageOnPort();
    }

    /// <summary>Gets a structured-cloned environment value by key.</summary>
    public static TsValue getEnvironmentData(string key)
    {
        ArgumentNullException.ThrowIfNull(key);
        var values = CurrentEnvironmentData();
        lock (EnvironmentDataLock)
        {
            return values.TryGetValue(key, out var payload)
                ? StructuredClone.Decode(payload)
                : TsValue.undefined();
        }
    }

    /// <summary>Stores a structured-cloned environment value by key.</summary>
    public static void setEnvironmentData(string key, TsValue value)
    {
        ArgumentNullException.ThrowIfNull(key);
        var payload = StructuredClone.Encode(value);
        var values = CurrentEnvironmentData();
        lock (EnvironmentDataLock)
        {
            values[key] = payload;
        }
    }

    /// <summary>Marks a reference value as unavailable for transfer.</summary>
    public static void markAsUntransferable(TsValue value)
    {
        var identity = TransferIdentity(value);
        lock (UntransferableValues)
        {
            UntransferableValues.Remove(identity);
            UntransferableValues.Add(identity, UntransferableMarker);
        }
    }

    /// <summary>Reports whether a reference value was marked unavailable for transfer.</summary>
    public static bool isMarkedAsUntransferable(TsValue value)
    {
        var identity = TransferIdentity(value);
        lock (UntransferableValues)
        {
            return UntransferableValues.TryGetValue(identity, out _);
        }
    }

    /// <summary>Initializes a worker process from the closed bootstrap protocol, or returns null for a main process.</summary>
    public static string? InitializeWorkerProcess(string[] arguments)
    {
        ArgumentNullException.ThrowIfNull(arguments);
        if (arguments.Length == 0 ||
            !string.Equals(arguments[0], WorkerArgumentMarker, StringComparison.Ordinal))
        {
            return null;
        }
        if (arguments.Length < 7 ||
            !string.Equals(arguments[6], WorkerArgumentDelimiter, StringComparison.Ordinal))
        {
            throw new InvalidOperationException("Worker bootstrap arguments do not match the closed protocol.");
        }
        var entryIdentity = arguments[1];
        if (entryIdentity.Length == 0)
            throw new InvalidOperationException("Worker bootstrap entry identity is empty.");
        if (!int.TryParse(arguments[2], NumberStyles.None, CultureInfo.InvariantCulture, out var port) ||
            port is < IPEndPoint.MinPort or > IPEndPoint.MaxPort)
        {
            throw new InvalidOperationException("Worker bootstrap port is invalid.");
        }
        byte[] token;
        try
        {
            token = Convert.FromBase64String(arguments[3]);
        }
        catch (FormatException error)
        {
            throw new InvalidOperationException("Worker bootstrap authentication token is invalid.", error);
        }
        if (token.Length != AuthenticationTokenBytes)
            throw new InvalidOperationException("Worker bootstrap authentication token has an invalid size.");
        if (!int.TryParse(arguments[4], NumberStyles.None, CultureInfo.InvariantCulture, out var selectedThreadId) ||
            selectedThreadId <= 0)
        {
            throw new InvalidOperationException("Worker bootstrap thread identity is invalid.");
        }
        var workerName = arguments[5];
        if (workerName.Length > 0 && Thread.CurrentThread.Name is null)
            Thread.CurrentThread.Name = workerName;

        var client = new TcpClient(AddressFamily.InterNetwork);
        try
        {
            client.ConnectAsync(IPAddress.Loopback, port)
                .WaitAsync(TimeSpan.FromSeconds(30))
                .GetAwaiter()
                .GetResult();
            var transport = new WorkerTransport(client);
            transport.Send(WorkerFrameKind.Authenticate, token);
            var workerDataFrame = transport.Receive();
            var environmentDataFrame = transport.Receive();
            if (workerDataFrame.Kind != WorkerFrameKind.WorkerData ||
                environmentDataFrame.Kind != WorkerFrameKind.EnvironmentData)
            {
                transport.Dispose();
                throw new InvalidDataException("Worker bootstrap frames are out of order.");
            }
            var data = StructuredClone.Decode(workerDataFrame.Payload);
            var environmentData = DecodeEnvironmentData(environmentDataFrame.Payload);
            var portValue = new MessagePort(transport);
            var context = new WorkerProcessContext(
                selectedThreadId,
                data,
                portValue,
                environmentData);
            if (Interlocked.CompareExchange(ref _context, context, null) is not null)
            {
                portValue.Dispose();
                throw new InvalidOperationException("Worker bootstrap context was initialized more than once.");
            }
            process.argv = [
                process.execPath,
                entryIdentity,
                .. arguments.Skip(7),
            ];
            InstallUnhandledErrorForwarding(transport);
            portValue.StartTransportReader();
            return entryIdentity;
        }
        catch
        {
            client.Dispose();
            throw;
        }
    }

    internal static byte[] EncodeEnvironmentDataSnapshot()
    {
        var values = CurrentEnvironmentData();
        lock (EnvironmentDataLock)
        {
            using var stream = new MemoryStream();
            using var writer = new BinaryWriter(stream, Encoding.UTF8, leaveOpen: true);
            writer.Write(values.Count);
            foreach (var entry in values.OrderBy(entry => entry.Key, StringComparer.Ordinal))
            {
                WriteBoundedString(writer, entry.Key);
                writer.Write(entry.Value.Length);
                writer.Write(entry.Value);
            }
            writer.Flush();
            if (stream.Length > WorkerTransport.MaximumFrameBytes)
                throw new InvalidOperationException("Worker environment-data snapshot exceeds the finite transport limit.");
            return stream.ToArray();
        }
    }

    private static Dictionary<string, byte[]> DecodeEnvironmentData(ReadOnlySpan<byte> payload)
    {
        using var stream = new MemoryStream(payload.ToArray(), writable: false);
        using var reader = new BinaryReader(stream, Encoding.UTF8, leaveOpen: true);
        var count = reader.ReadInt32();
        if (count < 0 || count > 1 << 20)
            throw new InvalidDataException("Worker environment-data count is outside the finite limit.");
        var result = new Dictionary<string, byte[]>(count, StringComparer.Ordinal);
        for (var index = 0; index < count; index++)
        {
            var key = ReadBoundedString(reader);
            var length = reader.ReadInt32();
            if (length < 0 || length > WorkerTransport.MaximumFrameBytes - stream.Position)
                throw new InvalidDataException("Worker environment-data payload length is invalid.");
            var value = reader.ReadBytes(length);
            if (value.Length != length)
                throw new EndOfStreamException("Worker environment-data payload is truncated.");
            if (!result.TryAdd(key, value))
                throw new InvalidDataException("Worker environment-data snapshot contains a duplicate key.");
        }
        if (stream.Position != stream.Length)
            throw new InvalidDataException("Worker environment-data snapshot contains trailing data.");
        return result;
    }

    private static Dictionary<string, byte[]> CurrentEnvironmentData() =>
        Volatile.Read(ref _context)?.EnvironmentData ?? MainEnvironmentData;

    private static object TransferIdentity(TsValue value)
    {
        var unwrapped = value.unwrap();
        return unwrapped is null or Undefined || unwrapped.GetType().IsValueType || unwrapped is string
            ? throw new TypeError("markAsUntransferable requires a closed reference value.")
            : unwrapped;
    }

    private static void InstallUnhandledErrorForwarding(WorkerTransport transport)
    {
        AppDomain.CurrentDomain.UnhandledException += (_, eventArgs) =>
        {
            var message = eventArgs.ExceptionObject is Exception error
                ? error.ToString()
                : Convert.ToString(eventArgs.ExceptionObject, CultureInfo.InvariantCulture) ??
                    "Worker terminated with an unknown error.";
            try
            {
                transport.Send(WorkerFrameKind.Error, Encoding.UTF8.GetBytes(message));
            }
            catch (Exception sendError) when (
                sendError is IOException or ObjectDisposedException or SocketException)
            {
            }
        };
    }

    private static void WriteBoundedString(BinaryWriter writer, string value)
    {
        var bytes = Encoding.UTF8.GetBytes(value);
        if (bytes.Length > 1 << 24)
            throw new InvalidOperationException("Worker environment-data key exceeds the finite string limit.");
        writer.Write(bytes.Length);
        writer.Write(bytes);
    }

    private static string ReadBoundedString(BinaryReader reader)
    {
        var length = reader.ReadInt32();
        if (length < 0 || length > 1 << 24)
            throw new InvalidDataException("Worker environment-data key length is invalid.");
        var bytes = reader.ReadBytes(length);
        if (bytes.Length != length)
            throw new EndOfStreamException("Worker environment-data key is truncated.");
        return Encoding.UTF8.GetString(bytes);
    }

    private sealed record WorkerProcessContext(
        int ThreadId,
        TsValue WorkerData,
        MessagePort ParentPort,
        Dictionary<string, byte[]> EnvironmentData);
}
