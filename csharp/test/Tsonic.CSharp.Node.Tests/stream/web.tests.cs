using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Tsonic.CSharp.Node.Tests;

public class StreamWebTests
{
    [Fact]
    public async Task ReadableStream_ShouldReadCancelPipeAndTee()
    {
        var readable = ReadableStream.fromChunks("a", "b");
        var reader = readable.getReader();

        var first = await reader.read();
        Assert.False(first.done);
        Assert.Equal("a", first.value);
        Assert.True(readable.locked);

        reader.releaseLock();
        Assert.False(readable.locked);

        var writable = new WritableStream();
        await readable.pipeTo(writable);
        Assert.Equal(["b"], writable.chunks);
        Assert.True(writable.closed);

        var tee = ReadableStream.fromChunks(1, 2).tee();
        Assert.Equal([1, 2], tee[0].values().ToArray());
        Assert.Equal([1, 2], tee[1].values().ToArray());

        await readable.cancel("done");
        Assert.True(readable.canceled);
    }

    [Fact]
    public async Task WritableStream_ShouldWriteCloseAbortAndUseWriter()
    {
        var writable = new WritableStream();
        var writer = writable.getWriter();

        await writer.write("x");
        Assert.True(writable.locked);
        writer.releaseLock();
        Assert.False(writable.locked);

        await writable.write("y");
        await writable.close();
        Assert.True(writable.closed);
        Assert.Equal(["x", "y"], writable.chunks);

        var aborted = new WritableStream();
        await aborted.abort("stop");
        Assert.True(aborted.aborted);
    }

    [Fact]
    public async Task TransformStream_ShouldPassThrough()
    {
        var transform = new TransformStream();

        await transform.writePassthrough("chunk");

        Assert.Equal(["chunk"], transform.readable.values().ToArray());
        Assert.Equal(["chunk"], transform.writable.chunks);
    }

    [Fact]
    public void StrategiesControllersAndByob_ShouldExposeClosedState()
    {
        var count = new CountQueuingStrategy(new QueuingStrategyInit { highWaterMark = 2 });
        var bytes = new ByteLengthQueuingStrategy(new QueuingStrategyInit { highWaterMark = 10 });
        var stream = new ReadableStream();
        var controller = new ReadableStreamDefaultController(stream);
        var byteController = new ReadableByteStreamController(stream);
        var request = new ReadableStreamBYOBRequest(new byte[4]);

        controller.enqueue("a");
        byteController.setByobRequest(request);
        byteController.enqueue([1, 2]);
        request.respond(2);

        Assert.Equal(2, count.highWaterMark);
        Assert.Equal(3, bytes.size(new byte[3]));
        Assert.Equal(2, request.bytesWritten);
        Assert.Same(request, byteController.byobRequest);
        Assert.Equal(2, stream.chunks.Length);
    }

    [Fact]
    public void WritableAndTransformControllers_ShouldExposeErrorsAndChunks()
    {
        using var cancellation = new CancellationTokenSource();
        var writable = new WritableStreamDefaultController { signal = cancellation.Token };
        var transform = new TransformStreamDefaultController();

        cancellation.Cancel();
        writable.error("bad");
        transform.enqueue("x");

        Assert.True(writable.abortSignal);
        Assert.True(writable.errored);
        Assert.Equal(["x"], transform.chunks);

        transform.terminate();
        Assert.Empty(transform.chunks);
    }

    [Fact]
    public void TextAndCompressionStreams_ShouldExposeReadableWritablePairs()
    {
        var encoder = new TextEncoderStream();
        var decoder = new TextDecoderStream();
        var compression = new CompressionStream("gzip");
        var decompression = new DecompressionStream("gzip");
        var pair = new ReadableWritablePair(encoder.readable, encoder.writable);

        Assert.NotNull(pair.readable);
        Assert.NotNull(decoder.writable);
        Assert.Equal("gzip", compression.format);
        Assert.Equal("gzip", decompression.format);
    }

    [Fact]
    public async Task NodeReadableWritableAdapters_ShouldRoundTripChunks()
    {
        var readable = new Readable();
        readable.push("a");
        readable.push("b");
        var webReadable = Readable.toWeb(readable);

        Assert.Equal(["a", "b"], webReadable.values().ToArray());

        var webWritable = new WritableStream();
        await webWritable.write("x");
        var writable = Writable.fromWeb(webWritable);

        Assert.True(writable.writable);
    }
}
