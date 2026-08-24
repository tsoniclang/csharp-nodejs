namespace Tsonic.CSharp.Node;

#pragma warning disable CS8981 // Lowercase type/member names (Node.js surface)
#pragma warning disable IDE1006 // Naming rule violation

/// <summary>
/// Options for fs.rm()/fs.rmSync().
/// </summary>
public sealed class RmOptions
{
    /// <summary>
    /// When true, remove directory contents recursively.
    /// </summary>
    public bool? recursive { get; set; }

    /// <summary>
    /// When true, ignore a missing path.
    /// </summary>
    public bool? force { get; set; }

    /// <summary>
    /// Maximum retries for retryable recursive-removal failures.
    /// </summary>
    public double? maxRetries { get; set; }

    /// <summary>
    /// Base delay in milliseconds between recursive-removal retries.
    /// </summary>
    public double? retryDelay { get; set; }
}

#pragma warning restore CS8981
#pragma warning restore IDE1006
