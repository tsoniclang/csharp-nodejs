using System;

namespace Tsonic.CSharp.Node.Http;

public partial class Server
{
    /// <summary>Registers a persistent zero-argument lifecycle listener.</summary>
    public new Server on(string eventName, Action listener)
    {
        base.on(eventName, listener);
        return this;
    }

    /// <summary>Registers a persistent error listener.</summary>
    public Server on(string eventName, Action<Exception> listener)
    {
        base.on(eventName, listener);
        return this;
    }

    /// <summary>Registers a one-time zero-argument lifecycle listener.</summary>
    public new Server once(string eventName, Action listener)
    {
        base.once(eventName, listener);
        return this;
    }

    /// <summary>Registers a one-time error listener.</summary>
    public Server once(string eventName, Action<Exception> listener)
    {
        base.once(eventName, listener);
        return this;
    }

    /// <summary>Removes a zero-argument lifecycle listener.</summary>
    public Server off(string eventName, Action listener)
    {
        base.off(eventName, listener);
        return this;
    }

    /// <summary>Removes an error listener.</summary>
    public Server off(string eventName, Action<Exception> listener)
    {
        base.off(eventName, listener);
        return this;
    }
}
