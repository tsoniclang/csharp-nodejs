using System;
using System.Text;

namespace Tsonic.CSharp.Node;

public partial class Buffer
{
    /// <summary>
    /// Decodes buf to a string according to the specified character encoding.
    /// </summary>
    /// <param name="encoding">The character encoding to use.</param>
    /// <param name="start">The byte offset to start decoding at.</param>
    /// <param name="end">The byte offset to stop decoding at (not inclusive).</param>
    /// <returns>The decoded string.</returns>
    public string toString(string encoding = "utf8", int start = 0, int? end = null)
    {
        var endIndex = end ?? length;

        if (start < 0) start = 0;
        if (endIndex > length) endIndex = length;
        if (start >= endIndex) return string.Empty;

        var normalized = encoding.ToLowerInvariant().Replace("-", "").Replace("_", "");

        if (normalized == "hex")
        {
            return BytesToHex(_data, start, endIndex);
        }
        else if (normalized == "base64")
        {
            return Convert.ToBase64String(_data, start, endIndex - start);
        }
        else if (normalized == "base64url")
        {
            var base64 = Convert.ToBase64String(_data, start, endIndex - start);
            return Base64ToBase64Url(base64);
        }
        else
        {
            var enc = GetEncoding(encoding);
            return enc.GetString(_data, start, endIndex - start);
        }
    }

    /// <summary>
    /// Returns a JSON representation of buf.
    /// </summary>
    /// <returns>An object with type and data properties.</returns>
    public object toJSON()
    {
        return new
        {
            type = "Buffer",
            data = _data.ToArray()
        };
    }
}
