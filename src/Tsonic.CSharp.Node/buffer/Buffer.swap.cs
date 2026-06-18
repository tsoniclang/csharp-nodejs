using System;

namespace Tsonic.CSharp.Node;

public partial class Buffer
{
    /// <summary>
    /// Reverses the buffer in-place.
    /// </summary>
    /// <returns>A reference to buf.</returns>
    public Buffer reverse()
    {
        Array.Reverse(_data);
        return this;
    }

    /// <summary>
    /// Interprets buf as an array of unsigned 16-bit integers and swaps the byte order in-place.
    /// </summary>
    /// <returns>A reference to buf.</returns>
    public Buffer swap16()
    {
        if (length % 2 != 0)
            throw new ArgumentException("Buffer size must be a multiple of 16-bits");

        for (int i = 0; i < length; i += 2)
        {
            var temp = _data[i];
            _data[i] = _data[i + 1];
            _data[i + 1] = temp;
        }

        return this;
    }

    /// <summary>
    /// Interprets buf as an array of unsigned 32-bit integers and swaps the byte order in-place.
    /// </summary>
    /// <returns>A reference to buf.</returns>
    public Buffer swap32()
    {
        if (length % 4 != 0)
            throw new ArgumentException("Buffer size must be a multiple of 32-bits");

        for (int i = 0; i < length; i += 4)
        {
            var temp0 = _data[i];
            var temp1 = _data[i + 1];
            _data[i] = _data[i + 3];
            _data[i + 1] = _data[i + 2];
            _data[i + 2] = temp1;
            _data[i + 3] = temp0;
        }

        return this;
    }

    /// <summary>
    /// Interprets buf as an array of 64-bit numbers and swaps byte order in-place.
    /// </summary>
    /// <returns>A reference to buf.</returns>
    public Buffer swap64()
    {
        if (length % 8 != 0)
            throw new ArgumentException("Buffer size must be a multiple of 64-bits");

        for (int i = 0; i < length; i += 8)
        {
            var temp0 = _data[i];
            var temp1 = _data[i + 1];
            var temp2 = _data[i + 2];
            var temp3 = _data[i + 3];
            _data[i] = _data[i + 7];
            _data[i + 1] = _data[i + 6];
            _data[i + 2] = _data[i + 5];
            _data[i + 3] = _data[i + 4];
            _data[i + 4] = temp3;
            _data[i + 5] = temp2;
            _data[i + 6] = temp1;
            _data[i + 7] = temp0;
        }

        return this;
    }
}
