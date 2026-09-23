namespace Tsonic.CSharp.Node;

/// <summary>
/// Closed target carrier for the supported Buffer specialization of Node's SpawnSyncReturns&lt;T&gt;.
/// </summary>
public sealed class SpawnSyncResult
{
    /// <summary>The bytes written to standard output.</summary>
    public Buffer? stdout { get; set; }

    /// <summary>The bytes written to standard error, absent when not captured.</summary>
    public Buffer? stderr { get; set; }

    /// <summary>The process exit code, or <see langword="null"/> when the process did not start.</summary>
    public int? status { get; set; }

    /// <summary>The started process ID, absent when process creation failed.</summary>
    public int? pid { get; set; }

    /// <summary>The proven termination signal, or null for an ordinary exit.</summary>
    public string? signal { get; set; }

    /// <summary>The launch or capture error, absent on success.</summary>
    public SpawnSyncError? error { get; set; }
}
