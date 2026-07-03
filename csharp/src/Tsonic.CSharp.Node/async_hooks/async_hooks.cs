using System;
using System.Collections.Generic;
using System.Threading;

namespace Tsonic.CSharp.Node;

#pragma warning disable CS1591

#pragma warning disable IDE1006

public static class async_hooks
{
    private static long _nextAsyncId = 1;
    private static readonly AsyncLocal<long> CurrentAsyncId = new();
    private static readonly AsyncLocal<long> CurrentTriggerAsyncId = new();

    public static AsyncHook createHook(AsyncHookCallbacks callbacks)
    {
        return new AsyncHook(callbacks);
    }

    public static long executionAsyncId()
    {
        return CurrentAsyncId.Value;
    }

    public static long executionAsyncResource()
    {
        return executionAsyncId();
    }

    public static long triggerAsyncId()
    {
        return CurrentTriggerAsyncId.Value;
    }

    internal static long NextAsyncId()
    {
        return Interlocked.Increment(ref _nextAsyncId);
    }

    internal static IDisposable Enter(long asyncId, long triggerAsyncId)
    {
        var previousAsyncId = CurrentAsyncId.Value;
        var previousTriggerAsyncId = CurrentTriggerAsyncId.Value;
        CurrentAsyncId.Value = asyncId;
        CurrentTriggerAsyncId.Value = triggerAsyncId;
        return new Scope(() =>
        {
            CurrentAsyncId.Value = previousAsyncId;
            CurrentTriggerAsyncId.Value = previousTriggerAsyncId;
        });
    }

    private sealed class Scope : IDisposable
    {
        private readonly Action _dispose;
        private bool _disposed;

        public Scope(Action dispose)
        {
            _dispose = dispose;
        }

        public void Dispose()
        {
            if (_disposed)
                return;

            _disposed = true;
            _dispose();
        }
    }
}

public sealed class AsyncHookCallbacks
{
    public Action<long, string, long, object?>? init { get; set; }
    public Action<long>? before { get; set; }
    public Action<long>? after { get; set; }
    public Action<long>? destroy { get; set; }
    public Action<long>? promiseResolve { get; set; }
}

public sealed class AsyncHook
{
    private readonly AsyncHookCallbacks _callbacks;

    public AsyncHook(AsyncHookCallbacks callbacks)
    {
        _callbacks = callbacks ?? throw new ArgumentNullException(nameof(callbacks));
    }

    public bool enabled { get; private set; }

    public AsyncHook enable()
    {
        enabled = true;
        return this;
    }

    public AsyncHook disable()
    {
        enabled = false;
        return this;
    }

    internal void EmitInit(long asyncId, string type, long triggerAsyncId, object? resource)
    {
        if (enabled)
            _callbacks.init?.Invoke(asyncId, type, triggerAsyncId, resource);
    }

    internal void EmitBefore(long asyncId)
    {
        if (enabled)
            _callbacks.before?.Invoke(asyncId);
    }

    internal void EmitAfter(long asyncId)
    {
        if (enabled)
            _callbacks.after?.Invoke(asyncId);
    }

    internal void EmitDestroy(long asyncId)
    {
        if (enabled)
            _callbacks.destroy?.Invoke(asyncId);
    }
}

public sealed class AsyncResource
{
    private readonly long _asyncId;
    private readonly long _triggerAsyncId;
    private bool _destroyed;

    public AsyncResource(string type, object? options = null)
    {
        this.type = string.IsNullOrWhiteSpace(type) ? throw new ArgumentException("Async resource type is required.", nameof(type)) : type;
        _asyncId = async_hooks.NextAsyncId();
        _triggerAsyncId = async_hooks.executionAsyncId();
        _ = options;
    }

    public string type { get; }

    public long asyncId()
    {
        return _asyncId;
    }

    public long triggerAsyncId()
    {
        return _triggerAsyncId;
    }

    public T runInAsyncScope<T>(Func<T> callback)
    {
        if (callback == null)
            throw new ArgumentNullException(nameof(callback));

        using (async_hooks.Enter(_asyncId, _triggerAsyncId))
        {
            return callback();
        }
    }

    public void runInAsyncScope(Action callback)
    {
        if (callback == null)
            throw new ArgumentNullException(nameof(callback));

        using (async_hooks.Enter(_asyncId, _triggerAsyncId))
        {
            callback();
        }
    }

    public void emitDestroy()
    {
        _destroyed = true;
    }

    public bool destroyed => _destroyed;
}

public sealed class AsyncLocalStorage<T>
{
    private readonly AsyncLocal<T?> _storage = new();

    public T? getStore()
    {
        return _storage.Value;
    }

    public void enterWith(T store)
    {
        _storage.Value = store;
    }

    public R run<R>(T store, Func<R> callback)
    {
        if (callback == null)
            throw new ArgumentNullException(nameof(callback));

        var previous = _storage.Value;
        _storage.Value = store;
        try
        {
            return callback();
        }
        finally
        {
            _storage.Value = previous;
        }
    }

    public void run(T store, Action callback)
    {
        if (callback == null)
            throw new ArgumentNullException(nameof(callback));

        var previous = _storage.Value;
        _storage.Value = store;
        try
        {
            callback();
        }
        finally
        {
            _storage.Value = previous;
        }
    }

    public void disable()
    {
        _storage.Value = default;
    }
}
