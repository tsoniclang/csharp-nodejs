namespace Tsonic.CSharp.Node;

#pragma warning disable CS1591

public static partial class fs
{
    private static readonly object StatWatchersLock = new();
    private static readonly Dictionary<string, HashSet<StatWatcher>> StatWatchers = new(StringComparer.Ordinal);

    public static FsWatcher watch(string path, Action<string, string?>? listener = null) =>
        watch(path, new WatchOptions(), listener);

    public static FsWatcher watch(
        string path,
        WatchOptions options,
        Action<string, string?>? listener = null)
    {
        ArgumentNullException.ThrowIfNull(options);
        var directory = Directory.Exists(path) ? path : Path.GetDirectoryName(Path.GetFullPath(path)) ?? Environment.CurrentDirectory;
        var filter = Directory.Exists(path) ? "*" : Path.GetFileName(path);
        var watcher = new FileSystemWatcher(directory, filter)
        {
            IncludeSubdirectories = options.recursive ?? false
        };
        var result = new FsWatcher(watcher);
        void Publish(string eventType, string? filename)
        {
            Tsonic.CSharp.Js.JsEventLoop.EnqueueHandleOwned(() =>
            {
                if (result.closed)
                    return;
                listener?.Invoke(eventType, filename);
                result.publish(eventType, filename);
            });
        }
        watcher.Changed += (_, e) => Publish("change", e.Name);
        watcher.Created += (_, e) => Publish("rename", e.Name);
        watcher.Deleted += (_, e) => Publish("rename", e.Name);
        watcher.Renamed += (_, e) => Publish("rename", e.Name);
        watcher.Error += (_, e) => Tsonic.CSharp.Js.JsEventLoop.EnqueueHandleOwned(() =>
        {
            if (!result.closed)
                result.emit("error", e.GetException());
        });
        watcher.EnableRaisingEvents = true;
        if (options.persistent == false)
            result.unref();
        return result;
    }
    public static StatWatcher watchFile(string path, Action<Stats, Stats>? listener = null)
    {
        var fullPath = Path.GetFullPath(path);
        StatWatcher? result = null;
        result = new StatWatcher(
            fullPath,
            listener,
            TimeSpan.FromMilliseconds(5007),
            () => RemoveStatWatcher(result!));
        lock (StatWatchersLock)
        {
            if (!StatWatchers.TryGetValue(fullPath, out var entries))
            {
                entries = [];
                StatWatchers.Add(fullPath, entries);
            }
            entries.Add(result);
        }
        return result;
    }
    public static void unwatchFile(string path, Action<Stats, Stats>? listener = null)
    {
        var fullPath = Path.GetFullPath(path);
        StatWatcher[] matches;
        lock (StatWatchersLock)
        {
            matches = StatWatchers.TryGetValue(fullPath, out var entries)
                ? entries.Where(entry => listener is null || Equals(entry.listener, listener)).ToArray()
                : [];
        }
        foreach (var watcher in matches)
            watcher.close();
    }

    private static void RemoveStatWatcher(StatWatcher watcher)
    {
        lock (StatWatchersLock)
        {
            if (!StatWatchers.TryGetValue(watcher.path, out var entries))
                return;
            entries.Remove(watcher);
            if (entries.Count == 0)
                StatWatchers.Remove(watcher.path);
        }
    }
}
