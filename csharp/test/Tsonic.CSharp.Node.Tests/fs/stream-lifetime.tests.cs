using System;
using System.IO;
using Tsonic.CSharp.Js;
using Xunit;

namespace Tsonic.CSharp.Node.Tests;

[Collection(JsEventLoopCollection.Name)]
public sealed class FsStreamLifetimeTests : FsTestBase
{
    [Fact]
    public void ReadStream_KeepsNativeIoAliveThroughTheFinalDataAndEnd()
    {
        var path = GetTestPath("read-lifetime.bin");
        var expected = new byte[1024 * 1024];
        System.Array.Fill(expected, (byte)7);
        File.WriteAllBytes(path, expected);
        var stream = fs.createReadStream(path, new ReadStreamOptions { highWaterMark = 4096 });
        var received = 0;
        var ends = 0;
        stream.on<Buffer>("data", bytes =>
        {
            Assert.Equal(0, ends);
            received += bytes.length;
        });
        stream.on("end", () => ends++);
        stream.on<Exception>("error", error => throw error);
        try
        {
            stream.resume();
            Assert.True(ProcessKeepAlive.HasReferences);
            JsEventLoop.Run();
            Assert.Equal(expected.Length, received);
            Assert.Equal(expected.Length, stream.bytesRead);
            Assert.Equal(1, ends);
            Assert.False(ProcessKeepAlive.HasReferences);
        }
        finally
        {
            stream.close();
        }
    }

    [Fact]
    public void WriteStream_KeepsNativeIoAliveThroughFinalization()
    {
        var path = GetTestPath("write-lifetime.txt");
        var stream = fs.createWriteStream(path);
        var finishes = 0;
        var closes = 0;
        stream.on("finish", () => finishes++);
        stream.on("close", () => closes++);
        stream.on<Exception>("error", error => throw error);
        try
        {
            stream.write("first");
            stream.end("second");
            Assert.True(ProcessKeepAlive.HasReferences);
            JsEventLoop.Run();
            Assert.Equal("firstsecond", File.ReadAllText(path));
            Assert.Equal(11, stream.bytesWritten);
            Assert.Equal(1, finishes);
            Assert.Equal(1, closes);
            Assert.False(ProcessKeepAlive.HasReferences);
        }
        finally
        {
            stream.destroy();
        }
    }

    [Fact]
    public void ReadStream_CancellationReleasesItsNativeIoReference()
    {
        var path = GetTestPath("cancel-lifetime.bin");
        File.WriteAllBytes(path, new byte[1024 * 1024]);
        var stream = fs.createReadStream(path);
        var closes = 0;
        stream.on("close", () => closes++);
        stream.resume();
        stream.close();
        JsEventLoop.Run();
        Assert.Equal(1, closes);
        Assert.True(stream.destroyed);
        Assert.False(ProcessKeepAlive.HasReferences);
    }
}
