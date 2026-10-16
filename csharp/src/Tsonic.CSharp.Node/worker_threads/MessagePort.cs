using Tsonic.CSharp.Runtime;

namespace Tsonic.CSharp.Node;

/// <summary>Represents one endpoint of a structured-clone worker message channel.</summary>
public sealed partial class MessagePort : EventEmitter, IDisposable
{
    private const int MaximumQueuedMessages = 1 << 16;
    private readonly Queue<PortMessage> _queue = new();
    private readonly object _stateLock = new();
    private Queue<PortError>? _errors;
    private PortError? _terminalError;
    private ulong _nextTicket;
    private MessagePort? _peer;
    private WorkerTransport? _transport;
    private bool _readerStarted;
    private bool _closed;
    private ulong? _closeTicket;
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
        MessagePort? peer;
        lock (_stateLock)
        {
            if (_closed)
                throw new InvalidOperationException("MessagePort is closed.");
            if (_transport is { } transport)
            {
                transport.Send(WorkerFrameKind.Message, StructuredClone.Encode(value));
                return;
            }
            peer = _peer;
        }
        peer?.ReceiveLocal(StructuredClone.Encode(value));
    }

    /// <summary>Synchronously receives the next queued message, or undefined when none is available.</summary>
    public TsValue receiveMessageOnPort()
    {
        lock (_stateLock)
            return _queue.TryDequeue(out var message) ? message.Value : TsValue.undefined();
    }

    /// <summary>Starts delivery of messages from the underlying transport.</summary>
    public void start()
    {
        BindCallbackOwner();
        StartTransportReader();
    }

    /// <summary>Closes this endpoint and releases its transport resources.</summary>
    public void close()
    {
        WorkerTransport? transport;
        lock (_stateLock)
        {
            if (_closed) return;
            _closed = true;
            _closeTicket = NextTicket();
            transport = _transport;
            _transport = null;
            _refed = false;
            ScheduleDelivery(PortSignal.Close, _closeTicket.Value);
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
            RequireQueueCapacity();
            var message = new PortMessage(NextTicket(), StructuredClone.Decode(payload));
            _queue.Enqueue(message);
            ScheduleDelivery(PortSignal.Message, message.Ticket);
        }
    }

    private void ReceiveError(Exception error)
    {
        lock (_stateLock)
        {
            if (_closed) return;
            RequireQueueCapacity();
            QueueError(error);
        }
    }

    private void RequireQueueCapacity()
    {
        if (_queue.Count + (_errors?.Count ?? 0) == MaximumQueuedMessages)
            throw new InvalidOperationException("MessagePort queue exceeds the finite message limit.");
        if (_nextTicket >= ulong.MaxValue - 2)
            throw new InvalidOperationException("MessagePort signal sequence is exhausted.");
    }

    private ulong NextTicket() => checked(++_nextTicket);

    private void QueueError(Exception error)
    {
        var selected = new PortError(NextTicket(), error);
        if (_queue.Count + (_errors?.Count ?? 0) < MaximumQueuedMessages)
            (_errors ??= new()).Enqueue(selected);
        else
            _terminalError = selected;
        ScheduleDelivery(PortSignal.Error, selected.Ticket);
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
                if (transport.Receive() is not { } frame)
                {
                    close();
                    return;
                }
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
                    QueueError(error);
                }
            }
            close();
        }
    }

    private readonly record struct PortMessage(ulong Ticket, TsValue Value);
    private readonly record struct PortError(ulong Ticket, Exception Error);

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
