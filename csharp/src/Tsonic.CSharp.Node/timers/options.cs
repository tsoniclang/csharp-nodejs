#pragma warning disable CS1591
#pragma warning disable IDE1006

using System.Threading;

namespace Tsonic.CSharp.Node;

public sealed class TimerOptions
{
    public bool @ref { get; set; } = true;

    public CancellationToken signal { get; set; }

    public bool signalAborted => signal.IsCancellationRequested;
}
