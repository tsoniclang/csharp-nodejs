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
        return normalized switch
        {
            "utf8" => Encoding.UTF8,
            "ascii" => Encoding.ASCII,
            "latin1" or "binary" => Encoding.Latin1,
            "utf16le" or "ucs2" => Encoding.Unicode, // UTF-16 LE
            _ => throw new ArgumentException($"Unknown encoding: {encoding}", nameof(encoding))
        };
    }

    /// <summary>
    /// Converts hex string to bytes.
    /// </summary>
    /// <param name="hex">Hex string to convert.</param>
    /// <returns>Byte array.</returns>
    private static byte[] HexToBytes(string hex)
    {
        // Remove any whitespace
        hex = hex.Replace(" ", "").Replace("\t", "").Replace("\n", "").Replace("\r", "");

        var bytes = new byte[hex.Length / 2];
        for (int i = 0; i < bytes.Length; i++)
        {
            bytes[i] = Convert.ToByte(hex.Substring(i * 2, 2), 16);
        }
        return bytes;
    }

    /// <summary>
    /// Converts bytes to hex string.
    /// </summary>
    /// <param name="bytes">Bytes to convert.</param>
    /// <param name="start">Start offset.</param>
    /// <param name="end">End offset.</param>
    /// <returns>Hex string.</returns>
    private static string BytesToHex(byte[] bytes, int start, int end)
    {
        var sb = new StringBuilder((end - start) * 2);
        for (int i = start; i < end; i++)
        {
            sb.Append(bytes[i].ToString("x2"));
        }
        return sb.ToString();
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
