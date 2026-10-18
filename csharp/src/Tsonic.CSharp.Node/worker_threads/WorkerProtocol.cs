using System.Buffers.Binary;
using System.Net.Sockets;

namespace Tsonic.CSharp.Node;

internal enum WorkerFrameKind : byte
{
    Authenticate = 1,
    WorkerData = 2,
    EnvironmentData = 3,
    Message = 4,
    Error = 5,
    Close = 6,
}

internal readonly record struct WorkerFrame(WorkerFrameKind Kind, byte[] Payload);

internal enum WorkerFramePhase { Boundary, Header, Payload }

internal sealed class WorkerTransport : IDisposable
{
    public const int MaximumFrameBytes = 64 * 1024 * 1024;
    private readonly TcpClient _client;
    private readonly NetworkStream _stream;
    private readonly object _writeLock = new();
    private bool _disposed;
    private WorkerFramePhase _readPhase;

    internal WorkerFramePhase ReadPhase => _readPhase;

    public WorkerTransport(TcpClient client)
    {
        _client = client;
        _stream = client.GetStream();
    }

    public void Send(WorkerFrameKind kind, ReadOnlySpan<byte> payload)
    {
        if (payload.Length > MaximumFrameBytes)
            throw new InvalidOperationException("Worker frame exceeds the finite transport limit.");

        Span<byte> header = stackalloc byte[5];
        header[0] = (byte)kind;
        BinaryPrimitives.WriteInt32BigEndian(header[1..], payload.Length);
        lock (_writeLock)
        {
            ObjectDisposedException.ThrowIf(_disposed, this);
            _stream.Write(header);
            _stream.Write(payload);
            _stream.Flush();
        }
    }

    public WorkerFrame? Receive() => ReadFrame(_stream, out _readPhase);

    internal static WorkerFrame? ReadFrame(System.IO.Stream stream, out WorkerFramePhase phase)
    {
        phase = WorkerFramePhase.Boundary;
        Span<byte> header = stackalloc byte[5];
        var received = stream.Read(header);
        if (received == 0) return null;
        phase = WorkerFramePhase.Header;
        stream.ReadExactly(header[received..]);
        var length = BinaryPrimitives.ReadInt32BigEndian(header[1..]);
        if (length < 0 || length > MaximumFrameBytes)
            throw new InvalidDataException("Worker frame length is outside the finite transport contract.");
        var payload = new byte[length];
        phase = WorkerFramePhase.Payload;
        stream.ReadExactly(payload);
        return new WorkerFrame((WorkerFrameKind)header[0], payload);
    }

    internal static SocketError? NativeSocketError(Exception error) => error switch
    {
        SocketException socket => socket.SocketErrorCode,
        IOException { InnerException: SocketException socket } => socket.SocketErrorCode,
        _ => null,
    };

    internal static bool IsPeerStopped(Exception error) =>
        NativeSocketError(error) is SocketError.ConnectionReset or SocketError.Shutdown;

    internal static bool IsBrokenPipe(Exception error) =>
        NativeSocketError(error) is SocketError.Shutdown;

    public void Dispose()
    {
        lock (_writeLock)
        {
            if (_disposed) return;
            _disposed = true;
            _stream.Dispose();
            _client.Dispose();
        }
    }
}
