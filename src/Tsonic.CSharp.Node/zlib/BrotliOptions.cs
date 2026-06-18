namespace Tsonic.CSharp.Node;

/// <summary>
/// Options for Brotli compression operations.
/// </summary>
public class BrotliOptions
{
    /// <summary>
    /// Compression quality. Range: 0 (fastest) to 11 (best compression).
    /// Default: 11 (maximum compression)
    /// </summary>
    public int? quality { get; set; }

    /// <summary>
    /// Chunk size for internal buffer. Default: 16*1024 (16 KB).
    /// </summary>
    public int? chunkSize { get; set; }

    /// <summary>
    /// Maximum output length to prevent excessive memory usage.
    /// </summary>
    public int? maxOutputLength { get; set; }
}
