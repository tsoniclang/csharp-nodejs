using System;

namespace Tsonic.CSharp.Node;

public partial class Buffer
{
    /// <summary>
    /// Returns a new Buffer that references the same memory as the original, but offset and cropped by start and end.
    /// Note: In this C# implementation, this creates a new copy of the data.
    /// </summary>
    /// <param name="start">Where the new Buffer will start.</param>
    /// <param name="end">Where the new Buffer will end (not inclusive).</param>
    /// <returns>A new Buffer instance.</returns>
    public Buffer slice(int? start = null, int? end = null)
    {
        var startIndex = start ?? 0;
        var endIndex = end ?? length;

        // Handle negative indices
        if (startIndex < 0) startIndex = Math.Max(0, length + startIndex);
        if (endIndex < 0) endIndex = Math.Max(0, length + endIndex);

        // Clamp to valid range
        startIndex = Math.Max(0, Math.Min(startIndex, length));
        endIndex = Math.Max(startIndex, Math.Min(endIndex, length));

        var sliceLength = endIndex - startIndex;
        var newData = new byte[sliceLength];
        Array.Copy(_data, startIndex, newData, 0, sliceLength);

        return new Buffer(newData);
    }

    /// <summary>
    /// Returns a view of the buffer memory (same as slice in this implementation).
    /// </summary>
    /// <param name="start">Where the view will start.</param>
    /// <param name="end">Where the view will end (not inclusive).</param>
    /// <returns>A new Buffer instance.</returns>
    public Buffer subarray(int? start = null, int? end = null)
    {
        return slice(start, end);
    }

    /// <summary>
    /// Copies data from a region of buf to a region in target.
    /// </summary>
    /// <param name="target">A Buffer to copy into.</param>
    /// <param name="targetStart">The offset within target at which to begin writing.</param>
    /// <param name="sourceStart">The offset within buf from which to begin copying.</param>
    /// <param name="sourceEnd">The offset within buf at which to stop copying (not inclusive).</param>
    /// <returns>The number of bytes copied.</returns>
    public int copy(Buffer target, int targetStart = 0, int? sourceStart = null, int? sourceEnd = null)
    {
        var srcStart = sourceStart ?? 0;
        var srcEnd = sourceEnd ?? length;

        // Clamp source range
        srcStart = Math.Max(0, Math.Min(srcStart, length));
        srcEnd = Math.Max(srcStart, Math.Min(srcEnd, length));

        // Clamp target start
        if (targetStart < 0 || targetStart >= target.length)
            return 0;

        var bytesToCopy = Math.Min(srcEnd - srcStart, target.length - targetStart);
        if (bytesToCopy <= 0)
            return 0;

        Array.Copy(_data, srcStart, target._data, targetStart, bytesToCopy);
        return bytesToCopy;
    }
}
