using System.Threading;
using Tsonic.CSharp.Js;

namespace Tsonic.CSharp.Node;

#pragma warning disable CS1591

public sealed class FsWatcher : EventEmitter
{
    private readonly FileSystemWatcher? _watcher;
    private int _closed;
    private int _referenced = 1;

    internal FsWatcher(FileSystemWatcher? watcher)
    {
        _watcher = watcher;
        ProcessKeepAlive.Acquire();
    }

    public bool closed => Volatile.Read(ref _closed) != 0;

    public void close()
    {
        if (Interlocked.Exchange(ref _closed, 1) != 0)
            return;
        _watcher?.Dispose();
        if (Interlocked.Exchange(ref _referenced, 0) != 0)
            ProcessKeepAlive.Release();
        emit("close");
    }

    internal void publish(string eventType, string? filename)
    {
        if (!closed)
            emit("change", eventType, filename);
    }

    public FsWatcher @ref()
    {
        if (!closed && Interlocked.Exchange(ref _referenced, 1) == 0)
            ProcessKeepAlive.Acquire();
        return this;
    }

    public FsWatcher unref()
    {
        if (Interlocked.Exchange(ref _referenced, 0) != 0)
            ProcessKeepAlive.Release();
        return this;
    }

    public void poll() { }
}

public sealed class StatWatcher : EventEmitter
{
    private readonly Timer _timer;
    private readonly Action _onClose;
    private Stats _previous;
    private int _callbackPending;
    private int _closed;
    private int _referenced = 1;

    internal StatWatcher(
        string path,
        Action<Stats, Stats>? listener,
        TimeSpan interval,
        Action onClose)
    {
        this.path = path;
        this.listener = listener;
        _onClose = onClose;
        _previous = CurrentStats(path);
        _timer = new Timer(
            static state => ((StatWatcher)state!).SchedulePoll(),
            this,
            interval,
            interval);
        ProcessKeepAlive.Acquire();
    }

    internal string path { get; }
    internal Action<Stats, Stats>? listener { get; }
    public bool closed => Volatile.Read(ref _closed) != 0;

    public void close()
    {
        if (Interlocked.Exchange(ref _closed, 1) != 0)
            return;
        _timer.Dispose();
        _onClose();
        if (Interlocked.Exchange(ref _referenced, 0) != 0)
            ProcessKeepAlive.Release();
        emit("close");
    }

    internal void publish(Stats current, Stats previous)
    {
        if (!closed)
        {
            listener?.Invoke(current, previous);
            emit("change", current, previous);
        }
    }

    private void SchedulePoll()
    {
        if (closed || Interlocked.Exchange(ref _callbackPending, 1) != 0)
            return;
        JsEventLoop.EnqueueHandleOwned(() =>
        {
            try
            {
                if (closed)
                    return;
                var current = CurrentStats(path);
                var previous = _previous;
                _previous = current;
                if (StatsChanged(current, previous))
                    publish(current, previous);
            }
            finally
            {
                Volatile.Write(ref _callbackPending, 0);
            }
        });
    }

    private static Stats CurrentStats(string path)
    {
        try
        {
            return File.Exists(path) || Directory.Exists(path)
                ? fs.statSync(path)
                : new Stats();
        }
        catch (FileNotFoundException)
        {
            return new Stats();
        }
        catch (DirectoryNotFoundException)
        {
            return new Stats();
        }
    }

    private static bool StatsChanged(Stats current, Stats previous) =>
        current.dev != previous.dev ||
        current.ino != previous.ino ||
        current.nlink != previous.nlink ||
        current.uid != previous.uid ||
        current.gid != previous.gid ||
        current.rdev != previous.rdev ||
        current.blksize != previous.blksize ||
        current.blocks != previous.blocks ||
        current.size != previous.size ||
        current.mode != previous.mode ||
        current.atimeMs != previous.atimeMs ||
        current.mtimeMs != previous.mtimeMs ||
        current.ctimeMs != previous.ctimeMs ||
        current.birthtimeMs != previous.birthtimeMs ||
        current.isFile != previous.isFile ||
        current.isDirectory != previous.isDirectory ||
        current.isSymbolicLink != previous.isSymbolicLink;

    public StatWatcher @ref()
    {
        if (!closed && Interlocked.Exchange(ref _referenced, 1) == 0)
            ProcessKeepAlive.Acquire();
        return this;
    }

    public StatWatcher unref()
    {
        if (Interlocked.Exchange(ref _referenced, 0) != 0)
            ProcessKeepAlive.Release();
        return this;
    }
}
