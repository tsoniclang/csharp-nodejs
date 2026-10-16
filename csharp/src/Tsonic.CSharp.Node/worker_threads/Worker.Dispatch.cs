using Tsonic.CSharp.Js;

namespace Tsonic.CSharp.Node;

public sealed partial class Worker
{
    private CallbackOwner? _callbackOwner;

    /// <inheritdoc />
    protected override void OnListenerAdding(object eventName) => BindCallbackOwner();

    /// <inheritdoc />
    protected override void OnListenerAdded(object eventName)
    {
        lock (_stateLock) ScheduleDelivery();
    }

    private void BindCallbackOwner()
    {
        var dispatch = BackgroundDispatch.Capture(NodePhases.Workers);
        var threadId = Environment.CurrentManagedThreadId;
        lock (_stateLock)
        {
            if (_callbackOwner is { } current)
            {
                if (current.Dispatch.IsActive)
                {
                    if (current.ThreadId != threadId)
                        throw new InvalidOperationException("Worker callbacks belong to another active event loop.");
                    return;
                }
                RetireCallbacks(current);
            }
            var owner = new CallbackOwner(this, dispatch, threadId);
            owner.Registration = dispatch.Register(static state =>
            {
                var selected = (CallbackOwner)state!;
                selected.Worker.RetireCallbacks(selected);
            }, owner);
            _callbackOwner = owner;
            AcquireReference();
            ScheduleDelivery();
        }
    }

    private void RetireCallbacks(CallbackOwner owner)
    {
        lock (_stateLock)
        {
            if (!ReferenceEquals(_callbackOwner, owner)) return;
            _callbackOwner = null;
            owner.Registration.Dispose();
            if (owner.Referenced)
            {
                owner.Referenced = false;
                owner.Dispatch.Release();
            }
            ClearNativeListeners();
            if (_nativeObservers != 0) ConsumeUnownedSignals();
            Monitor.PulseAll(_stateLock);
        }
    }

    private void AcquireReference()
    {
        if (_callbackOwner is not { Referenced: false } owner || !_refed ||
            _terminalConsumed || !owner.Dispatch.IsActive) return;
        owner.Dispatch.Acquire();
        owner.Referenced = true;
    }

    private void ReleaseReference()
    {
        if (_callbackOwner is not { Referenced: true } owner) return;
        owner.Referenced = false;
        owner.Dispatch.Release();
    }

    private void ScheduleDelivery()
    {
        if (_physicalFailure is { } failure && _callbackOwner is { } failedOwner && failedOwner.Dispatch.IsActive)
        {
            failedOwner.Dispatch.ReportFailure(failure);
            return;
        }
        if (_callbackOwner is not { Scheduled: false } owner || !owner.Dispatch.IsActive ||
            (_signals.Count == 0 && _readerTerminal is null &&
                (!_exitCode.HasValue || _terminalConsumed))) return;
        if (owner.Dispatch.TryEnqueue(static state =>
            {
                var selected = (CallbackOwner)state!;
                selected.Worker.Deliver(selected);
            }, owner))
        {
            owner.Scheduled = true;
            return;
        }
        if (owner.Dispatch.IsActive)
            owner.Dispatch.ReportFailure(new InvalidOperationException(
                "Pending Node callback count exceeds the finite event-loop limit."));
    }

    private void Deliver(CallbackOwner owner)
    {
        WorkerSignal? signal = null;
        int? exitCode = null;
        lock (_stateLock)
        {
            if (!ReferenceEquals(_callbackOwner, owner) || !owner.Dispatch.IsActive) return;
            owner.Scheduled = false;
            if (_signals.TryDequeue(out var next)) signal = next;
            else if (_readerTerminal is { } terminal)
            {
                _readerTerminal = null;
                signal = terminal;
            }
            else if (_exitCode.HasValue && !_terminalConsumed)
            {
                _terminalConsumed = true;
                exitCode = _exitCode;
            }
            if (signal is { } selected && SuppressesNativeIo(selected)) signal = null;
            Monitor.PulseAll(_stateLock);
        }
        try
        {
            if (signal is { } value)
            {
                switch (value.Kind)
                {
                    case WorkerSignalKind.Message:
                        emit("message", value.Value);
                        break;
                    case WorkerSignalKind.InternalError:
                        owner.Dispatch.ReportFailure(value.Error!);
                        break;
                    default:
                        emit("error", value.Error);
                        break;
                }
            }
            else if (exitCode.HasValue) emit("exit", exitCode.Value);
        }
        finally
        {
            lock (_stateLock)
            {
                if (exitCode.HasValue && ReferenceEquals(_callbackOwner, owner)) ReleaseReference();
                ScheduleDelivery();
            }
        }
    }

    private sealed class CallbackOwner(Worker worker, LoopDispatch dispatch, int threadId)
    {
        internal readonly Worker Worker = worker;
        internal readonly LoopDispatch Dispatch = dispatch;
        internal readonly int ThreadId = threadId;
        internal LoopRegistration Registration;
        internal bool Referenced;
        internal bool Scheduled;
    }
}
