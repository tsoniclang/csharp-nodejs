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
    private static int _nextHandleId = 0;
    private static readonly ConcurrentDictionary<int, Timeout> ActiveHandles = new();

    private readonly int _handleId;
    private readonly ManualResetEventSlim _cancelSignal;
    private Thread? _timeoutThread;
    private readonly Action _callback;
    private readonly int _delay;
    private readonly int _period;
    private bool _isRef = true;
    private bool _disposed = false;

    internal Timeout(Action callback, int delay, int period = System.Threading.Timeout.Infinite)
    {
        _handleId = Interlocked.Increment(ref _nextHandleId);
        _callback = callback;
        _delay = delay;
        _period = period;
        _cancelSignal = new ManualResetEventSlim(false);
        ProcessKeepAlive.Acquire();

        _timeoutThread = new Thread(Run)
        {
            IsBackground = true,
            Name = period == System.Threading.Timeout.Infinite
                ? "Tsonic.CSharp.Node.Timeout"
                : "Tsonic.CSharp.Node.Interval",
        };
        _timeoutThread.Start();

        ActiveHandles[_handleId] = this;
    }

    private void Run()
    {
        if (_cancelSignal.Wait(_delay))
        {
            return;
        }

        while (!_disposed)
        {
            Execute();

            if (_period == System.Threading.Timeout.Infinite)
            {
                return;
            }

            if (_cancelSignal.Wait(_period))
            {
                return;
            }
        }
    }

    private void Execute()
    {
        if (!_disposed)
        {
            try
            {
                _callback();
            }
            finally
            {
                if (_period == System.Threading.Timeout.Infinite)
                {
                    Dispose();
                }
            }
        }
    }

    /// <summary>
    /// Requests that the Node.js event loop not exit so long as the Timeout is active.
    /// In this C# implementation, this is a no-op for compatibility.
    /// </summary>
    public Timeout @ref()
    {
        if (!_disposed && !_isRef)
        {
            ProcessKeepAlive.Acquire();
        }
        _isRef = true;
        return this;
    }

    /// <summary>
    /// Allows the Node.js event loop to exit if this is the only active handle.
    /// In this C# implementation, this is a no-op for compatibility.
    /// </summary>
    public Timeout unref()
    {
        if (!_disposed && _isRef)
        {
            ProcessKeepAlive.Release();
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
        if (!_disposed)
        {
            // Note: Cannot truly reset a Timer, would need to track original delay
            // For now, this is a no-op
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
        if (!_disposed)
        {
            _disposed = true;
            ActiveHandles.TryRemove(_handleId, out _);
            _cancelSignal.Set();
            if (_isRef)
            {
                ProcessKeepAlive.Release();
            }
        }
        GC.SuppressFinalize(this);
    }
}
