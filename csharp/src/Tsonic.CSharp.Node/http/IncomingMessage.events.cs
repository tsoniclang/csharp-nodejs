using System;
using Tsonic.CSharp.Node;

namespace Tsonic.CSharp.Node.Http;

public partial class IncomingMessage
{
    /// <summary>Registers a persistent binary data listener.</summary>
    public new IncomingMessage on(string eventName, Action<Buffer> listener)
    {
        base.on(eventName, listener);
        return this;
    }

    /// <summary>Registers a persistent zero-argument lifecycle listener.</summary>
    public new IncomingMessage on(string eventName, Action listener)
    {
        base.on(eventName, listener);
        return this;
    }

    /// <summary>Registers a persistent error listener.</summary>
    public new IncomingMessage on(string eventName, Action<Exception> listener)
    {
        base.on(eventName, listener);
        return this;
    }

    /// <summary>Registers a one-time binary data listener.</summary>
    public new IncomingMessage once(string eventName, Action<Buffer> listener)
    {
        base.once(eventName, listener);
        return this;
    }

    /// <summary>Registers a one-time zero-argument lifecycle listener.</summary>
    public new IncomingMessage once(string eventName, Action listener)
    {
        base.once(eventName, listener);
        return this;
    }

    /// <summary>Registers a one-time error listener.</summary>
    public new IncomingMessage once(string eventName, Action<Exception> listener)
    {
        base.once(eventName, listener);
        return this;
    }

    /// <summary>Removes a binary data listener.</summary>
    public new IncomingMessage off(string eventName, Action<Buffer> listener)
    {
        base.off(eventName, listener);
        return this;
    }

    /// <summary>Removes a zero-argument lifecycle listener.</summary>
    public new IncomingMessage off(string eventName, Action listener)
    {
        base.off(eventName, listener);
        return this;
    }

    /// <summary>Removes an error listener.</summary>
    public new IncomingMessage off(string eventName, Action<Exception> listener)
    {
        base.off(eventName, listener);
        return this;
    }
}
