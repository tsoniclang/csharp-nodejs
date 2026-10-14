using Tsonic.CSharp.Js;
using Tsonic.CSharp.Runtime;

namespace Tsonic.CSharp.Node;

public sealed partial class MessagePort
{
    private CallbackOwner? _callbackOwner;

    /// <inheritdoc />
    protected override void OnListenerAdding(object eventName) => BindCallbackOwner();

    /// <inheritdoc />
    protected override void OnListenerAdded(object eventName) => StartTransportReader();

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
            ScheduleBufferedDelivery(owner);
        }
    }

    private void RetireCallbacks(CallbackOwner owner)
    {
        lock (_stateLock)
        {
            if (!ReferenceEquals(_callbackOwner, owner)) return;
            _callbackOwner = null;
            owner.Registration.Dispose();
            owner.Pending?.Clear();
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

    private void ScheduleDelivery(PortSignal signal, ulong ticket)
    {
        if (_callbackOwner is { } owner)
            AdmitDelivery(owner, new PortDelivery(signal, ticket));
    }

    private void ScheduleBufferedDelivery(CallbackOwner owner)
    {
        var messages = _queue.GetEnumerator();
        var errors = _errors?.GetEnumerator() ?? default;
        var hasMessage = messages.MoveNext();
        var hasError = _errors is not null && errors.MoveNext();
        while (hasMessage || hasError)
        {
            if (hasError && (!hasMessage || errors.Current.Ticket < messages.Current.Ticket))
            {
                AdmitDelivery(owner, new PortDelivery(PortSignal.Error, errors.Current.Ticket));
                hasError = errors.MoveNext();
            }
            else
            {
                AdmitDelivery(owner, new PortDelivery(PortSignal.Message, messages.Current.Ticket));
                hasMessage = messages.MoveNext();
            }
        }
        if (_terminalError is { } terminal)
            AdmitDelivery(owner, new PortDelivery(PortSignal.Error, terminal.Ticket));
        if (_closeTicket is { } closed)
            AdmitDelivery(owner, new PortDelivery(PortSignal.Close, closed));
    }

    private static void AdmitDelivery(CallbackOwner owner, PortDelivery delivery)
    {
        if (owner.Dispatch.TryEnqueue(static state =>
            {
                var selected = (CallbackOwner)state!;
                selected.Port.Deliver(selected);
            }, owner))
        {
            (owner.Pending ??= new()).Enqueue(delivery);
            return;
        }
        if (owner.Dispatch.IsActive)
            owner.Dispatch.ReportFailure(new InvalidOperationException(
                "Pending Node callback count exceeds the finite event-loop limit."));
    }

    private void Deliver(CallbackOwner owner)
    {
        string eventName;
        object? value;
        lock (_stateLock)
        {
            if (!ReferenceEquals(_callbackOwner, owner) || !owner.Dispatch.IsActive) return;
            if (owner.Pending is not { } pending || !pending.TryDequeue(out var delivery))
                throw new InvalidOperationException("MessagePort callback has no admitted delivery ticket.");
            switch (delivery.Signal)
            {
                case PortSignal.Message:
                    if (!_queue.TryPeek(out var message) || message.Ticket != delivery.Ticket) return;
                    _queue.Dequeue();
                    eventName = "message";
                    value = message.Value;
                    break;
                case PortSignal.Error:
                    eventName = "error";
                    if (_errors is { } errors && errors.TryPeek(out var error) && error.Ticket == delivery.Ticket)
                    {
                        _errors.Dequeue();
                        value = error.Error;
                    }
                    else if (_terminalError is { } terminal && terminal.Ticket == delivery.Ticket)
                    {
                        value = terminal.Error;
                        _terminalError = null;
                    }
                    else
                        throw new InvalidOperationException("MessagePort callback has no matching physical error.");
                    break;
                case PortSignal.Close:
                    if (_closeTicket != delivery.Ticket)
                        throw new InvalidOperationException("MessagePort callback has no matching physical close.");
                    _closeTicket = null;
                    eventName = "close";
                    value = null;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(delivery.Signal));
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
        }
    }

    private enum PortSignal { Message, Error, Close }
    private readonly record struct PortDelivery(PortSignal Signal, ulong Ticket);

    private sealed class CallbackOwner(MessagePort port, LoopDispatch dispatch, int threadId)
    {
        internal readonly MessagePort Port = port;
        internal readonly LoopDispatch Dispatch = dispatch;
        internal readonly int ThreadId = threadId;
        internal LoopRegistration Registration;
        internal bool Referenced;
        internal Queue<PortDelivery>? Pending;
    }
}
