using System;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using Tsonic.CSharp.Js;

namespace Tsonic.CSharp.Node;

internal static class BackgroundDispatch
{
    private const int WorkerCount = 4;
    private const int MaximumPendingWork = 16 * 1024;

    internal static LoopDispatch Capture(PhaseRegistration phase) => JsEventLoop.RegisterPhase(phase.Phase, NodePhases.Order);
    internal static LoopDispatch Bind(ref LoopDispatch dispatch, PhaseRegistration phase) => dispatch.IsBound ? dispatch : dispatch = Capture(phase);

    internal static void Enqueue(LoopDispatch dispatch, Action callback)
    {
        if (!dispatch.TryEnqueue(callback) && dispatch.IsActive)
            dispatch.ReportFailure(new InvalidOperationException("Pending Node callback count exceeds the finite event-loop limit."));
    }

    internal static void Enqueue(LoopDispatch dispatch, SendOrPostCallback callback, object? state)
    {
        if (!dispatch.TryEnqueue(callback, state) && dispatch.IsActive)
            dispatch.ReportFailure(new InvalidOperationException("Pending Node callback count exceeds the finite event-loop limit."));
    }

    public static Task RunReferenced(Action action)
    {
        ArgumentNullException.ThrowIfNull(action);
        var dispatch = Capture(NodePhases.Background);
        dispatch.Acquire();
        try
        {
            return Enqueue(() =>
            {
                action();
                return Task.CompletedTask;
            }, dispatch);
        }
        catch
        {
            dispatch.Release();
            throw;
        }
    }

    public static Task<TResult> RunReferenced<TResult>(Func<TResult> action)
    {
        ArgumentNullException.ThrowIfNull(action);
        var completion = new TaskCompletionSource<TResult>(TaskCreationOptions.RunContinuationsAsynchronously);
        try
        {
            _ = RunReferenced(() =>
            {
                try
                {
                    completion.TrySetResult(action());
                }
                catch (OperationCanceledException error)
                {
                    completion.TrySetCanceled(error.CancellationToken);
                }
                catch (Exception error)
                {
                    completion.TrySetException(error);
                }
            });
        }
        catch (Exception error)
        {
            completion.TrySetException(error);
        }

        return completion.Task;
    }

    public static Task RunReferencedAsync(Func<Task> action)
    {
        ArgumentNullException.ThrowIfNull(action);
        var dispatch = Capture(NodePhases.Background);
        dispatch.Acquire();
        try
        {
            return Enqueue(action, dispatch);
        }
        catch
        {
            dispatch.Release();
            throw;
        }
    }

    public static Task RunHandleOwned(Action action)
    {
        ArgumentNullException.ThrowIfNull(action);
        return Enqueue(() =>
        {
            action();
            return Task.CompletedTask;
        });
    }

    public static Task RunHandleOwnedAsync(Func<Task> action)
    {
        ArgumentNullException.ThrowIfNull(action);
        return Enqueue(action);
    }

    private static Task Enqueue(Func<Task> action, LoopDispatch reference = default)
    {
        _ = WorkPool.Workers;
        if (!WorkPool.Reservations.Wait(0))
            throw new InvalidOperationException("Node background work count exceeds its finite limit.");

        var completion = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        if (!WorkPool.Work.Writer.TryWrite(new WorkItem(action, completion, reference)))
        {
            WorkPool.Reservations.Release();
            throw new InvalidOperationException("Node background work queue exceeds its finite limit.");
        }
        return completion.Task;
    }

    private static Task[] CreateWorkers()
    {
        var workers = new Task[WorkerCount];
        for (var index = 0; index < workers.Length; index++)
            workers[index] = Task.Run(WorkerLoop);
        return workers;
    }

    private static async Task WorkerLoop()
    {
        await foreach (var item in WorkPool.Work.Reader.ReadAllAsync().ConfigureAwait(false))
            _ = Execute(item);
    }

    private static async Task Execute(WorkItem item)
    {
        try
        {
            await item.Action().ConfigureAwait(false);
            item.Completion.TrySetResult();
        }
        catch (OperationCanceledException error)
        {
            item.Completion.TrySetCanceled(error.CancellationToken);
        }
        catch (Exception error)
        {
            item.Completion.TrySetException(error);
        }
        finally
        {
            if (item.Reference.IsBound) item.Reference.Release();
            WorkPool.Reservations.Release();
        }
    }

    private sealed record WorkItem(Func<Task> Action, TaskCompletionSource Completion, LoopDispatch Reference);

    private static class WorkPool
    {
        internal static readonly SemaphoreSlim Reservations = new(MaximumPendingWork, MaximumPendingWork);
        internal static readonly System.Threading.Channels.Channel<WorkItem> Work = System.Threading.Channels.Channel.CreateBounded<WorkItem>(new BoundedChannelOptions(MaximumPendingWork)
        {
            FullMode = BoundedChannelFullMode.Wait,
            SingleReader = false,
            SingleWriter = false,
        });
        internal static readonly Task[] Workers = CreateWorkers();
    }
}

internal static class NodePhases
{
    internal static readonly PhaseRegistration Background = new();
    internal static readonly PhaseRegistration RuntimeTasks = new();
    internal static readonly PhaseRegistration Signals = new();
    internal static readonly PhaseRegistration Timers = new();
    internal static readonly PhaseRegistration Http = new();
    internal static readonly PhaseRegistration Net = new();
    internal static readonly PhaseRegistration Tls = new();
    internal static readonly PhaseRegistration Watchers = new();
    internal static readonly PhaseRegistration Workers = new();
    internal static readonly PhaseRegistration Ports = new();
    internal static readonly DispatchPhase[] Order = [
        JsEventLoop.TimerPhase, Background.Phase, RuntimeTasks.Phase, Signals.Phase,
        Timers.Phase, Http.Phase, Net.Phase, Tls.Phase, Watchers.Phase, Workers.Phase, Ports.Phase,
    ];
}
