using System;

namespace Tsonic.CSharp.Node;

public partial class Buffer
{
    /// <summary>
    /// Fills buf with the specified value.
    /// </summary>
    /// <param name="value">The value to fill buf with.</param>
    /// <param name="offset">Number of bytes to skip before starting to fill buf.</param>
    /// <param name="end">Where to stop filling buf (not inclusive).</param>
    /// <param name="encoding">The encoding for value if value is a string.</param>
    /// <returns>A reference to buf.</returns>
    public Buffer fill(object value, int offset = 0, int? end = null, string encoding = "utf8")
    {
        var endIndex = end ?? length;

        // Clamp range
        offset = Math.Max(0, Math.Min(offset, length));
        endIndex = Math.Max(offset, Math.Min(endIndex, length));

        if (offset >= endIndex)
            return this;

        if (value is string str)
        {
            if (str.Length == 0)
                return this;

            var bytes = GetEncoding(encoding).GetBytes(str);
            if (bytes.Length == 0)
                return this;

            FillPattern(bytes, _data.Slice(offset, endIndex - offset));
        }
        else if (value is int intValue)
        {
            var byteValue = (byte)(intValue & 0xFF);
            _data.Slice(offset, endIndex - offset).Fill(byteValue);
        }
        else if (value is Buffer bufferValue)
        {
            if (bufferValue.length == 0)
                return this;

            FillPattern(bufferValue._data, _data.Slice(offset, endIndex - offset));
        }

        return this;
    }

    private static void FillPattern(ReadOnlySpan<byte> pattern, Span<byte> destination)
    {
        var filled = Math.Min(pattern.Length, destination.Length);
        pattern.Slice(0, filled).CopyTo(destination);
        while (filled < destination.Length)
        {
            var count = Math.Min(filled, destination.Length - filled);
            destination.Slice(0, count).CopyTo(destination.Slice(filled));
            filled += count;
        }
    }
}
