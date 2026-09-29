using System;
using System.Collections.Generic;
using Xunit;

namespace Tsonic.CSharp.Node.Tests;

public class ReadableFromTests
{
    [Fact]
    public void ReadableFromPreservesChunkIdentityForArraysAndLists()
    {
        var chunk = Buffer.from("data");
        foreach (IReadOnlyList<Buffer> input in new IReadOnlyList<Buffer>[] { new[] { chunk }, new List<Buffer> { chunk } })
        {
            var readable = Readable.from(input);
            Assert.Same(chunk, readable.read());
            Assert.Null(readable.read());
        }
    }

    [Fact]
    public void ReadableFromRejectsNullCollectionsAndChunks()
    {
        Assert.Throws<ArgumentNullException>(() => Readable.from(null!));
        Assert.Throws<ArgumentException>(() => Readable.from(new Buffer[] { null! }));
    }
}
