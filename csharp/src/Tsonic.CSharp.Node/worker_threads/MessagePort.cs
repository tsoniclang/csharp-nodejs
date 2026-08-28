using System.Collections.Concurrent;
using Tsonic.CSharp.Js;
using Tsonic.CSharp.Runtime;

namespace Tsonic.CSharp.Node;

public sealed class MessagePort : EventEmitter, IDisposable
{
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
    }

    internal MessagePort(WorkerTransport transport)
    {
        _transport = transport;
        ProcessKeepAlive.Acquire();
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

    public TsValue receiveMessageOnPort()
    {
        if (!_queue.TryDequeue(out var value))
            return TsValue.undefined();
        Interlocked.Decrement(ref _queuedMessages);
        return value;
    }

    public void start() => StartTransportReader();

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
                if (transport is not null) ProcessKeepAlive.Release();
            }
        }
        try { transport?.Send(WorkerFrameKind.Close, ReadOnlySpan<byte>.Empty); }
        catch (IOException) { }
        catch (ObjectDisposedException) { }
        transport?.Dispose();
        Tsonic.CSharp.Js.JsEventLoop.EnqueueReferenced(() => emit("close"));
    }

    public MessagePort @ref()
    {
        lock (_stateLock)
        {
            if (!_closed && !_refed)
            {
                _refed = true;
                if (_transport is not null) ProcessKeepAlive.Acquire();
            }
        }
        return this;
    }

    public MessagePort unref()
    {
        lock (_stateLock)
        {
            if (_refed)
            {
                _refed = false;
                if (_transport is not null) ProcessKeepAlive.Release();
            }
        }
        return this;
    }

    public bool hasRef()
    {
        lock (_stateLock) return _refed;
    }

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
        EnqueuePortCallback(() =>
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
                        EnqueuePortCallback(() => emit(
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
            EnqueuePortCallback(() => emit("error", error));
            close();
        }
    }

    private void EnqueuePortCallback(Action callback)
    {
        lock (_stateLock)
        {
            if (_transport is not null)
            {
                Tsonic.CSharp.Js.JsEventLoop.EnqueueHandleOwned(callback);
                return;
            }
        }
        Tsonic.CSharp.Js.JsEventLoop.EnqueueReferenced(callback);
    }
}

public sealed class MessageChannel
{
    public MessageChannel()
    {
        port1 = new MessagePort();
        port2 = new MessagePort();
        port1.Connect(port2);
        port2.Connect(port1);
    }

    public MessagePort port1 { get; }
    public MessagePort port2 { get; }
}
