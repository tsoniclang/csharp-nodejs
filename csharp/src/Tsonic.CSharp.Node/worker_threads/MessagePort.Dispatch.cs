using Tsonic.CSharp.Js;
using Tsonic.CSharp.Runtime;

namespace Tsonic.CSharp.Node;

public sealed partial class MessagePort
{
    private CallbackOwner? _callbackOwner;

    /// <inheritdoc />
    protected override void OnListenerAdding(object eventName) => BindCallbackOwner();

    /// <inheritdoc />
    protected override void OnListenerAdded(object eventName)
    {
        StartTransportReader();
        lock (_stateLock) ScheduleDelivery();
    }

    private void BindCallbackOwner()
    {
        var dispatch = BackgroundDispatch.Capture(NodePhases.Ports);
        var threadId = Environment.CurrentManagedThreadId;
        lock (_stateLock)
        {
            if (_callbackOwner is { } current)
            {
                if (current.Dispatch.IsActive)
                {
                    if (current.ThreadId != threadId)
                        throw new InvalidOperationException("MessagePort callbacks belong to another active event loop.");
                    return;
                }
                RetireCallbacks(current);
            }

            var owner = new CallbackOwner(this, dispatch, threadId);
            owner.Registration = dispatch.Register(static state =>
            {
                var selected = (CallbackOwner)state!;
                selected.Port.RetireCallbacks(selected);
            }, owner);
            _callbackOwner = owner;
            AcquireReference();
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
        }
    }

    private void AcquireReference()
    {
        if (_callbackOwner is not { Referenced: false } owner || !_refed ||
            _transport is null || !owner.Dispatch.IsActive)
            return;
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
        if (_callbackOwner is not { } owner) return;
        while (owner.PendingMessages < _queue.Count)
        {
            if (!AdmitDelivery(owner, static state =>
                {
                    var selected = (CallbackOwner)state!;
                    selected.Port.Deliver(selected, PortSignal.Message);
                })) return;
            owner.PendingMessages++;
        }
        var errors = (_errors?.Count ?? 0) + (_terminalError is null ? 0 : 1);
        while (owner.PendingErrors < errors)
        {
            if (!AdmitDelivery(owner, static state =>
                {
                    var selected = (CallbackOwner)state!;
                    selected.Port.Deliver(selected, PortSignal.Error);
                })) return;
            owner.PendingErrors++;
        }
        if (_closePending && !owner.PendingClose)
        {
            if (!AdmitDelivery(owner, static state =>
                {
                    var selected = (CallbackOwner)state!;
                    selected.Port.Deliver(selected, PortSignal.Close);
                })) return;
            owner.PendingClose = true;
        }
    }

    private static bool AdmitDelivery(CallbackOwner owner, SendOrPostCallback callback)
    {
        if (owner.Dispatch.TryEnqueue(callback, owner)) return true;
        if (owner.Dispatch.IsActive)
            owner.Dispatch.ReportFailure(new InvalidOperationException(
                "Pending Node callback count exceeds the finite event-loop limit."));
        return false;
    }

    private void Deliver(CallbackOwner owner, PortSignal signal)
    {
        string eventName;
        object? value;
        lock (_stateLock)
        {
            if (!ReferenceEquals(_callbackOwner, owner) || !owner.Dispatch.IsActive) return;
            switch (signal)
            {
                case PortSignal.Message:
                    owner.PendingMessages--;
                    if (!_queue.TryDequeue(out var message)) return;
                    eventName = "message";
                    value = message;
                    break;
                case PortSignal.Error:
                    owner.PendingErrors--;
                    eventName = "error";
                    if (_errors is { } errors && errors.TryDequeue(out var error)) value = error;
                    else if (_terminalError is { } terminalError)
                    {
                        value = terminalError;
                        _terminalError = null;
                    }
                    else return;
                    break;
                case PortSignal.Close:
                    owner.PendingClose = false;
                    if (!_closePending) return;
                    _closePending = false;
                    eventName = "close";
                    value = null;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(signal));
            }
        }
        try
        {
            if (eventName == "close") emit(eventName);
            else emit(eventName, value);
        }
        finally
        {
            if (eventName == "close") RetireCallbacks(owner);
            else lock (_stateLock) ScheduleDelivery();
        }
    }

    private enum PortSignal { Message, Error, Close }

    private sealed class CallbackOwner(MessagePort port, LoopDispatch dispatch, int threadId)
    {
        internal readonly MessagePort Port = port;
        internal readonly LoopDispatch Dispatch = dispatch;
        internal readonly int ThreadId = threadId;
        internal LoopRegistration Registration;
        internal bool Referenced;
        internal int PendingMessages;
        internal int PendingErrors;
        internal bool PendingClose;
    }
}
