using System;

namespace Tsonic.CSharp.Node;

public partial class Duplex
{
    /// <summary>Registers a persistent binary data listener.</summary>
    public new Duplex on(string eventName, Action<Buffer> listener)
    {
        base.on(eventName, listener);
        return this;
    }

    /// <summary>Registers a one-time binary data listener.</summary>
    public new Duplex once(string eventName, Action<Buffer> listener)
    {
        base.once(eventName, listener);
        return this;
    }

    /// <summary>Removes a binary data listener.</summary>
    public new Duplex off(string eventName, Action<Buffer> listener)
    {
        base.off(eventName, listener);
        return this;
    }

    /// <summary>Registers a persistent lifecycle listener.</summary>
    public new Duplex on(string eventName, Action listener)
    {
        base.on(eventName, listener);
        return this;
    }

    /// <summary>Registers a persistent error listener.</summary>
    public new Duplex on(string eventName, Action<Exception> listener)
    {
        base.on(eventName, listener);
        return this;
    }

    /// <summary>Registers a one-time lifecycle listener.</summary>
    public new Duplex once(string eventName, Action listener)
    {
        base.once(eventName, listener);
        return this;
    }

    /// <summary>Registers a one-time error listener.</summary>
    public new Duplex once(string eventName, Action<Exception> listener)
    {
        base.once(eventName, listener);
        return this;
    }

    /// <summary>Removes a lifecycle listener.</summary>
    public new Duplex off(string eventName, Action listener)
    {
        base.off(eventName, listener);
        return this;
    }

    /// <summary>Removes an error listener.</summary>
    public new Duplex off(string eventName, Action<Exception> listener)
    {
        base.off(eventName, listener);
        return this;
    }
}
