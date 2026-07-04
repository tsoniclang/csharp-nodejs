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
        if (length != otherBuffer.length)
            return false;

        for (int i = 0; i < length; i++)
        {
            if (_data[i] != otherBuffer._data[i])
                return false;
        }

        return true;
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

        var sourceLength = sEnd - sStart;
        var targetLength = tEnd - tStart;
        var minLength = Math.Min(sourceLength, targetLength);

        for (int i = 0; i < minLength; i++)
        {
            if (_data[sStart + i] < target._data[tStart + i])
                return -1;
            if (_data[sStart + i] > target._data[tStart + i])
                return 1;
        }

        // If all bytes are equal so far, the shorter buffer comes first
        if (sourceLength < targetLength)
            return -1;
        if (sourceLength > targetLength)
            return 1;

        return 0;
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

        byte[] searchBytes;

        if (value is string str)
        {
            searchBytes = GetEncoding(encoding).GetBytes(str);
        }
        else if (value is int intValue)
        {
            searchBytes = new[] { (byte)(intValue & 0xFF) };
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

        for (int i = byteOffset; i <= length - searchBytes.Length; i++)
        {
            bool found = true;
            for (int j = 0; j < searchBytes.Length; j++)
            {
                if (_data[i + j] != searchBytes[j])
                {
                    found = false;
                    break;
                }
            }
            if (found)
                return i;
        }

        return -1;
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

        byte[] searchBytes;

        if (value is string str)
        {
            searchBytes = GetEncoding(encoding).GetBytes(str);
        }
        else if (value is int intValue)
        {
            searchBytes = new[] { (byte)(intValue & 0xFF) };
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

        for (int i = Math.Min(offset, length - searchBytes.Length); i >= 0; i--)
        {
            bool found = true;
            for (int j = 0; j < searchBytes.Length; j++)
            {
                if (_data[i + j] != searchBytes[j])
                {
                    found = false;
                    break;
                }
            }
            if (found)
                return i;
        }

        return -1;
    }
}
