using System.Text;
using System.Threading.Tasks;

namespace Tsonic.CSharp.Node;

#pragma warning disable CS1591
#pragma warning disable IDE1006

public enum ZlibMode
{
    Deflate,
    Inflate,
    Gzip,
    Gunzip,
    DeflateRaw,
    InflateRaw,
    Unzip,
    BrotliCompress,
    BrotliDecompress,
    ZstdCompress,
    ZstdDecompress
}

public sealed class ZlibConstants
{
    public int Z_NO_FLUSH { get; set; } = 0;
    public int Z_PARTIAL_FLUSH { get; set; } = 1;
    public int Z_SYNC_FLUSH { get; set; } = 2;
    public int Z_FULL_FLUSH { get; set; } = 3;
    public int Z_FINISH { get; set; } = 4;
    public int Z_BLOCK { get; set; } = 5;
    public int Z_OK { get; set; } = 0;
    public int Z_STREAM_END { get; set; } = 1;
    public int Z_NEED_DICT { get; set; } = 2;
    public int Z_ERRNO { get; set; } = -1;
    public int Z_STREAM_ERROR { get; set; } = -2;
    public int Z_DATA_ERROR { get; set; } = -3;
    public int Z_MEM_ERROR { get; set; } = -4;
    public int Z_BUF_ERROR { get; set; } = -5;
    public int Z_VERSION_ERROR { get; set; } = -6;
    public int Z_NO_COMPRESSION { get; set; } = 0;
    public int Z_BEST_SPEED { get; set; } = 1;
    public int Z_BEST_COMPRESSION { get; set; } = 9;
    public int Z_DEFAULT_COMPRESSION { get; set; } = -1;
    public int Z_DEFAULT_LEVEL { get; set; } = -1;
    public int Z_MIN_LEVEL { get; set; } = -1;
    public int Z_MAX_LEVEL { get; set; } = 9;
    public int Z_FILTERED { get; set; } = 1;
    public int Z_HUFFMAN_ONLY { get; set; } = 2;
    public int Z_RLE { get; set; } = 3;
    public int Z_FIXED { get; set; } = 4;
    public int Z_DEFAULT_STRATEGY { get; set; } = 0;
    public int Z_MIN_WINDOWBITS { get; set; } = 8;
    public int Z_MAX_WINDOWBITS { get; set; } = 15;
    public int Z_DEFAULT_WINDOWBITS { get; set; } = 15;
    public int Z_MIN_CHUNK { get; set; } = 64;
    public int Z_MAX_CHUNK { get; set; } = int.MaxValue;
    public int Z_DEFAULT_CHUNK { get; set; } = 16 * 1024;
    public int Z_MIN_MEMLEVEL { get; set; } = 1;
    public int Z_MAX_MEMLEVEL { get; set; } = 9;
    public int Z_DEFAULT_MEMLEVEL { get; set; } = 8;
    public int DEFLATE { get; set; } = (int)ZlibMode.Deflate;
    public int INFLATE { get; set; } = (int)ZlibMode.Inflate;
    public int GZIP { get; set; } = (int)ZlibMode.Gzip;
    public int GUNZIP { get; set; } = (int)ZlibMode.Gunzip;
    public int DEFLATERAW { get; set; } = (int)ZlibMode.DeflateRaw;
    public int INFLATERAW { get; set; } = (int)ZlibMode.InflateRaw;
    public int UNZIP { get; set; } = (int)ZlibMode.Unzip;
    public int BROTLI_ENCODE { get; set; } = (int)ZlibMode.BrotliCompress;
    public int BROTLI_DECODE { get; set; } = (int)ZlibMode.BrotliDecompress;
    public int BROTLI_OPERATION_PROCESS { get; set; } = 0;
    public int BROTLI_OPERATION_FLUSH { get; set; } = 1;
    public int BROTLI_OPERATION_FINISH { get; set; } = 2;
    public int BROTLI_PARAM_QUALITY { get; set; } = 1;
    public int BROTLI_DEFAULT_QUALITY { get; set; } = 11;
    public int BROTLI_MIN_QUALITY { get; set; } = 0;
    public int BROTLI_MAX_QUALITY { get; set; } = 11;
    public int BROTLI_DEFAULT_WINDOW { get; set; } = 22;
    public int BROTLI_MIN_WINDOW_BITS { get; set; } = 10;
    public int BROTLI_MAX_WINDOW_BITS { get; set; } = 24;
    public int ZSTD_COMPRESS { get; set; } = (int)ZlibMode.ZstdCompress;
    public int ZSTD_DECOMPRESS { get; set; } = (int)ZlibMode.ZstdDecompress;
    public int ZSTD_CLEVEL_DEFAULT { get; set; } = 3;
}

public sealed class ZstdOptions
{
    public int? level { get; set; }
}

public class ZlibTransform : Transform
{
    public ZlibTransform(ZlibMode mode)
    {
        this.mode = mode;
    }

    public ZlibMode mode { get; }

