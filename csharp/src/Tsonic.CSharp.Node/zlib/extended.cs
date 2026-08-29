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
    BrotliDecompress
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
}

public static partial class zlib
{
    private static readonly ZlibConstants ConstantsValue = new();
    public static ZlibConstants constants => ConstantsValue;

    public static Buffer gzipSync(Buffer buffer, ZlibOptions? options = null) => Buffer.from(GzipBytes(BufferBytes(buffer), options));
    public static Buffer gunzipSync(Buffer buffer, ZlibOptions? options = null) => Buffer.from(GunzipBytes(BufferBytes(buffer), options));
    public static Buffer deflateSync(Buffer buffer, ZlibOptions? options = null) => Buffer.from(DeflateBytes(BufferBytes(buffer), options));
    public static Buffer inflateSync(Buffer buffer, ZlibOptions? options = null) => Buffer.from(InflateBytes(BufferBytes(buffer), options));
    public static Buffer deflateRawSync(Buffer buffer, ZlibOptions? options = null) => Buffer.from(DeflateRawBytes(BufferBytes(buffer), options));
    public static Buffer inflateRawSync(Buffer buffer, ZlibOptions? options = null) => Buffer.from(InflateRawBytes(BufferBytes(buffer), options));
    public static Buffer unzipSync(Buffer buffer, ZlibOptions? options = null) => Buffer.from(UnzipBytes(BufferBytes(buffer), options));
    public static Buffer brotliCompressSync(Buffer buffer, BrotliOptions? options = null) => Buffer.from(BrotliCompressBytes(BufferBytes(buffer), options));
    public static Buffer brotliDecompressSync(Buffer buffer, BrotliOptions? options = null) => Buffer.from(BrotliDecompressBytes(BufferBytes(buffer), options));
    public static void gzip(Buffer buffer, Action<Exception?, Buffer> callback) => CompleteBuffer(() => gzipSync(buffer), callback);
    public static void gzip(Buffer buffer, ZlibOptions options, Action<Exception?, Buffer> callback) => CompleteBuffer(() => gzipSync(buffer, options), callback);
    public static void gunzip(Buffer buffer, Action<Exception?, Buffer> callback) => CompleteBuffer(() => gunzipSync(buffer), callback);
    public static void gunzip(Buffer buffer, ZlibOptions options, Action<Exception?, Buffer> callback) => CompleteBuffer(() => gunzipSync(buffer, options), callback);
    public static void deflate(Buffer buffer, Action<Exception?, Buffer> callback) => CompleteBuffer(() => deflateSync(buffer), callback);
    public static void deflate(Buffer buffer, ZlibOptions options, Action<Exception?, Buffer> callback) => CompleteBuffer(() => deflateSync(buffer, options), callback);
    public static void inflate(Buffer buffer, Action<Exception?, Buffer> callback) => CompleteBuffer(() => inflateSync(buffer), callback);
    public static void inflate(Buffer buffer, ZlibOptions options, Action<Exception?, Buffer> callback) => CompleteBuffer(() => inflateSync(buffer, options), callback);
    public static void deflateRaw(Buffer buffer, Action<Exception?, Buffer> callback) => CompleteBuffer(() => deflateRawSync(buffer), callback);
    public static void deflateRaw(Buffer buffer, ZlibOptions options, Action<Exception?, Buffer> callback) => CompleteBuffer(() => deflateRawSync(buffer, options), callback);
    public static void inflateRaw(Buffer buffer, Action<Exception?, Buffer> callback) => CompleteBuffer(() => inflateRawSync(buffer), callback);
    public static void inflateRaw(Buffer buffer, ZlibOptions options, Action<Exception?, Buffer> callback) => CompleteBuffer(() => inflateRawSync(buffer, options), callback);
    public static void unzip(Buffer buffer, Action<Exception?, Buffer> callback) => CompleteBuffer(() => unzipSync(buffer), callback);
    public static void unzip(Buffer buffer, ZlibOptions options, Action<Exception?, Buffer> callback) => CompleteBuffer(() => unzipSync(buffer, options), callback);
    public static void brotliCompress(Buffer buffer, Action<Exception?, Buffer> callback) => CompleteBuffer(() => brotliCompressSync(buffer), callback);
    public static void brotliCompress(Buffer buffer, BrotliOptions options, Action<Exception?, Buffer> callback) => CompleteBuffer(() => brotliCompressSync(buffer, options), callback);
    public static void brotliDecompress(Buffer buffer, Action<Exception?, Buffer> callback) => CompleteBuffer(() => brotliDecompressSync(buffer), callback);
    public static void brotliDecompress(Buffer buffer, BrotliOptions options, Action<Exception?, Buffer> callback) => CompleteBuffer(() => brotliDecompressSync(buffer, options), callback);
    public static ZlibTransform createDeflate(ZlibOptions? options = null) => new(ZlibMode.Deflate, options);
    public static ZlibTransform createInflate(ZlibOptions? options = null) => new(ZlibMode.Inflate, options);
    public static ZlibTransform createGzip(ZlibOptions? options = null) => new(ZlibMode.Gzip, options);
    public static ZlibTransform createGunzip(ZlibOptions? options = null) => new(ZlibMode.Gunzip, options);
    public static ZlibTransform createDeflateRaw(ZlibOptions? options = null) => new(ZlibMode.DeflateRaw, options);
    public static ZlibTransform createInflateRaw(ZlibOptions? options = null) => new(ZlibMode.InflateRaw, options);
    public static ZlibTransform createUnzip(ZlibOptions? options = null) => new(ZlibMode.Unzip, options);
    public static ZlibTransform createBrotliCompress(BrotliOptions? options = null) => new(ZlibMode.BrotliCompress, brotliOptions: options);
    public static ZlibTransform createBrotliDecompress(BrotliOptions? options = null) => new(ZlibMode.BrotliDecompress, brotliOptions: options);
    private static byte[] BufferBytes(Buffer buffer)
    {
        ArgumentNullException.ThrowIfNull(buffer);
        return buffer.InternalData;
    }

    private static void CompleteBuffer(Func<Buffer> operation, Action<Exception?, Buffer> callback)
    {
        _ = BackgroundDispatch.RunReferenced(() =>
        {
            try
            {
                var result = operation();
                Tsonic.CSharp.Js.JsEventLoop.EnqueueReferenced(() => callback(null, result));
            }
            catch (Exception error)
            {
                Tsonic.CSharp.Js.JsEventLoop.EnqueueReferenced(() => callback(error, null!));
            }
        });
    }
}
