using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace Tsonic.CSharp.Node;

#pragma warning disable CS1591

#pragma warning disable IDE1006

public static class diagnostics_channel
{
    private static readonly ConcurrentDictionary<string, Channel> Channels = new(StringComparer.Ordinal);

    public static Channel channel(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Channel name is required.", nameof(name));

        return Channels.GetOrAdd(name, static key => new Channel(key));
    }

    public static bool hasSubscribers(string name)
    {
        return Channels.TryGetValue(name, out var channel) && channel.hasSubscribers;
    }

    public static IDisposable subscribe(string name, Action<object?, string> onMessage)
    {
        return channel(name).subscribe(onMessage);
    }

    public static bool unsubscribe(string name, Action<object?, string> onMessage)
    {
        return Channels.TryGetValue(name, out var channel) && channel.unsubscribe(onMessage);
    }

    public static void tracingChannel(string name)
    {
        _ = channel(name);
    }
}

public sealed class Channel
{
    private readonly List<Action<object?, string>> _subscribers = new();
    private readonly object _lock = new();

    internal Channel(string name)
    {
        this.name = name;
    }

    public string name { get; }

    public bool hasSubscribers
    {
        get
        {
            lock (_lock)
                return _subscribers.Count > 0;
        }
    }

    public IDisposable subscribe(Action<object?, string> onMessage)
    {
        if (onMessage == null)
            throw new ArgumentNullException(nameof(onMessage));

        lock (_lock)
            _subscribers.Add(onMessage);

        return new Subscription(this, onMessage);
    }

    public bool unsubscribe(Action<object?, string> onMessage)
    {
        lock (_lock)
            return _subscribers.Remove(onMessage);
    }

    public void publish(object? message)
    {
        Action<object?, string>[] subscribers;
        lock (_lock)
            subscribers = _subscribers.ToArray();

        foreach (var subscriber in subscribers)
            subscriber(message, name);
    }

    public T runStores<T>(object? context, Func<T> callback)
    {
        _ = context;
        if (callback == null)
            throw new ArgumentNullException(nameof(callback));

        return callback();
    }

    public void runStores(object? context, Action callback)
    {
        _ = context;
        if (callback == null)
            throw new ArgumentNullException(nameof(callback));

        callback();
    }

    private sealed class Subscription : IDisposable
    {
        private readonly Channel _channel;
        private readonly Action<object?, string> _subscriber;
        private bool _disposed;

        public Subscription(Channel channel, Action<object?, string> subscriber)
        {
            _channel = channel;
            _subscriber = subscriber;
        }

        public void Dispose()
        {
            if (_disposed)
                return;

            _disposed = true;
            _channel.unsubscribe(_subscriber);
        }
    }
}
