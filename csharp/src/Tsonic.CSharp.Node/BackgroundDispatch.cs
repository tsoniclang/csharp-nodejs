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
    private static readonly SemaphoreSlim WorkReservations = new(MaximumPendingWork, MaximumPendingWork);
    private static readonly System.Threading.Channels.Channel<WorkItem> Work =
        System.Threading.Channels.Channel.CreateBounded<WorkItem>(
        new BoundedChannelOptions(MaximumPendingWork)
        {
            FullMode = BoundedChannelFullMode.Wait,
            SingleReader = false,
            SingleWriter = false
        });
    private static readonly Task[] Workers = CreateWorkers();

    public static Task RunReferenced(Action action)
    {
        ArgumentNullException.ThrowIfNull(action);
        ProcessKeepAlive.Acquire();
        try
        {
            return Enqueue(() =>
            {
                try
                {
                    action();
                    return Task.CompletedTask;
                }
                finally
                {
                    ProcessKeepAlive.Release();
                }
            });
        }
        catch
        {
            ProcessKeepAlive.Release();
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
        ProcessKeepAlive.Acquire();
        try
        {
            return Enqueue(async () =>
            {
                try
                {
                    await action().ConfigureAwait(false);
                }
                finally
                {
                    ProcessKeepAlive.Release();
                }
            });
        }
        catch
        {
            ProcessKeepAlive.Release();
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

    private static Task Enqueue(Func<Task> action)
    {
        _ = Workers;
        if (!WorkReservations.Wait(0))
            throw new InvalidOperationException("Node background work count exceeds its finite limit.");

        var completion = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        if (!Work.Writer.TryWrite(new WorkItem(action, completion)))
        {
            WorkReservations.Release();
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
        await foreach (var item in Work.Reader.ReadAllAsync().ConfigureAwait(false))
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
            WorkReservations.Release();
        }
    }

    private sealed record WorkItem(Func<Task> Action, TaskCompletionSource Completion);
}
