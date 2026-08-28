using System;
using System.Collections.Concurrent;
using System.Threading;
using Tsonic.CSharp.Js;

namespace Tsonic.CSharp.Node;

/// <summary>
/// Represents an immediate callback that can be used with setImmediate/clearImmediate.
/// </summary>
public class Immediate : IDisposable
{
    private const int StateScheduled = 0;
    private const int StateRunning = 1;
    private const int StateCompleted = 2;
    private const int StateCancelled = 3;

    private static int _nextHandleId = 0;
    private static readonly ConcurrentDictionary<int, Immediate> ActiveHandles = new();
    private readonly int _handleId;
    private readonly Action _callback;
    private int _state = StateScheduled;
    private int _cleanupState = 0;
    private bool _isRef = true;

    internal Immediate(Action callback)
    {
        _handleId = Interlocked.Increment(ref _nextHandleId);
        _callback = callback;
        ProcessKeepAlive.Acquire();
        ActiveHandles[_handleId] = this;
        Tsonic.CSharp.Js.JsEventLoop.EnqueueHandleOwned(TryExecute);
    }

    private void TryExecute()
    {
        try
        {
            if (Volatile.Read(ref _state) != StateScheduled)
            {
                return;
            }

            if (Interlocked.CompareExchange(ref _state, StateRunning, StateScheduled) != StateScheduled)
            {
                return;
            }

            _callback();
            Interlocked.Exchange(ref _state, StateCompleted);
        }
        finally
        {
            if (Volatile.Read(ref _state) is StateCompleted or StateCancelled)
            {
                Cleanup();
            }
        }
    }

    /// <summary>
    /// Requests that the Node.js event loop not exit so long as the Immediate is active.
    /// </summary>
    public Immediate @ref()
    {
        if (Volatile.Read(ref _cleanupState) == 0 && !_isRef)
        {
            ProcessKeepAlive.Acquire();
        }
        _isRef = true;
        return this;
    }

    /// <summary>
    /// Allows the Node.js event loop to exit if this is the only active handle.
    /// </summary>
    public Immediate unref()
    {
        if (Volatile.Read(ref _cleanupState) == 0 && _isRef)
        {
            ProcessKeepAlive.Release();
        }
        _isRef = false;
        return this;
    }

    /// <summary>
    /// Returns true if the immediate will keep the event loop active.
    /// </summary>
    public bool hasRef()
    {
        return _isRef;
    }

    /// <summary>
    /// Disposes the immediate resources.
    /// </summary>
    public void Dispose()
    {
        if (Interlocked.CompareExchange(ref _state, StateCancelled, StateScheduled) == StateScheduled)
        {
            Cleanup();
        }
        else if (Volatile.Read(ref _state) is StateCompleted or StateCancelled)
        {
            Cleanup();
        }
    }

    private void Cleanup()
    {
        if (Interlocked.Exchange(ref _cleanupState, 1) != 0)
        {
            return;
        }

        ActiveHandles.TryRemove(_handleId, out _);
        if (_isRef)
        {
            ProcessKeepAlive.Release();
        }
        GC.SuppressFinalize(this);
    }
}
