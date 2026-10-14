using Tsonic.CSharp.Runtime;

namespace Tsonic.CSharp.Node;

/// <summary>Represents one endpoint of a structured-clone worker message channel.</summary>
public sealed partial class MessagePort : EventEmitter, IDisposable
{
    private const int MaximumQueuedMessages = 1 << 16;
    private readonly Queue<TsValue> _queue = new();
    private readonly object _stateLock = new();
    private Queue<Exception>? _errors;
    private Exception? _terminalError;
    private MessagePort? _peer;
    private WorkerTransport? _transport;
    private bool _readerStarted;
    private bool _closed;
    private bool _closePending;
    private bool _refed = true;

    internal MessagePort()
    {
    }

    internal MessagePort(WorkerTransport transport)
    {
        _transport = transport;
    }

    internal void Connect(MessagePort peer) => _peer = peer;

    internal void StartTransportReader()
    {
        lock (_stateLock)
        {
            if (_readerStarted || _transport is null || _closed) return;
            _readerStarted = true;
        }
        _ = Task.Run(ReadTransport);
    }

    /// <summary>Sends a structured-cloned value to the connected endpoint.</summary>
    public void postMessage(TsValue value)
    {
        WorkerTransport? transport;
        MessagePort? peer;
        lock (_stateLock)
        {
            if (_closed)
                throw new InvalidOperationException("MessagePort is closed.");
            transport = _transport;
            peer = _peer;
        }
        var payload = StructuredClone.Encode(value);
        if (transport is not null) transport.Send(WorkerFrameKind.Message, payload);
        else peer?.ReceiveLocal(payload);
    }

    /// <summary>Synchronously receives the next queued message, or undefined when none is available.</summary>
    public TsValue receiveMessageOnPort()
    {
        lock (_stateLock)
            return _queue.TryDequeue(out var value) ? value : TsValue.undefined();
    }

    /// <summary>Starts delivery of messages from the underlying transport.</summary>
    public void start()
    {
        BindCallbackOwner();
        StartTransportReader();
        lock (_stateLock) ScheduleDelivery();
    }

    /// <summary>Closes this endpoint and releases its transport resources.</summary>
    public void close()
    {
        WorkerTransport? transport;
        lock (_stateLock)
        {
            if (_closed) return;
            _closed = true;
            _closePending = true;
            transport = _transport;
            _transport = null;
            _refed = false;
            ScheduleDelivery();
            ReleaseReference();
        }
        try { transport?.Send(WorkerFrameKind.Close, ReadOnlySpan<byte>.Empty); }
        catch (IOException) { }
        catch (ObjectDisposedException) { }
        transport?.Dispose();
    }

    /// <summary>Keeps the process alive while this port remains open.</summary>
    public MessagePort @ref()
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

    /// <summary>Allows the process to exit while this port remains open.</summary>
    public MessagePort unref()
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

    /// <summary>Reports whether this port currently keeps the process alive.</summary>
    public bool hasRef()
    {
        lock (_stateLock) return _refed;
    }

    /// <summary>Closes the port and releases its transport resources.</summary>
    public void Dispose() => close();

    private void ReceiveLocal(byte[] payload)
    {
        lock (_stateLock)
        {
            if (_closed)
                throw new InvalidOperationException("MessagePort is closed.");
            if (_queue.Count + (_errors?.Count ?? 0) == MaximumQueuedMessages)
                throw new InvalidOperationException("MessagePort queue exceeds the finite message limit.");
            _queue.Enqueue(StructuredClone.Decode(payload));
            ScheduleDelivery();
        }
    }

    private void ReceiveError(Exception error)
    {
        lock (_stateLock)
        {
            if (_closed) return;
            if (_queue.Count + (_errors?.Count ?? 0) == MaximumQueuedMessages)
                throw new InvalidOperationException("MessagePort queue exceeds the finite message limit.");
            (_errors ??= new()).Enqueue(error);
            ScheduleDelivery();
        }
    }

    private void ReadTransport()
    {
        WorkerTransport? transport;
        lock (_stateLock) transport = _transport;
        if (transport is null) return;
        try
        {
            while (true)
            {
                var frame = transport.Receive();
                switch (frame.Kind)
                {
                    case WorkerFrameKind.Message:
                        ReceiveLocal(frame.Payload);
                        break;
                    case WorkerFrameKind.Error:
                        ReceiveError(new InvalidOperationException(
                            System.Text.Encoding.UTF8.GetString(frame.Payload)));
                        break;
                    case WorkerFrameKind.Close:
                        close();
                        return;
                    default:
                        throw new InvalidDataException(
                            $"Unexpected worker message-port frame '{frame.Kind}'.");
                }
            }
        }
        catch (Exception error) when (
            error is IOException or ObjectDisposedException or InvalidDataException or InvalidOperationException)
        {
            lock (_stateLock)
            {
                if (!_closed)
                {
                    if (_queue.Count + (_errors?.Count ?? 0) < MaximumQueuedMessages)
                        (_errors ??= new()).Enqueue(error);
                    else
                        _terminalError = error;
                }
            }
            close();
        }
    }

}

/// <summary>Creates two locally connected message-port endpoints.</summary>
public sealed class MessageChannel
{
    /// <summary>Creates a connected pair of message ports.</summary>
    public MessageChannel()
    {
        port1 = new MessagePort();
        port2 = new MessagePort();
        port1.Connect(port2);
        port2.Connect(port1);
    }

    /// <summary>Gets the first endpoint.</summary>
    public MessagePort port1 { get; }
    /// <summary>Gets the second endpoint.</summary>
    public MessagePort port2 { get; }
}
