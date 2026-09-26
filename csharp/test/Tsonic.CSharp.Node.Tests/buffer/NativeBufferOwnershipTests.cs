using System;
using Xunit;

namespace Tsonic.CSharp.Node.Tests;

public class NativeBufferOwnershipTests
{
    [Fact]
    public void BufferFromUint8ArrayCopiesExactBytes()
    {
        var source = new Tsonic.CSharp.Js.Uint8Array(3);
        source[0] = 0;
        source[1] = 128;
        source[2] = 255;
        var result = Buffer.from(source);
        Assert.Equal(3, result.length);
        Assert.Equal(0, result[0]);
        Assert.Equal(128, result[1]);
        Assert.Equal(255, result[2]);
        source[1] = 4;
        Assert.Equal(128, result[1]);
    }

    [Fact]
    public void NestedViewsAliasOnlyTheirSelectedWindow()
    {
        var original = Buffer.from(new byte[] { 1, 2, 3, 4, 5 });
        var view = original.subarray(1, 4).slice(1);
        view[0] = 9;
        Assert.Equal(9, original[2]);
        view.fill(7);
        Assert.Equal(2, original[1]);
        Assert.Equal(7, original[2]);
        Assert.Equal(7, original[3]);
        Assert.Equal(5, original[4]);
        var independent = Buffer.from(view);
        independent[0] = 0;
        Assert.Equal(7, view[0]);
    }

    [Fact]
    public void HashCopiesCloneDigestStateNotRetainedInput()
    {
        var prefix = Buffer.alloc(4096, 97);
        var hash = crypto.createHash("sha256");
        for (var index = 0; index < 32; index++) hash.update(prefix);
        var copy = hash.copy();
        hash.update("left");
        copy.update("right");
        var left = crypto.createHash("sha256");
        var right = crypto.createHash("sha256");
        for (var index = 0; index < 32; index++) { left.update(prefix); right.update(prefix); }
        Assert.Equal(left.update("left").digest("hex"), hash.digest("hex"));
        Assert.Equal(right.update("right").digest("hex"), copy.digest("hex"));
        Assert.Throws<InvalidOperationException>(() => hash.update(prefix));
        Assert.Throws<InvalidOperationException>(() => hash.copy());
    }

    [Fact]
    public void MetadataDatesRemainStableAndIndependentOfNumericFields()
    {
        var stats = fs.statSync(".");
        var snapshot = stats.mtimeMs;
        stats.mtimeMs = 0;
        Assert.Equal(snapshot, stats.mtime.getTime());
        Assert.Same(stats.mtime, stats.mtime);
        var replacement = new Tsonic.CSharp.Js.Date(42);
        stats.mtime = replacement;
        Assert.Same(replacement, stats.mtime);
        Assert.Equal(0, stats.mtimeMs);
    }
}
