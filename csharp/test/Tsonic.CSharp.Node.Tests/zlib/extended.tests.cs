using System.Collections.Generic;
using System.Linq;
using Tsonic.CSharp.Js;
using Xunit;

namespace Tsonic.CSharp.Node.Tests;

[Collection(JsEventLoopCollection.Name)]
public class ZlibExtendedTests
{
    [Fact]
    public void Constants_ExposeCoreValues()
    {
        Assert.Equal(0, zlib.constants.Z_NO_FLUSH);
        Assert.Equal(9, zlib.constants.Z_BEST_COMPRESSION);
        Assert.Equal((int)ZlibMode.Gzip, zlib.constants.GZIP);
        Assert.Equal(11, zlib.constants.BROTLI_MAX_QUALITY);
    }

    [Fact]
    public void AsyncGzip_RoundTrips()
    {
        Buffer? compressed = null;
        Buffer? output = null;
        Exception? failure = null;

        zlib.gzip(Buffer.from("async gzip"), (error, result) =>
        {
            failure = error;
            compressed = result;
        });
        JsEventLoop.Run();
        Assert.Null(failure);
        Assert.NotNull(compressed);

        zlib.gunzip(compressed!, (error, result) =>
        {
            failure = error;
            output = result;
        });
        JsEventLoop.Run();

        Assert.Null(failure);
        Assert.Equal("async gzip", output!.toString());
    }

    [Fact]
    public void CreateGzipTransform_RoundTrips()
    {
        var gzip = zlib.createGzip();
        var gunzip = zlib.createGunzip();
        var chunks = new List<Buffer>();
        gunzip.on<Buffer>("data", chunks.Add);
        gzip.pipe(gunzip);

        gzip.end(Buffer.from("transform gzip"));
        JsEventLoop.Run();

        Assert.Equal("transform gzip", string.Concat(chunks.Select(chunk => chunk.toString())));
    }
}
