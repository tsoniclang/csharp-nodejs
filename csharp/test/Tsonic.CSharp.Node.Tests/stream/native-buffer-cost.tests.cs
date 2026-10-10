using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using Xunit;

namespace Tsonic.CSharp.Node.Tests;

[Collection(JsEventLoopCollection.Name)]
public class NativeReadableBufferCostTests
{
    [Fact]
    public void BothEndedQueuePreservesIdentityThroughWrappedGrowth()
    {
        var source = new Readable();
        var expected = new List<string>();
        for (var index = 0; index < 128; index++)
        {
            var value = $"initial-{index}";
            source.push(value);
            expected.Add(value);
        }
        for (var index = 0; index < 79; index++)
        {
            Assert.Same(expected[0], source.read());
            expected.RemoveAt(0);
        }
        for (var index = 0; index < 43; index++)
        {
            var value = $"préfix-{index}";
            source.unshift(value);
            expected.Insert(0, value);
        }
        for (var index = 0; index < 51; index++)
        {
            var value = $"suffix-{index}";
            source.push(value);
            expected.Add(value);
        }
        var bytes = 0;
        foreach (var value in expected)
            bytes += Encoding.UTF8.GetByteCount(value);
        Assert.Equal(bytes, source.readableLength);
        foreach (var value in expected)
        {
            Assert.Same(value, source.read());
            bytes -= Encoding.UTF8.GetByteCount(value);
            Assert.Equal(bytes, source.readableLength);
        }
        Assert.Null(source.read());
    }

    [Fact]
    public void CachedChunkSizesRetainPartialBufferAndEmptyChunkAccounting()
    {
        var source = new Readable();
        var original = Buffer.from("abcd");
        var empty = Buffer.alloc(0);
        source.push(original);
        source.push("é😀");
        source.unshift(empty);
        Assert.Equal(10, source.readableLength);
        Assert.Same(empty, source.read());
        Assert.Equal(10, source.readableLength);
        var first = source.readBuffer(2)!;
        Assert.Equal(8, source.readableLength);
        original[0] = (byte)'z';
        Assert.Equal("zb", first.toString());
        Assert.Equal("cd", source.readBuffer(2)!.toString());
        Assert.Equal(6, source.readableLength);
        Assert.Equal("é😀", source.read());
        Assert.Equal(0, source.readableLength);
    }

    [Fact]
    public void WarmQueueDoesNotAllocateNodesWhenChunksAreReused()
    {
        var source = new Readable();
        var chunk = new object();
        for (var index = 0; index < 100; index++)
        {
            source.push(chunk);
            _ = source.read();
        }
        var identical = true;
        var before = GC.GetAllocatedBytesForCurrentThread();
        for (var index = 0; index < 10_000; index++)
        {
            source.push(chunk);
            identical &= ReferenceEquals(chunk, source.read());
        }
        var allocated = GC.GetAllocatedBytesForCurrentThread() - before;
        Assert.True(identical);
        Assert.Equal(0, allocated);
    }

    [Theory]
    [InlineData("utf8", "A\0��")]
    [InlineData("UTF-8", "A\0��")]
    [InlineData("ascii", "A\0??")]
    [InlineData("latin1", "A\0\u0080ÿ")]
    [InlineData("binary", "A\0\u0080ÿ")]
    [InlineData("utf16le", "A\uff80")]
    [InlineData("UCS-2", "A\uff80")]
    [InlineData("hex", "410080ff")]
    [InlineData("base64", "QQCA/w==")]
    [InlineData("base64url", "QQCA_w")]
    public void EncodedByteChunksUseTheCanonicalNativeDecoder(string encoding, string expected)
    {
        var source = new Readable().setEncoding(encoding);
        var bytes = new byte[] { 0x41, 0x00, 0x80, 0xff };
        source.push(bytes);
        bytes[0] = 0;
        Assert.Equal(expected, source.read());
        Assert.Equal(0, source.readableLength);
        source.push(Array.Empty<byte>());
        Assert.Equal(string.Empty, source.read());
        Assert.Equal(0, source.readableLength);
    }

    [Fact]
    public void EncodedByteChunksAllocateOnlyTheNativeDecodedString()
    {
        var bytes = new byte[4096];
        Array.Fill(bytes, (byte)'x');
        var source = new Readable().setEncoding("utf8");
        for (var index = 0; index < 100; index++)
        {
            GC.KeepAlive(Encoding.UTF8.GetString(bytes));
            source.push(bytes);
            GC.KeepAlive(source.read());
        }
        var before = GC.GetAllocatedBytesForCurrentThread();
        for (var index = 0; index < 100; index++)
            GC.KeepAlive(Encoding.UTF8.GetString(bytes));
        var nativeAllocated = GC.GetAllocatedBytesForCurrentThread() - before;
        before = GC.GetAllocatedBytesForCurrentThread();
        for (var index = 0; index < 100; index++)
        {
            source.push(bytes);
            GC.KeepAlive(source.read());
        }
        var streamAllocated = GC.GetAllocatedBytesForCurrentThread() - before;
        Assert.Equal(nativeAllocated, streamAllocated);
        Assert.Equal(0, source.readableLength);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void RemovedAndDestroyedChunksDoNotRemainRootedInReusableSlots(bool destroy)
    {
        var source = new Readable();
        var tracked = PushTracked(source);
        if (destroy)
            source.destroy();
        else
            Drain(source);
        GC.Collect();
        GC.WaitForPendingFinalizers();
        GC.Collect();
        Assert.False(tracked.IsAlive);
        Assert.Equal(0, source.readableLength);
        GC.KeepAlive(source);
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static WeakReference PushTracked(Readable source)
    {
        var chunk = new object();
        source.push(chunk);
        return new WeakReference(chunk);
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static void Drain(Readable source) => _ = source.read();
}
