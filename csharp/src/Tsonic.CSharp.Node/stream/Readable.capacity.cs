using System.Threading;
using System.Threading.Tasks;

namespace Tsonic.CSharp.Node;

public partial class Readable
{
    private TaskCompletionSource? _capacityWaiter;

    /// <summary>Waits until the finite readable buffer can accept more data.</summary>
    protected void WaitForReadCapacity(CancellationToken cancellationToken = default)
    {
        CapacityTask().WaitAsync(cancellationToken).GetAwaiter().GetResult();
    }

    /// <summary>Asynchronously waits until the finite readable buffer can accept more data.</summary>
    protected Task WaitForReadCapacityAsync(CancellationToken cancellationToken = default) =>
        CapacityTask().WaitAsync(cancellationToken);

    private Task CapacityTask()
    {
        lock (_readLock)
        {
            if (_destroyed || _bufferedSize < _highWaterMark)
                return Task.CompletedTask;
            _capacityWaiter ??= new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
            return _capacityWaiter.Task;
        }
    }

    private void BlockReadCapacityIfNeeded()
    {
        if (_bufferedSize < _highWaterMark || _destroyed)
            return;
        _capacityWaiter ??= new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
    }

    private void SignalReadCapacityIfAvailable()
    {
        if (!_destroyed && _bufferedSize >= _highWaterMark)
            return;
        var waiter = _capacityWaiter;
        _capacityWaiter = null;
        waiter?.TrySetResult();
    }
}
