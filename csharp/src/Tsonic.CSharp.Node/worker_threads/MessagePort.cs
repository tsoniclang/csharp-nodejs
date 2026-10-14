using System.Collections.Concurrent;
using Tsonic.CSharp.Js;
using Tsonic.CSharp.Runtime;

namespace Tsonic.CSharp.Node;

/// <summary>Represents one endpoint of a structured-clone worker message channel.</summary>
public sealed class MessagePort : EventEmitter, IDisposable
{
    private Tsonic.CSharp.Js.LoopDispatch _ownerDispatch;
    private Tsonic.CSharp.Js.LoopRegistration _loopRegistration;
    private Tsonic.CSharp.Js.LoopDispatch PortDispatch => BackgroundDispatch.Bind(ref _ownerDispatch, NodePhases.Ports);

    private const int MaximumQueuedMessages = 1 << 16;
    private readonly ConcurrentQueue<TsValue> _queue = new();
    private readonly object _stateLock = new();
    private MessagePort? _peer;
    private WorkerTransport? _transport;
    private int _queuedMessages;
    private bool _readerStarted;
    private bool _closed;
    private bool _refed = true;

    internal MessagePort()
    {
        _loopRegistration = PortDispatch.Register(static state => ((MessagePort)state!).close(), this);
    }

    internal MessagePort(WorkerTransport transport)
    {
        _transport = transport;
        _loopRegistration = PortDispatch.Register(static state => ((MessagePort)state!).close(), this);
        PortDispatch.Acquire();
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
        lock (_stateLock)
        {
            if (_closed)
                throw new InvalidOperationException("MessagePort is closed.");
            var payload = StructuredClone.Encode(value);
            if (_transport is not null)
            {
                _transport.Send(WorkerFrameKind.Message, payload);
                return;
            }
            _peer?.ReceiveLocal(payload);
        }
    }

    /// <summary>Synchronously receives the next queued message, or undefined when none is available.</summary>
    public TsValue receiveMessageOnPort()
    {
        if (!_queue.TryDequeue(out var value))
            return TsValue.undefined();
        Interlocked.Decrement(ref _queuedMessages);
        return value;
    }

    /// <summary>Starts delivery of messages from the underlying transport.</summary>
    public void start() => StartTransportReader();

    /// <summary>Closes this endpoint and releases its transport resources.</summary>
    public void close()
    {
        WorkerTransport? transport;
        lock (_stateLock)
        {
            if (_closed) return;
            _closed = true;
            transport = _transport;
            _transport = null;
            if (_refed)
            {
                _refed = false;
                if (transport is not null) PortDispatch.Release();
            }
        }
        try { transport?.Send(WorkerFrameKind.Close, ReadOnlySpan<byte>.Empty); }
        catch (IOException) { }
        catch (ObjectDisposedException) { }
        transport?.Dispose();
        _loopRegistration.Dispose();
        BackgroundDispatch.Enqueue(PortDispatch, () => emit("close"));
    }

    /// <summary>Keeps the process alive while this port remains open.</summary>
    public MessagePort @ref()
    {
        lock (_stateLock)
        {
            if (!_closed && !_refed)
            {
                _refed = true;
                if (_transport is not null) PortDispatch.Acquire();
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
                if (_transport is not null) PortDispatch.Release();
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
        if (Interlocked.Increment(ref _queuedMessages) > MaximumQueuedMessages)
        {
            Interlocked.Decrement(ref _queuedMessages);
            throw new InvalidOperationException("MessagePort queue exceeds the finite message limit.");
        }
        TsValue value;
        try
        {
            value = StructuredClone.Decode(payload);
            _queue.Enqueue(value);
        }
        catch
        {
            Interlocked.Decrement(ref _queuedMessages);
            throw;
        }
        BackgroundDispatch.Enqueue(PortDispatch, () =>
        {
            if (_queue.TryDequeue(out var selected))
            {
                Interlocked.Decrement(ref _queuedMessages);
                emit("message", selected);
            }
        });
    }

    private void ReadTransport()
    {
        try
        {
            while (true)
            {
                var frame = _transport?.Receive() ?? default;
                switch (frame.Kind)
                {
                    case WorkerFrameKind.Message:
                        ReceiveLocal(frame.Payload);
                        break;
                    case WorkerFrameKind.Error:
                        BackgroundDispatch.Enqueue(PortDispatch, () => emit(
                            "error",
                            new InvalidOperationException(
                                System.Text.Encoding.UTF8.GetString(frame.Payload))));
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
            error is IOException or ObjectDisposedException or InvalidDataException)
        {
            BackgroundDispatch.Enqueue(PortDispatch, () => emit("error", error));
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
