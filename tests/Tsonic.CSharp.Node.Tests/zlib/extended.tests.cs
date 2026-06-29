using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Tsonic.CSharp.Node.Tests;

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
    public async Task AsyncGzip_RoundTrips()
    {
        var input = Encoding.UTF8.GetBytes("async gzip");
        var compressed = await zlib.gzip(input);
        var output = await zlib.gunzip(compressed);

        Assert.Equal("async gzip", Encoding.UTF8.GetString(output));
    }

    [Fact]
    public void CreateGzipTransform_RoundTrips()
    {
        var input = Encoding.UTF8.GetBytes("transform gzip");
        var gzip = zlib.createGzip();
        var gunzip = zlib.createGunzip();

        var output = gunzip.transform(gzip.transform(input));

        Assert.Equal("transform gzip", Encoding.UTF8.GetString(output));
    }

    [Fact]
    public void ZstdAlias_RoundTripsThroughClosedManagedCarrier()
    {
        var input = Encoding.UTF8.GetBytes("zstd alias");

        var compressed = zlib.zstdCompressSync(input);
        var output = zlib.zstdDecompressSync(compressed);

        Assert.Equal("zstd alias", Encoding.UTF8.GetString(output));
    }

    [Fact]
    public void GzipStringHelper_RoundTrips()
    {
        var compressed = zlib.gzipStringSync("string gzip");

        Assert.Equal("string gzip", zlib.gunzipStringSync(compressed));
    }
}
