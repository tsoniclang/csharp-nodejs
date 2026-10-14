using System;
using System.Collections.Concurrent;
using System.Threading;
using Tsonic.CSharp.Js;

namespace Tsonic.CSharp.Node;

/// <summary>
/// Represents a timeout that can be used with setTimeout/clearTimeout.
/// </summary>
public class Timeout : IDisposable
{
    private Tsonic.CSharp.Js.LoopDispatch _ownerDispatch;
    private Tsonic.CSharp.Js.LoopRegistration _loopRegistration;
    private Tsonic.CSharp.Js.LoopDispatch TimerDispatch => BackgroundDispatch.Bind(ref _ownerDispatch, NodePhases.Timers);

    private static int _nextHandleId = 0;
    private static readonly ConcurrentDictionary<int, Timeout> ActiveHandles = new();
    private static readonly SendOrPostCallback DispatchCallback = static state => ((Timeout)state!).InvokeCallback();

    private readonly int _handleId;
    private readonly Timer _timer;
    private readonly Action _callback;
    private readonly int _delay;
    private readonly int _period;
    private readonly object _gate = new();
    private bool _isRef = true;
    private bool _disposed = false;

    internal Timeout(Action callback, int delay, int period = System.Threading.Timeout.Infinite)
    {
        _handleId = Interlocked.Increment(ref _nextHandleId);
        _callback = callback;
        _delay = delay;
        _period = period;
        var dispatch = TimerDispatch;
        _loopRegistration = dispatch.Register(static state => ((Timeout)state!).Dispose(), this);
        dispatch.Acquire();
        Timer? timer = null;
        try
        {
            _timer = timer = new Timer(static state => ((Timeout)state!).Execute(), this, System.Threading.Timeout.Infinite, System.Threading.Timeout.Infinite);
            ActiveHandles[_handleId] = this;
            _timer.Change(_delay, _period);
        }
        catch
        {
            ActiveHandles.TryRemove(_handleId, out _);
            dispatch.Release();
            _loopRegistration.Dispose();
            timer?.Dispose();
            throw;
        }
    }

    private void Execute()
    {
        lock (_gate)
        {
            if (_disposed)
            {
                return;
            }

            if (_period == System.Threading.Timeout.Infinite)
            {
                _timer.Change(System.Threading.Timeout.Infinite, System.Threading.Timeout.Infinite);
            }
        }

        BackgroundDispatch.Enqueue(TimerDispatch, DispatchCallback, this);
    }

    private void InvokeCallback()
    {
        lock (_gate)
        {
            if (_disposed)
                return;
        }
        try
        {
            _callback();
        }
        finally
        {
            if (_period == System.Threading.Timeout.Infinite)
                Dispose();
        }
    }

    /// <summary>
    /// Requests that the Node.js event loop not exit so long as the Timeout is active.
    /// </summary>
    public Timeout @ref()
    {
        if (!_disposed && !_isRef)
        {
            TimerDispatch.Acquire();
        }
        _isRef = true;
        return this;
    }

    /// <summary>
    /// Allows the Node.js event loop to exit if this is the only active handle.
    /// </summary>
    public Timeout unref()
    {
        if (!_disposed && _isRef)
        {
            TimerDispatch.Release();
        }
        _isRef = false;
        return this;
    }

    /// <summary>
    /// Returns true if the timer will keep the event loop active.
    /// </summary>
    public bool hasRef()
    {
        return _isRef;
    }

    /// <summary>
    /// Restarts the timer, as if it was just created.
    /// </summary>
    public Timeout refresh()
    {
        lock (_gate)
        {
            if (!_disposed)
            {
                _timer.Change(_delay, _period);
            }
        }
        return this;
    }

    /// <summary>
    /// Cancels the timeout (alias for clearTimeout).
    /// </summary>
    public void close()
    {
        Dispose();
    }

    /// <summary>
    /// Disposes the timer resources.
    /// </summary>
    public void Dispose()
    {
        lock (_gate)
        {
            if (!_disposed)
            {
                _disposed = true;
                ActiveHandles.TryRemove(_handleId, out _);
                _timer.Dispose();
                if (_isRef)
                {
                    TimerDispatch.Release();
                }
            }
        }
        _loopRegistration.Dispose();
        GC.SuppressFinalize(this);
    }
}
