namespace Tsonic.CSharp.Node;

/// <summary>A closed process-launch or bounded-capture failure.</summary>
public sealed class SpawnSyncError : Exception
{
    /// <summary>The native failure code.</summary>
    public string code { get; }
    /// <summary>The failure message without implicit stack collection.</summary>
    public string message => Message;

    /// <summary>Constructs a process failure without capturing a stack.</summary>
    public SpawnSyncError(string code, string message) : base(message)
    {
        this.code = code;
    }
}
