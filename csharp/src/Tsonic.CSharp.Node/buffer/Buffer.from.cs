using System;
using System.Linq;

namespace Tsonic.CSharp.Node;

public partial class Buffer
{
    /// <summary>
    /// Creates a new Buffer containing the UTF-8 bytes of a string.
    /// </summary>
    /// <param name="str">The string to encode.</param>
    /// <param name="encoding">The character encoding to use.</param>
    /// <returns>A new Buffer instance.</returns>
    public static Buffer from(string str, string encoding = "utf8")
    {
        var enc = GetEncoding(encoding);
        var bytes = enc.GetBytes(str);
        return new Buffer(bytes);
    }

    /// <summary>
    /// Creates a new Buffer from an array of bytes.
    /// </summary>
    /// <param name="array">An array of bytes (values will be truncated to 0-255).</param>
    /// <returns>A new Buffer instance.</returns>
    public static Buffer from(int[] array)
    {
        var bytes = array.Select(v => (byte)(v & 0xFF)).ToArray();
        return new Buffer(bytes);
    }

    /// <summary>
    /// Creates a new Buffer from an array of bytes.
    /// </summary>
    /// <param name="array">An array of bytes.</param>
    /// <returns>A new Buffer instance.</returns>
    public static Buffer from(byte[] array)
    {
        var copy = new byte[array.Length];
        Array.Copy(array, copy, array.Length);
        return new Buffer(copy);
    }

    /// <summary>
    /// Creates a new Buffer from a Buffer (creates a copy).
    /// </summary>
    /// <param name="buffer">The buffer to copy.</param>
    /// <returns>A new Buffer instance.</returns>
    public static Buffer from(Buffer buffer)
    {
        var copy = new byte[buffer.length];
        Array.Copy(buffer._data, copy, buffer.length);
        return new Buffer(copy);
    }

    /// <summary>
    /// Creates a Buffer of the given elements.
    /// </summary>
    /// <param name="items">Elements to create buffer from.</param>
    /// <returns>A new Buffer instance.</returns>
    public static Buffer of(params int[] items)
    {
        return from(items);
    }
}
