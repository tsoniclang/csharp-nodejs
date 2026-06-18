using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using Tsonic.CSharp.Js;

namespace Tsonic.CSharp.Node;

/// <summary>
/// Represents an immediate callback that can be used with setImmediate/clearImmediate.
/// </summary>
public class Immediate : IDisposable
{
    private const int DispatchGraceMilliseconds = 10;
    private const int StateScheduled = 0;
    private const int StateRunning = 1;
    private const int StateCompleted = 2;
    private const int StateCancelled = 3;

    private static int _nextHandleId = 0;
    private static readonly ConcurrentDictionary<int, Immediate> ActiveHandles = new();
    private static readonly ConcurrentQueue<Immediate> PendingHandles = new();
    private static int _dispatchScheduled = 0;

    private readonly int _handleId;
    private readonly Action _callback;
    private readonly CancellationTokenSource _cancellation = new();
    private readonly long _readyAfterTick;
    private int _state = StateScheduled;
    private int _cleanupState = 0;
    private bool _isRef = true;

    internal Immediate(Action callback)
    {
        _handleId = Interlocked.Increment(ref _nextHandleId);
        _callback = callback;
        _readyAfterTick = Environment.TickCount64 + DispatchGraceMilliseconds;
        ProcessKeepAlive.Acquire();
        ActiveHandles[_handleId] = this;
        PendingHandles.Enqueue(this);
        ScheduleDispatch();
    }

    private static void ScheduleDispatch()
    {
        if (Interlocked.CompareExchange(ref _dispatchScheduled, 1, 0) != 0)
        {
            return;
        }

        _ = BackgroundDispatch.RunAsync(DispatchPendingAsync, "Tsonic.CSharp.Node.Immediate.dispatch");
    }

    private static async Task DispatchPendingAsync()
    {
        while (true)
        {
            await Task.Delay(1);

            var batchCount = PendingHandles.Count;
            for (var index = 0; index < batchCount; index++)
            {
                if (!PendingHandles.TryDequeue(out var handle))
                {
                    break;
                }

                handle.TryExecute();
            }

            Interlocked.Exchange(ref _dispatchScheduled, 0);
            if (PendingHandles.IsEmpty)
            {
                return;
            }

            if (Interlocked.CompareExchange(ref _dispatchScheduled, 1, 0) != 0)
            {
                return;
            }
        }
    }

    private void TryExecute()
    {
        try
        {
            if (Volatile.Read(ref _state) != StateScheduled)
            {
                return;
            }

            if (Environment.TickCount64 < _readyAfterTick)
            {
                PendingHandles.Enqueue(this);
                return;
            }

            if (Interlocked.CompareExchange(ref _state, StateRunning, StateScheduled) != StateScheduled)
            {
                return;
            }

            if (_cancellation.IsCancellationRequested)
            {
                Interlocked.Exchange(ref _state, StateCancelled);
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
    /// In this C# implementation, this is a no-op for compatibility.
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
    /// In this C# implementation, this is a no-op for compatibility.
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
            _cancellation.Cancel();
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
        _cancellation.Dispose();
        GC.SuppressFinalize(this);
    }
}
