namespace Tsonic.CSharp.Node;

/// <summary>
/// Options for Gzip and Deflate compression operations.
/// </summary>
public class ZlibOptions
{
    /// <summary>
    /// Compression level. Range: -1 (default), 0 (no compression) to 9 (max compression).
    /// Default: -1 (optimal balance)
    /// </summary>
    public int? level { get; set; }

    /// <summary>
    /// Chunk size for internal buffer. Default: 16*1024 (16 KB).
    /// </summary>
    public int? chunkSize { get; set; }

    /// <summary>
    /// Window size (8-15). Larger values use more memory but may improve compression.
    /// </summary>
    public int? windowBits { get; set; }

    /// <summary>
    /// Memory level (1-9). Higher values use more memory for better compression.
    /// </summary>
    public int? memLevel { get; set; }

    /// <summary>
    /// Compression strategy.
    /// </summary>
    public int? strategy { get; set; }

    /// <summary>
    /// Maximum output length to prevent excessive memory usage.
    /// </summary>
    public int? maxOutputLength { get; set; }
}
