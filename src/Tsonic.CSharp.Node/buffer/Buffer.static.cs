using System;
using System.Linq;

namespace Tsonic.CSharp.Node;

public partial class Buffer
{
    /// <summary>
    /// Returns true if obj is a Buffer, false otherwise.
    /// </summary>
    /// <param name="obj">The object to check.</param>
    /// <returns>True if obj is a Buffer.</returns>
    public static bool isBuffer(object? obj)
    {
        return obj is Buffer;
    }

    /// <summary>
    /// Returns true if encoding is the name of a supported character encoding.
    /// </summary>
    /// <param name="encoding">A character encoding name to check.</param>
    /// <returns>True if the encoding is supported.</returns>
    public static bool isEncoding(string encoding)
    {
        var normalized = encoding.ToLowerInvariant().Replace("-", "").Replace("_", "");
        return normalized switch
        {
            "utf8" => true,
            "ascii" => true,
            "latin1" => true,
            "binary" => true,
            "base64" => true,
            "base64url" => true,
            "hex" => true,
            "utf16le" => true,
            "ucs2" => true,
            _ => false
        };
    }

    /// <summary>
    /// Returns the byte length of a string when encoded using encoding.
    /// </summary>
    /// <param name="str">A value to calculate the length of.</param>
    /// <param name="encoding">The character encoding.</param>
    /// <returns>The number of bytes contained within string.</returns>
    public static int byteLength(string str, string encoding = "utf8")
    {
        var enc = GetEncoding(encoding);
        return enc.GetByteCount(str);
    }

    /// <summary>
    /// Compares buf1 to buf2, typically for sorting arrays of Buffer instances.
    /// </summary>
    /// <param name="buf1">The first buffer to compare.</param>
    /// <param name="buf2">The second buffer to compare.</param>
    /// <returns>-1, 0, or 1 depending on the comparison result.</returns>
    public static int compare(Buffer buf1, Buffer buf2)
    {
        return buf1.compare(buf2);
    }

    /// <summary>
    /// Returns a new Buffer which is the result of concatenating all the Buffer instances in the list together.
    /// </summary>
    /// <param name="list">List of Buffer instances to concatenate.</param>
    /// <param name="totalLength">Total length of the Buffer instances in list when concatenated.</param>
    /// <returns>A new concatenated Buffer.</returns>
    public static Buffer concat(Buffer[] list, int? totalLength = null)
    {
        if (list.Length == 0)
            return alloc(0);

        var length = totalLength ?? list.Sum(b => b.length);
        var result = alloc(length);
        var offset = 0;

        foreach (var buf in list)
        {
            if (offset >= length)
                break;

            var copyLength = Math.Min(buf.length, length - offset);
            Array.Copy(buf._data, 0, result._data, offset, copyLength);
            offset += copyLength;
        }

        return result;
    }
}
