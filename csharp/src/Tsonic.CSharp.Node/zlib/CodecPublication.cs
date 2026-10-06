using System;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Sources;
using Tsonic.CSharp.Js;

namespace Tsonic.CSharp.Node;

internal sealed class CodecPublication : IValueTaskSource<bool>, IDisposable
{
    private readonly object _sync = new();
    private readonly Func<byte[], bool> _publish;
    private readonly Action<Exception> _failure;
    private readonly Func<CancellationToken, Task> _capacity;
    private readonly CancellationToken _stopping;
    private readonly CancellationTokenRegistration _registration;
    private ManualResetValueTaskSourceCore<bool> _completion = new() { RunContinuationsAsynchronously = true };
    private bool _pending;
    private bool _publishing;

    public CodecPublication(
        Func<byte[], bool> publish,
        Action<Exception> failure,
        Func<CancellationToken, Task> capacity,
        CancellationToken stopping)
    {
        _publish = publish;
        _failure = failure;
        _capacity = capacity;
        _stopping = stopping;
        _registration = stopping.Register(Cancel);
    }

    [System.Runtime.CompilerServices.AsyncMethodBuilder(typeof(System.Runtime.CompilerServices.PoolingAsyncValueTaskMethodBuilder))]
    public async ValueTask Publish(byte[] bytes)
    {
        short version;
        lock (_sync)
        {
            _stopping.ThrowIfCancellationRequested();
            if (_publishing)
                throw new InvalidOperationException("Codec output publication overlaps its preceding acknowledgement.");
            _completion.Reset();
            version = _completion.Version;
            _pending = true;
            _publishing = true;
        }
        try
        {
            JsEventLoop.EnqueueReferenced(() =>
            {
                if (_stopping.IsCancellationRequested)
                    return;
                try
                {
                    Complete(version, _publish(bytes), null);
                }
                catch (Exception error)
                {
                    try
                    {
                        _failure(error);
                    }
                    finally
                    {
                        Complete(version, false, error);
                    }
                }
            });
        }
        catch (Exception error)
        {
            Complete(version, false, error);
        }
        try
        {
            var accepted = await new ValueTask<bool>(this, version).ConfigureAwait(false);
            if (!accepted)
                await _capacity(_stopping).ConfigureAwait(false);
        }
        finally
        {
            lock (_sync)
                _publishing = false;
        }
    }

    private void Complete(short version, bool accepted, Exception? error)
    {
        lock (_sync)
        {
            if (!_pending || version != _completion.Version)
                return;
            _pending = false;
            if (error is null)
                _completion.SetResult(accepted);
            else
                _completion.SetException(error);
        }
    }

    private void Cancel()
    {
        lock (_sync)
        {
            if (!_pending)
                return;
            _pending = false;
            _completion.SetException(new OperationCanceledException(_stopping));
        }
    }

    bool IValueTaskSource<bool>.GetResult(short token) => _completion.GetResult(token);
    ValueTaskSourceStatus IValueTaskSource<bool>.GetStatus(short token) => _completion.GetStatus(token);
    void IValueTaskSource<bool>.OnCompleted(Action<object?> continuation, object? state, short token, ValueTaskSourceOnCompletedFlags flags) =>
        _completion.OnCompleted(continuation, state, token, flags);

    public void Dispose() => _registration.Dispose();
}
