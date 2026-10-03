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
    public void AsyncDecompression_DistinguishesFailureFromSuccessfulEmptyOutput()
    {
        var invalid = Buffer.from("invalid compressed data");
        var gzip = zlib.gzipSync(Buffer.alloc(0));
        var deflate = zlib.deflateSync(Buffer.alloc(0));
        var failures = 0;
        var successes = 0;
        Action<Exception?, Buffer?> failure = (error, output) =>
        {
            Assert.NotNull(error);
            Assert.NotEmpty(error.Message);
            Assert.Null(output);
            failures++;
        };
        Action<Exception?, Buffer?> success = (error, output) =>
        {
            Assert.Null(error);
            Assert.NotNull(output);
            Assert.Equal(0, output.length);
            successes++;
        };

        zlib.gunzip(invalid, failure);
        zlib.inflate(invalid, failure);
        zlib.gunzip(invalid, new ZlibOptions(), failure);
        zlib.inflate(invalid, new ZlibOptions(), failure);
        zlib.gunzip(gzip, success);
        zlib.inflate(deflate, success);
        zlib.gunzip(gzip, new ZlibOptions(), success);
        zlib.inflate(deflate, new ZlibOptions(), success);
        JsEventLoop.Run();

        Assert.Equal(4, failures);
        Assert.Equal(4, successes);
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
