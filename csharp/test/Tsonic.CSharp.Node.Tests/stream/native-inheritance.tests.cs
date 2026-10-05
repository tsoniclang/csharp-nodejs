using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Tsonic.CSharp.Js;
using Xunit;

namespace Tsonic.CSharp.Node.Tests;

[Collection(JsEventLoopCollection.Name)]
public class NativeStreamInheritanceTests : FsTestBase
{
    [Fact]
    public void FileStream_InheritedMethodsPreserveIdentityPressureAndFinish()
    {
        var path = GetTestPath("native-stream.txt");
        var file = fs.createWriteStream(path, new WriteStreamOptions { highWaterMark = 1 });
        Writable writable = file;
        var removedCalls = 0;
        Action removed = () => removedCalls++;
        Action<Exception> removedError = error => removedCalls++;
        Assert.Same(file, file.on("error", removedError));
        Assert.Same(file, file.off("error", removedError));
        Assert.Same(file, file.once("error", removedError));
        file.off("error", removedError);
        Assert.Same(file, file.once("drain", removed));
        Assert.Same(file, file.off("drain", removed));
        Assert.Same(file, file.once("finish", removed));
        Assert.Same(file, file.off("finish", removed));

        var drainCalls = 0;
        var finishCalls = 0;
        var finished = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
        file.once("drain", () => drainCalls++);
        file.once("finish", () =>
        {
            finishCalls++;
            finished.TrySetResult(true);
        });
        file.on("error", (Action<Exception>)(error => finished.TrySetException(error)));
        using var deadline = new CancellationTokenSource(TimeSpan.FromSeconds(5));
        using var cancellation = deadline.Token.Register(() => finished.TrySetCanceled(deadline.Token));
        try
        {
            file.cork();
            Assert.False(file.write(Buffer.from("native stream")));
            Assert.True(writable.writableNeedDrain);
            Assert.Same(file, file.end());
            Assert.True(writable.writableEnded);
            JsEventLoop.Run(finished.Task);
        }
        finally
        {
            file.destroy();
        }
        Assert.Equal(0, removedCalls);
        Assert.Equal(1, drainCalls);
        Assert.Equal(1, finishCalls);
        Assert.True(writable.writableFinished);
        Assert.False(writable.writableNeedDrain);
        Assert.Equal(13, file.bytesWritten);
        Assert.Equal("native stream", File.ReadAllText(path));
    }

    [Fact]
    public void Zlib_InheritedDestructionPreservesTheOriginalStream()
    {
        var codec = zlib.createGzip();
        Transform transform = codec;
        Duplex duplex = transform;
        Assert.Same(codec, duplex.destroyChain());
        Assert.True(codec.destroyed);
        Assert.False(codec.writable);
        Assert.False(codec.readable);
    }

    [Fact]
    public void ProjectedCodec_DestructionRetainsErrorIdentityAndEmitsOnlyOnce()
    {
        var codec = zlib.createGzip();
        Duplex duplex = codec;
        var errorCalls = 0;
        Exception? received = null;
        duplex.once("error", (Action<Exception>)(error =>
        {
            errorCalls++;
            received = error;
        }));
        var error = new Exception("native codec failure");
        duplex.destroyChain(error);
        duplex.destroyChain(error);
        Assert.Equal(1, errorCalls);
        Assert.Same(error, received);
        Assert.True(codec.destroyed);
    }
}
