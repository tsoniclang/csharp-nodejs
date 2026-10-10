using System;

namespace Tsonic.CSharp.Node;

public partial class Buffer
{
    /// <inheritdoc />
    public override string ToString() => toString();

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

        return Decode(_data.Slice(start, endIndex - start), encoding);
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
