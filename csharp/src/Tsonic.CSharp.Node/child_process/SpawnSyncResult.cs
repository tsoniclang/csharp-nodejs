namespace Tsonic.CSharp.Node;

/// <summary>
/// Closed target carrier for the supported Buffer specialization of Node's SpawnSyncReturns&lt;T&gt;.
/// </summary>
public sealed class SpawnSyncResult
{
    /// <summary>The bytes written to standard output.</summary>
    public Buffer stdout { get; set; } = Buffer.alloc(0);

    /// <summary>The bytes written to standard error, or the process-start error message.</summary>
    public Buffer stderr { get; set; } = Buffer.alloc(0);

    /// <summary>The process exit code, or <see langword="null"/> when the process did not start.</summary>
    public int? status { get; set; }
}
