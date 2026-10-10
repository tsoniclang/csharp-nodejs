using System;
using System.Text;

namespace Tsonic.CSharp.Node;

public partial class Buffer
{
    /// <summary>
    /// Gets a System.Text.Encoding instance for the specified encoding name.
    /// </summary>
    /// <param name="encoding">The encoding name.</param>
    /// <returns>A System.Text.Encoding instance.</returns>
    private static Encoding GetEncoding(string encoding)
    {
        var normalized = encoding.ToLowerInvariant().Replace("-", "").Replace("_", "");
        return GetTextEncoding(normalized, encoding);
    }

    private static Encoding GetTextEncoding(string normalized, string encoding)
    {
        return normalized switch
        {
            "utf8" => Encoding.UTF8,
            "ascii" => Encoding.ASCII,
            "latin1" or "binary" => Encoding.Latin1,
            "utf16le" or "ucs2" => Encoding.Unicode, // UTF-16 LE
            _ => throw new ArgumentException($"Unknown encoding: {encoding}", nameof(encoding))
        };
    }

    private static byte[] GetStringBytes(string value, string encoding)
    {
        var normalized = encoding.ToLowerInvariant().Replace("-", "").Replace("_", "");
        return normalized switch
        {
            "hex" => HexToBytes(value),
            "base64" => Convert.FromBase64String(value),
            "base64url" => Convert.FromBase64String(Base64UrlToBase64(value)),
            _ => GetTextEncoding(normalized, encoding).GetBytes(value)
        };
    }

    internal static string Decode(ReadOnlySpan<byte> bytes, string encoding)
    {
        if (bytes.IsEmpty)
            return string.Empty;
        var normalized = encoding.ToLowerInvariant().Replace("-", "").Replace("_", "");
        return normalized switch
        {
            "hex" => Convert.ToHexStringLower(bytes),
            "base64" => Convert.ToBase64String(bytes),
            "base64url" => Base64ToBase64Url(Convert.ToBase64String(bytes)),
            _ => GetTextEncoding(normalized, encoding).GetString(bytes)
        };
    }

    /// <summary>
    /// Converts hex string to bytes.
    /// </summary>
    /// <param name="hex">Hex string to convert.</param>
    /// <returns>Byte array.</returns>
    private static byte[] HexToBytes(string hex)
    {
        return Convert.FromHexString(hex);
    }

    /// <summary>
    /// Converts base64url string to base64 string.
    /// </summary>
    /// <param name="base64url">Base64url string.</param>
    /// <returns>Base64 string.</returns>
    private static string Base64UrlToBase64(string base64url)
    {
        var base64 = base64url.Replace('-', '+').Replace('_', '/');
        // Add padding if needed
        var padding = (4 - (base64.Length % 4)) % 4;
        if (padding > 0)
        {
            base64 = base64 + new string('=', padding);
        }
        return base64;
    }

    /// <summary>
    /// Converts base64 string to base64url string.
    /// </summary>
    /// <param name="base64">Base64 string.</param>
    /// <returns>Base64url string.</returns>
    private static string Base64ToBase64Url(string base64)
    {
        return base64.Replace('+', '-').Replace('/', '_').TrimEnd('=');
    }
}
