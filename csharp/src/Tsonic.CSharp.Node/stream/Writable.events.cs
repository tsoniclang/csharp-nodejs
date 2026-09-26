using System;

namespace Tsonic.CSharp.Node;

public partial class Writable
{
    /// <summary>Registers a persistent lifecycle listener.</summary>
    public new Writable on(string eventName, Action listener)
    {
        base.on(eventName, listener);
        return this;
    }

    /// <summary>Registers a persistent error listener.</summary>
    public Writable on(string eventName, Action<Exception> listener)
    {
        base.on(eventName, listener);
        return this;
    }

    /// <summary>Registers a one-time lifecycle listener.</summary>
    public new Writable once(string eventName, Action listener)
    {
        base.once(eventName, listener);
        return this;
    }

    /// <summary>Registers a one-time error listener.</summary>
    public Writable once(string eventName, Action<Exception> listener)
    {
        base.once(eventName, listener);
        return this;
    }

    /// <summary>Removes a lifecycle listener.</summary>
    public Writable off(string eventName, Action listener)
    {
        base.off(eventName, listener);
        return this;
    }

    /// <summary>Removes an error listener.</summary>
    public Writable off(string eventName, Action<Exception> listener)
    {
        base.off(eventName, listener);
        return this;
    }
}
