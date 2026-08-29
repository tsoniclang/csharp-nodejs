using System;
using System.Collections.Generic;
using System.Text;

namespace Tsonic.CSharp.Node;

internal sealed class WritableState
{
    private sealed class WriteRequest
    {
        public required object? Chunk { get; init; }
        public required string? Encoding { get; init; }
        public required Action? Callback { get; init; }
        public required int Size { get; init; }
        public bool Completed { get; set; }
    }

    private readonly Queue<WriteRequest> _buffer = new();
    private readonly object _sync = new();
    private readonly Action<object?, string?, Action> _write;
    private readonly Action<Action> _final;
    private readonly Action<string> _emit;
    private readonly int _highWaterMark;
    private long _bufferedSize;
    private bool _corked;
    private bool _destroyed;
    private bool _draining;
    private bool _ended;
    private bool _finalized;
    private bool _finalCompleted;
    private bool _needsDrain;
    private bool _writing;

    public WritableState(
        Action<object?, string?, Action> write,
        Action<Action> final,
        Action<string> emit,
        int highWaterMark = 64 * 1024)
    {
        if (highWaterMark <= 0)
            throw new ArgumentOutOfRangeException(nameof(highWaterMark));

        _write = write;
        _final = final;
        _emit = emit;
        _highWaterMark = highWaterMark;
    }

    public bool Writable { get { lock (_sync) return !_ended && !_destroyed; } }
    public bool Ended { get { lock (_sync) return _ended; } }
    public bool Destroyed { get { lock (_sync) return _destroyed; } }
    public long BufferedSize { get { lock (_sync) return _bufferedSize; } }
    public bool Corked { get { lock (_sync) return _corked; } }

    public bool Write(object? chunk, string? encoding, Action? callback)
    {
        bool accepted;
        lock (_sync)
        {
            if (_ended || _destroyed)
                throw new InvalidOperationException("write after end");
            EnqueueWrite(chunk, encoding, callback);
            accepted = _bufferedSize < _highWaterMark;
        }
        DrainWrites();
        return accepted;
    }

    public void End(object? chunk, string? encoding, Action? callback)
    {
        lock (_sync)
        {
            if (_ended)
                throw new InvalidOperationException("end after end");

            if (chunk is not null)
                EnqueueWrite(chunk, encoding, null);

            _ended = true;
            _corked = false;
            if (callback is not null)
                RegisterFinishCallback(callback);
        }
        DrainWrites();
    }

    public void Cork()
    {
        lock (_sync)
        {
            if (_ended || _destroyed)
                throw new InvalidOperationException("cork after end");
            _corked = true;
        }
    }

    public void Uncork()
    {
        lock (_sync)
            _corked = false;
        DrainWrites();
    }

    public void Destroy()
    {
        lock (_sync)
        {
            if (_destroyed)
                return;

            _destroyed = true;
            _buffer.Clear();
            _bufferedSize = 0;
        }
    }

    private Action<Action>? FinishCallbackRegistrar { get; set; }

    public void SetFinishCallbackRegistrar(Action<Action> registrar)
    {
        FinishCallbackRegistrar = registrar;
    }

    private void RegisterFinishCallback(Action callback)
    {
        var registrar = FinishCallbackRegistrar
            ?? throw new InvalidOperationException("Writable finish callback registrar was not configured.");
        registrar(callback);
    }

    private void DrainWrites()
    {
        WriteRequest? request = null;
        var finalize = false;
        lock (_sync)
        {
            if (_draining || _destroyed || _corked || _writing)
                return;
            if (_buffer.Count > 0)
            {
                request = _buffer.Dequeue();
                _writing = true;
                _draining = true;
            }
            else if (_ended && !_finalized)
            {
                _finalized = true;
                _draining = true;
                finalize = true;
            }
            else
            {
                return;
            }
        }

        if (request is not null)
        {
            try
            {
                _write(request.Chunk, request.Encoding, () => CompleteWrite(request));
            }
            catch
            {
                lock (_sync)
                {
                    _writing = false;
                    _draining = false;
                }
                throw;
            }
            bool completedSynchronously;
            lock (_sync)
            {
                _draining = false;
                completedSynchronously = !_writing;
            }
            if (completedSynchronously)
                DrainWrites();
            return;
        }

        if (!finalize)
            return;
        try
        {
            _final(CompleteFinal);
        }
        finally
        {
            lock (_sync)
                _draining = false;
        }
    }

    private void CompleteWrite(WriteRequest request)
    {
        Action? callback;
        var emitDrain = false;
        lock (_sync)
        {
            if (request.Completed)
                throw new InvalidOperationException("Writable write callback was invoked more than once.");

            request.Completed = true;
            _writing = false;
            if (_destroyed)
                return;
            _bufferedSize = checked(_bufferedSize - request.Size);
            callback = request.Callback;

            if (_needsDrain && _bufferedSize < _highWaterMark)
            {
                _needsDrain = false;
                emitDrain = true;
            }
        }
        try
        {
            callback?.Invoke();
            if (emitDrain)
                _emit("drain");
        }
        finally
        {
            DrainWrites();
        }
    }

    private void CompleteFinal()
    {
        lock (_sync)
        {
            if (_finalCompleted)
                throw new InvalidOperationException("Writable final callback was invoked more than once.");
            _finalCompleted = true;
            if (_destroyed)
                return;
        }
        _emit("finish");
    }

    private void EnqueueWrite(object? chunk, string? encoding, Action? callback)
    {
        var size = ChunkSize(chunk, encoding);
        _buffer.Enqueue(new WriteRequest
        {
            Chunk = chunk,
            Encoding = encoding,
            Callback = callback,
            Size = size,
        });
        _bufferedSize = checked(_bufferedSize + size);
        if (_bufferedSize >= _highWaterMark)
            _needsDrain = true;
    }

    private static int ChunkSize(object? chunk, string? encoding)
    {
        return chunk switch
        {
            null => 0,
            Buffer buffer => buffer.length,
            byte[] bytes => bytes.Length,
            string text => Encoding.GetEncoding(encoding ?? "utf-8").GetByteCount(text),
            _ => 1,
        };
    }
}
