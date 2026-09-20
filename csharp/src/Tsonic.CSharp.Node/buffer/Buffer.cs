using System;
using System.Text;
using System.Linq;
using Tsonic.CSharp.Runtime;

namespace Tsonic.CSharp.Node;

/// <summary>
/// Buffer objects are used to represent a fixed-length sequence of bytes.
/// This class provides a C# implementation of Node.js Buffer API.
/// </summary>
public partial class Buffer : ITsClosedValueCarrier
{
    private readonly Memory<byte> _memory;
    private Span<byte> _data => _memory.Span;

    /// <summary>
    /// Gets the length of the buffer in bytes.
    /// </summary>
    public int length => _data.Length;

    /// <summary>
    /// Creates a new Buffer instance with the specified byte array.
    /// </summary>
    /// <param name="data">The byte array to wrap.</param>
    private Buffer(Memory<byte> data)
    {
        _memory = data;
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
    /// Gets the exact writable memory range.
    /// </summary>
    internal Memory<byte> InternalMemory => _memory;

    internal static Buffer TakeOwnership(byte[] data) => new(data);

    internal static Buffer TakeOwnership(Memory<byte> data) => new(data);

    /// <summary>
    /// The size (in bytes) of pre-allocated internal Buffer instances used for pooling.
    /// This value may be modified.
    /// </summary>
    public static int poolSize { get; set; } = 8192;
}
