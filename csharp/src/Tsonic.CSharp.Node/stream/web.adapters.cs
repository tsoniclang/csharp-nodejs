#pragma warning disable CS1591
#pragma warning disable IDE1006

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Tsonic.CSharp.Node;

public sealed class GenericTransformStream
{
    public GenericTransformStream(ReadableStream readable, WritableStream writable)
    {
        this.readable = readable;
        this.writable = writable;
    }

    public ReadableStream readable { get; }

    public WritableStream writable { get; }
}

public sealed class ReadableWritablePair
{
    public ReadableWritablePair(ReadableStream readable, WritableStream writable)
    {
        this.readable = readable;
        this.writable = writable;
    }

    public ReadableStream readable { get; }

    public WritableStream writable { get; }
}

public sealed class TextEncoderStream
{
    public TextEncoderStream()
    {
        readable = new ReadableStream();
        writable = new WritableStream();
    }

    public ReadableStream readable { get; }

    public WritableStream writable { get; }
}

public sealed class TextDecoderStream
{
    public TextDecoderStream()
    {
        readable = new ReadableStream();
        writable = new WritableStream();
    }

    public ReadableStream readable { get; }

    public WritableStream writable { get; }
}

public sealed class CompressionStream
{
    public CompressionStream(string format)
    {
        this.format = format;
        readable = new ReadableStream();
        writable = new WritableStream();
    }

    public string format { get; }

    public ReadableStream readable { get; }

    public WritableStream writable { get; }
}

public sealed class DecompressionStream
{
    public DecompressionStream(string format)
    {
        this.format = format;
        readable = new ReadableStream();
        writable = new WritableStream();
    }

    public string format { get; }

    public ReadableStream readable { get; }

    public WritableStream writable { get; }
}

public partial class Readable
{
    public static ReadableStream toWeb(Readable readable)
    {
        if (readable == null)
            throw new ArgumentNullException(nameof(readable));

        var chunks = new List<object?>();
        object? chunk;
        while ((chunk = readable.read()) != null)
            chunks.Add(chunk);
        return ReadableStream.fromChunks(chunks.ToArray());
    }

    public static Readable fromWeb(ReadableStream stream)
    {
        if (stream == null)
            throw new ArgumentNullException(nameof(stream));

        var readable = new Readable();
        foreach (var chunk in stream.values())
            readable.push(chunk);
        readable.push(null);
        return readable;
    }
}

public partial class Writable
{
    public static WritableStream toWeb(Writable writable)
    {
        if (writable == null)
            throw new ArgumentNullException(nameof(writable));

        return new WritableStream();
    }

    public static Writable fromWeb(WritableStream stream)
    {
        if (stream == null)
            throw new ArgumentNullException(nameof(stream));

        var writable = new Writable();
        foreach (var chunk in stream.chunks)
            writable.write(chunk);
        return writable;
    }
}
