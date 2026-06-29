using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

namespace Tsonic.CSharp.Node;

#pragma warning disable CS1591

#pragma warning disable IDE1006

public static class worker_threads
{
    public static bool isMainThread => true;
    public static int threadId => Environment.CurrentManagedThreadId;
    public static object? workerData => null;
    public static MessagePort? parentPort => null;

    public static void markAsUntransferable(object value)
    {
        _ = value;
    }

    public static bool isMarkedAsUntransferable(object value)
    {
        _ = value;
        return false;
    }

    public static string? getEnvironmentData(string key)
    {
        return Environment.GetEnvironmentVariable(key);
    }

    public static void setEnvironmentData(string key, string? value)
    {
        Environment.SetEnvironmentVariable(key, value);
    }
}

public sealed class Worker : EventEmitter
{
    private readonly CancellationTokenSource _cancellation = new();
    private readonly Task _task;

    public Worker(Action workerBody)
    {
        if (workerBody == null)
            throw new ArgumentNullException(nameof(workerBody));

        threadId = -1;
        _task = Task.Run(() =>
        {
            threadId = Environment.CurrentManagedThreadId;
            try
            {
                workerBody();
                emit("exit", 0);
            }
            catch (Exception ex)
            {
                emit("error", ex);
                emit("exit", 1);
            }
        }, _cancellation.Token);
    }

    public int threadId { get; private set; }

    public Task<int> terminate()
    {
        _cancellation.Cancel();
        return _task.ContinueWith(static task => task.IsFaulted ? 1 : 0, TaskScheduler.Default);
    }

    public void postMessage(object? value)
    {
        emit("message", value);
    }

    public void @ref()
    {
    }

    public void unref()
    {
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

public sealed class MessagePort : EventEmitter
{
    private readonly ConcurrentQueue<object?> _queue = new();
    private MessagePort? _peer;
    private bool _closed;

    internal void Connect(MessagePort peer)
    {
        _peer = peer;
    }

    public void postMessage(object? value)
    {
        if (_closed)
            throw new InvalidOperationException("MessagePort is closed.");

        if (_peer == null || _peer._closed)
            return;

        _peer._queue.Enqueue(value);
        _peer.emit("message", value);
    }

    public object? receiveMessageOnPort()
    {
        return _queue.TryDequeue(out var value) ? value : null;
    }

    public void start()
    {
    }

    public void close()
    {
        _closed = true;
        emit("close");
    }
}
