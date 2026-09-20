using System;

namespace Tsonic.CSharp.Node;

public partial class Buffer
{
    /// <summary>
    /// Returns true if both buf and otherBuffer have exactly the same bytes.
    /// </summary>
    /// <param name="otherBuffer">A Buffer to compare to.</param>
    /// <returns>True if the buffers are equal.</returns>
    public bool equals(Buffer otherBuffer)
    {
        return _data.SequenceEqual(otherBuffer._data);
    }

    /// <summary>
    /// Compares buf with target and returns a number indicating sort order.
    /// </summary>
    /// <param name="target">A Buffer to compare to.</param>
    /// <param name="targetStart">The offset within target at which to begin comparison.</param>
    /// <param name="targetEnd">The offset within target at which to end comparison (not inclusive).</param>
    /// <param name="sourceStart">The offset within buf at which to begin comparison.</param>
    /// <param name="sourceEnd">The offset within buf at which to end comparison (not inclusive).</param>
    /// <returns>-1, 0, or 1 depending on the comparison result.</returns>
    public int compare(Buffer target, int? targetStart = null, int? targetEnd = null,
                       int? sourceStart = null, int? sourceEnd = null)
    {
        var tStart = targetStart ?? 0;
        var tEnd = targetEnd ?? target.length;
        var sStart = sourceStart ?? 0;
        var sEnd = sourceEnd ?? length;

        // Clamp ranges
        tStart = Math.Max(0, Math.Min(tStart, target.length));
        tEnd = Math.Max(tStart, Math.Min(tEnd, target.length));
        sStart = Math.Max(0, Math.Min(sStart, length));
        sEnd = Math.Max(sStart, Math.Min(sEnd, length));

        return Math.Sign(_data.Slice(sStart, sEnd - sStart)
            .SequenceCompareTo(target._data.Slice(tStart, tEnd - tStart)));
    }

    /// <summary>
    /// Equivalent to buf.indexOf() !== -1.
    /// </summary>
    /// <param name="value">What to search for.</param>
    /// <param name="byteOffset">Where to begin searching in buf.</param>
    /// <param name="encoding">If value is a string, this is the encoding used.</param>
    /// <returns>True if value was found in buf.</returns>
    public bool includes(object value, int byteOffset = 0, string encoding = "utf8")
    {
        return indexOf(value, byteOffset, encoding) != -1;
    }

    /// <summary>
    /// Returns the index of the first occurrence of value in buf, or -1 if buf does not contain value.
    /// </summary>
    /// <param name="value">What to search for.</param>
    /// <param name="byteOffset">Where to begin searching in buf.</param>
    /// <param name="encoding">If value is a string, this is its encoding.</param>
    /// <returns>The index of the first occurrence of value, or -1.</returns>
    public int indexOf(object value, int byteOffset = 0, string encoding = "utf8")
    {
        if (byteOffset < 0) byteOffset = Math.Max(0, length + byteOffset);
        if (byteOffset >= length) return -1;

        ReadOnlySpan<byte> searchBytes;

        if (value is string str)
        {
            searchBytes = GetEncoding(encoding).GetBytes(str);
        }
        else if (value is int intValue)
        {
            var found = _data.Slice(byteOffset).IndexOf((byte)intValue);
            return found < 0 ? -1 : byteOffset + found;
        }
        else if (value is Buffer bufferValue)
        {
            searchBytes = bufferValue._data;
        }
        else
        {
            return -1;
        }

        if (searchBytes.Length == 0) return byteOffset;

        var index = _data.Slice(byteOffset).IndexOf(searchBytes);
        return index < 0 ? -1 : byteOffset + index;
    }

    /// <summary>
    /// Returns the index of the last occurrence of value in buf, or -1 if buf does not contain value.
    /// </summary>
    /// <param name="value">What to search for.</param>
    /// <param name="byteOffset">Where to begin searching in buf.</param>
    /// <param name="encoding">If value is a string, this is its encoding.</param>
    /// <returns>The index of the last occurrence of value, or -1.</returns>
    public int lastIndexOf(object value, int? byteOffset = null, string encoding = "utf8")
    {
        var offset = byteOffset ?? length - 1;
        if (offset < 0) offset = Math.Max(0, length + offset);
        if (offset >= length) offset = length - 1;

        ReadOnlySpan<byte> searchBytes;

        if (value is string str)
        {
            searchBytes = GetEncoding(encoding).GetBytes(str);
        }
        else if (value is int intValue)
        {
            return _data.Slice(0, offset + 1).LastIndexOf((byte)intValue);
        }
        else if (value is Buffer bufferValue)
        {
            searchBytes = bufferValue._data;
        }
        else
        {
            return -1;
        }

        if (searchBytes.Length == 0) return offset;

        var maxStart = Math.Min(offset, length - searchBytes.Length);
        return maxStart < 0 ? -1 : _data.Slice(0, maxStart + searchBytes.Length).LastIndexOf(searchBytes);
    }
}
