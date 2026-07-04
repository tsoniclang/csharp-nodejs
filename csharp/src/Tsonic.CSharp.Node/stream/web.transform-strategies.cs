#pragma warning disable CS1591
#pragma warning disable IDE1006

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Tsonic.CSharp.Node;

public sealed class TransformStream
{
    public TransformStream()
    {
        readable = new ReadableStream();
        writable = new WritableStream();
    }

    public ReadableStream readable { get; }

    public WritableStream writable { get; }

    public async Task writePassthrough(object? chunk)
    {
        await writable.write(chunk).ConfigureAwait(false);
        readable.Enqueue(chunk);
    }
}

public sealed class QueuingStrategyInit
{
    public double highWaterMark { get; set; } = 1;
}

public class QueuingStrategy
{
    public QueuingStrategy(double highWaterMark = 1)
    {
        this.highWaterMark = highWaterMark;
    }

    public double highWaterMark { get; }

    public virtual double size(object? chunk)
    {
        _ = chunk;
        return 1;
    }
}

public sealed class CountQueuingStrategy : QueuingStrategy
{
    public CountQueuingStrategy(QueuingStrategyInit init) : base(init.highWaterMark)
    {
    }
}

public sealed class ByteLengthQueuingStrategy : QueuingStrategy
{
    public ByteLengthQueuingStrategy(QueuingStrategyInit init) : base(init.highWaterMark)
    {
    }

    public override double size(object? chunk)
    {
        return chunk switch
        {
            byte[] bytes => bytes.Length,
            Buffer buffer => buffer.length,
            string text => text.Length,
            _ => 1
        };
    }
}
