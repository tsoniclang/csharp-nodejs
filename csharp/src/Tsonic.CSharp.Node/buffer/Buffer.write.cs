using System;

namespace Tsonic.CSharp.Node;

public partial class Buffer
{
    /// <summary>
    /// Writes string to buf at offset according to the character encoding.
    /// </summary>
    /// <param name="str">String to write to buf.</param>
    /// <param name="offset">Number of bytes to skip before starting to write string.</param>
    /// <param name="length">Maximum number of bytes to write.</param>
    /// <param name="encoding">The character encoding of string.</param>
    /// <returns>Number of bytes written.</returns>
    public int write(string str, int offset = 0, int? length = null, string encoding = "utf8")
    {
        if (offset < 0 || offset >= this.length)
            return 0;

        var maxLength = length ?? (this.length - offset);
        if (maxLength <= 0)
            return 0;

        var normalized = encoding.ToLowerInvariant().Replace("-", "").Replace("_", "");

        if (normalized == "hex")
        {
            return WriteHex(str, offset, maxLength);
        }
        else if (normalized == "base64" || normalized == "base64url")
        {
            return WriteBase64(str, offset, maxLength, normalized == "base64url");
        }
        else
        {
            var enc = GetEncoding(encoding);
            var bytes = enc.GetBytes(str);
            var bytesToWrite = Math.Min(bytes.Length, maxLength);
            Array.Copy(bytes, 0, _data, offset, bytesToWrite);
            return bytesToWrite;
        }
    }

    /// <summary>
    /// Writes hex string to buffer.
    /// </summary>
    private int WriteHex(string hex, int offset, int maxLength)
    {
        // Remove any whitespace
        hex = hex.Replace(" ", "").Replace("\t", "").Replace("\n", "").Replace("\r", "");

        var bytesToWrite = Math.Min(hex.Length / 2, maxLength);
        for (int i = 0; i < bytesToWrite; i++)
        {
            _data[offset + i] = Convert.ToByte(hex.Substring(i * 2, 2), 16);
        }
        return bytesToWrite;
    }

    /// <summary>
    /// Writes base64 string to buffer.
    /// </summary>
    private int WriteBase64(string base64, int offset, int maxLength, bool isBase64Url)
    {
        if (isBase64Url)
        {
            base64 = Base64UrlToBase64(base64);
        }

        var bytes = Convert.FromBase64String(base64);
        var bytesToWrite = Math.Min(bytes.Length, maxLength);
        Array.Copy(bytes, 0, _data, offset, bytesToWrite);
        return bytesToWrite;
    }
}
