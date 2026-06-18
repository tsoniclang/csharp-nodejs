using System;
using System.Text;
using System.Linq;

namespace Tsonic.CSharp.Node;

/// <summary>
/// Buffer objects are used to represent a fixed-length sequence of bytes.
/// This class provides a C# implementation of Node.js Buffer API.
/// </summary>
public partial class Buffer
{
    private readonly byte[] _data;

    /// <summary>
    /// Gets the length of the buffer in bytes.
    /// </summary>
    public int length => _data.Length;

    /// <summary>
    /// Creates a new Buffer instance with the specified byte array.
    /// </summary>
    /// <param name="data">The byte array to wrap.</param>
    private Buffer(byte[] data)
    {
        _data = data;
    }

    /// <summary>
    /// Allows indexer access to buffer bytes.
    /// </summary>
    /// <param name="index">The zero-based index of the byte.</param>
    /// <returns>The byte at the specified index.</returns>
    public byte this[int index]
    {
        get => _data[index];
        set => _data[index] = value;
    }

    /// <summary>
    /// Gets the internal byte array. Use with caution.
    /// </summary>
    internal byte[] InternalData => _data;

    /// <summary>
    /// The size (in bytes) of pre-allocated internal Buffer instances used for pooling.
    /// This value may be modified.
    /// </summary>
    public static int poolSize { get; set; } = 8192;
}
