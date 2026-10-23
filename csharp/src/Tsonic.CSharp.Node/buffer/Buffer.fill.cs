using System;
using Tsonic.CSharp.Runtime;

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

        var destination = _data.Slice(offset, endIndex - offset);

        if (value is string str)
        {
            if (destination.IsEmpty || str.Length == 0)
                return this;

            var bytes = GetEncoding(encoding).GetBytes(str);
            if (bytes.Length == 0)
                return this;

            FillPattern(bytes, destination);
        }
        else if (value is Buffer bufferValue)
        {
            if (destination.IsEmpty || bufferValue.length == 0)
                return this;

            FillPattern(bufferValue._data, destination);
        }
        else
        {
            var byteValue = value switch
            {
                sbyte number => (byte)(number & 0xFF),
                byte number => number,
                short number => (byte)(number & 0xFF),
                ushort number => (byte)(number & 0xFF),
                int number => (byte)(number & 0xFF),
                uint number => (byte)(number & 0xFFU),
                long number => (byte)(number & 0xFFL),
                ulong number => (byte)(number & 0xFFUL),
                Half number => fillByte((double)number),
                float number => fillByte((double)number),
                double number => fillByte(number),
                decimal number => fillByte(number),
                _ => throw new TypeError("ERR_INVALID_ARG_TYPE: fill value must be a string, Buffer or native numeric value.")
            };
            destination.Fill(byteValue);
        }

        return this;
    }

    private static byte fillByte(double value)
    {
        if (!double.IsFinite(value))
            return 0;
        var remainder = Math.Truncate(value) % 256;
        return (byte)(remainder < 0 ? remainder + 256 : remainder);
    }

    private static byte fillByte(decimal value)
    {
        var remainder = decimal.Truncate(value) % 256M;
        return (byte)(remainder < 0 ? remainder + 256M : remainder);
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
