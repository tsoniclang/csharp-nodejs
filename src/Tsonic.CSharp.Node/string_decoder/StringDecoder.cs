using System.Text;

namespace Tsonic.CSharp.Node;

/// <summary>
/// Provides an API for decoding Buffer objects into strings in a manner
/// that preserves encoded multi-byte UTF-8 and UTF-16 characters.
/// All methods follow JavaScript naming conventions (lowercase).
/// </summary>
public partial class StringDecoder
{
    internal readonly Encoding _encoding;
    internal readonly Decoder _decoder;

    /// <summary>
    /// Creates a new StringDecoder instance.
    /// </summary>
    /// <param name="encoding">The character encoding to use. Defaults to 'utf8'.</param>
    public StringDecoder(string? encoding = null)
    {
        encoding ??= "utf8";

        _encoding = encoding.ToLowerInvariant() switch
        {
            "utf8" or "utf-8" => Encoding.UTF8,
            "utf16le" or "utf-16le" or "ucs2" or "ucs-2" => Encoding.Unicode,
            "ascii" => Encoding.ASCII,
            "latin1" or "binary" => Encoding.Latin1,
            _ => Encoding.UTF8
        };

        _decoder = _encoding.GetDecoder();
    }
}