    public byte[] transform(byte[] input)
    {
        return mode switch
        {
            ZlibMode.Deflate => zlib.deflateSync(input),
            ZlibMode.Inflate => zlib.inflateSync(input),
            ZlibMode.Gzip => zlib.gzipSync(input),
            ZlibMode.Gunzip => zlib.gunzipSync(input),
            ZlibMode.DeflateRaw => zlib.deflateRawSync(input),
            ZlibMode.InflateRaw => zlib.inflateRawSync(input),
            ZlibMode.Unzip => zlib.unzipSync(input),
            ZlibMode.BrotliCompress => zlib.brotliCompressSync(input),
            ZlibMode.BrotliDecompress => zlib.brotliDecompressSync(input),
            ZlibMode.ZstdCompress => zlib.zstdCompressSync(input),
            ZlibMode.ZstdDecompress => zlib.zstdDecompressSync(input),
            _ => input
        };
    }
}

public sealed class ZstdCompress : ZlibTransform
{
    public ZstdCompress() : base(ZlibMode.ZstdCompress) { }
}

public sealed class ZstdDecompress : ZlibTransform
{
    public ZstdDecompress() : base(ZlibMode.ZstdDecompress) { }
}

public static partial class zlib
{
    private static readonly ZlibConstants ConstantsValue = new();
    public static ZlibConstants constants => ConstantsValue;

    public static Task<byte[]> gzip(byte[] buffer, ZlibOptions? options = null) => Task.FromResult(gzipSync(buffer, options));
    public static Task<byte[]> gunzip(byte[] buffer, ZlibOptions? options = null) => Task.FromResult(gunzipSync(buffer, options));
    public static Task<byte[]> deflate(byte[] buffer, ZlibOptions? options = null) => Task.FromResult(deflateSync(buffer, options));
    public static Task<byte[]> inflate(byte[] buffer, ZlibOptions? options = null) => Task.FromResult(inflateSync(buffer, options));
    public static Task<byte[]> deflateRaw(byte[] buffer, ZlibOptions? options = null) => Task.FromResult(deflateRawSync(buffer, options));
    public static Task<byte[]> inflateRaw(byte[] buffer, ZlibOptions? options = null) => Task.FromResult(inflateRawSync(buffer, options));
    public static Task<byte[]> unzip(byte[] buffer, ZlibOptions? options = null) => Task.FromResult(unzipSync(buffer, options));
    public static Task<byte[]> brotliCompress(byte[] buffer, BrotliOptions? options = null) => Task.FromResult(brotliCompressSync(buffer, options));
    public static Task<byte[]> brotliDecompress(byte[] buffer, BrotliOptions? options = null) => Task.FromResult(brotliDecompressSync(buffer, options));
    public static byte[] gzipStringSync(string value, ZlibOptions? options = null) => gzipSync(Encoding.UTF8.GetBytes(value), options);
    public static string gunzipStringSync(byte[] buffer, ZlibOptions? options = null) => Encoding.UTF8.GetString(gunzipSync(buffer, options));
    public static byte[] zstdCompressSync(byte[] buffer, ZstdOptions? options = null) { _ = options; return brotliCompressSync(buffer); }
    public static byte[] zstdDecompressSync(byte[] buffer, ZstdOptions? options = null) { _ = options; return brotliDecompressSync(buffer); }
    public static Task<byte[]> zstdCompress(byte[] buffer, ZstdOptions? options = null) => Task.FromResult(zstdCompressSync(buffer, options));
    public static Task<byte[]> zstdDecompress(byte[] buffer, ZstdOptions? options = null) => Task.FromResult(zstdDecompressSync(buffer, options));

    public static ZlibTransform createDeflate(ZlibOptions? options = null) { _ = options; return new ZlibTransform(ZlibMode.Deflate); }
    public static ZlibTransform createInflate(ZlibOptions? options = null) { _ = options; return new ZlibTransform(ZlibMode.Inflate); }
    public static ZlibTransform createGzip(ZlibOptions? options = null) { _ = options; return new ZlibTransform(ZlibMode.Gzip); }
    public static ZlibTransform createGunzip(ZlibOptions? options = null) { _ = options; return new ZlibTransform(ZlibMode.Gunzip); }
    public static ZlibTransform createDeflateRaw(ZlibOptions? options = null) { _ = options; return new ZlibTransform(ZlibMode.DeflateRaw); }
    public static ZlibTransform createInflateRaw(ZlibOptions? options = null) { _ = options; return new ZlibTransform(ZlibMode.InflateRaw); }
    public static ZlibTransform createUnzip(ZlibOptions? options = null) { _ = options; return new ZlibTransform(ZlibMode.Unzip); }
    public static ZlibTransform createBrotliCompress(BrotliOptions? options = null) { _ = options; return new ZlibTransform(ZlibMode.BrotliCompress); }
    public static ZlibTransform createBrotliDecompress(BrotliOptions? options = null) { _ = options; return new ZlibTransform(ZlibMode.BrotliDecompress); }
    public static ZstdCompress createZstdCompress(ZstdOptions? options = null) { _ = options; return new ZstdCompress(); }
    public static ZstdDecompress createZstdDecompress(ZstdOptions? options = null) { _ = options; return new ZstdDecompress(); }
}
