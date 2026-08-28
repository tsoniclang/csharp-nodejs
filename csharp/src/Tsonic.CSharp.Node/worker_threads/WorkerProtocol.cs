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

internal sealed class WorkerTransport : IDisposable
{
    public const int MaximumFrameBytes = 64 * 1024 * 1024;
    private readonly TcpClient _client;
    private readonly NetworkStream _stream;
    private readonly object _writeLock = new();
    private bool _disposed;

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

    public WorkerFrame Receive()
    {
        var header = new byte[5];
        _stream.ReadExactly(header);
        var length = BinaryPrimitives.ReadInt32BigEndian(header.AsSpan(1));
        if (length < 0 || length > MaximumFrameBytes)
            throw new InvalidDataException("Worker frame length is outside the finite transport contract.");
        var payload = new byte[length];
        _stream.ReadExactly(payload);
        return new WorkerFrame((WorkerFrameKind)header[0], payload);
    }

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
