using System;
using System.IO;
using System.IO.Compression;

namespace Tsonic.CSharp.Node;

/// <summary>
/// The zlib module provides compression functionality implemented using Gzip, Deflate, and Brotli.
/// </summary>
public static partial class zlib
{
    /// <summary>
    /// Compress data using Gzip.
    /// </summary>
    /// <param name="buffer">The data to compress.</param>
    /// <param name="options">Optional compression options.</param>
    /// <returns>The compressed data.</returns>
    private static byte[] GzipBytes(byte[] buffer, ZlibOptions? options = null)
    {
        if (buffer == null)
            throw new ArgumentNullException(nameof(buffer));
        ValidateZlibOptions(options);
        if (buffer.Length == 0)
        {
            return
            [
                0x1f, 0x8b, 0x08, 0x00, 0x00, 0x00, 0x00, 0x00,
                0x00, 0x03, 0x03, 0x00, 0x00, 0x00, 0x00, 0x00,
                0x00, 0x00, 0x00, 0x00,
            ];
        }

        var level = options?.level ?? -1; // Default compression
        var compressionLevel = level switch
        {
            0 => CompressionLevel.NoCompression,
            >= 1 and <= 5 => CompressionLevel.Fastest,
            >= 6 and <= 9 => CompressionLevel.Optimal,
            -1 => CompressionLevel.Optimal,
            _ => throw new ArgumentOutOfRangeException(nameof(options), "Zlib compression level must be -1 through 9.")
        };

        using var output = new BoundedMemoryStream(options?.maxOutputLength);
        using (var gzip = new GZipStream(output, compressionLevel))
        {
            gzip.Write(buffer, 0, buffer.Length);
            gzip.Flush();
        }
        return output.ToArray();
    }

    /// <summary>
    /// Decompress Gzip data.
    /// </summary>
    /// <param name="buffer">The compressed data.</param>
    /// <param name="options">Optional decompression options.</param>
    /// <returns>The decompressed data.</returns>
    private static byte[] GunzipBytes(byte[] buffer, ZlibOptions? options = null)
    {
        if (buffer == null)
            throw new ArgumentNullException(nameof(buffer));
        ValidateZlibOptions(options);

        using var input = new MemoryStream(buffer);
        using var gzip = new GZipStream(input, CompressionMode.Decompress);
        using var output = new BoundedMemoryStream(options?.maxOutputLength);

        gzip.CopyTo(output);
        return output.ToArray();
    }

    /// <summary>
    /// Compress data using Deflate.
    /// </summary>
    /// <param name="buffer">The data to compress.</param>
    /// <param name="options">Optional compression options.</param>
    /// <returns>The compressed data.</returns>
    private static byte[] DeflateBytes(byte[] buffer, ZlibOptions? options = null)
    {
        if (buffer == null)
            throw new ArgumentNullException(nameof(buffer));
        ValidateZlibOptions(options);
        if (buffer.Length == 0)
            return [0x78, 0x9c, 0x03, 0x00, 0x00, 0x00, 0x00, 0x01];

        var level = options?.level ?? -1;
        var compressionLevel = level switch
        {
            0 => CompressionLevel.NoCompression,
            >= 1 and <= 5 => CompressionLevel.Fastest,
            >= 6 and <= 9 => CompressionLevel.Optimal,
            -1 => CompressionLevel.Optimal,
            _ => throw new ArgumentOutOfRangeException(nameof(options), "Zlib compression level must be -1 through 9.")
        };

        using var output = new BoundedMemoryStream(options?.maxOutputLength);
        using (var deflate = new ZLibStream(output, compressionLevel))
        {
            deflate.Write(buffer, 0, buffer.Length);
        }
        return output.ToArray();
    }

    /// <summary>
    /// Decompress Deflate data.
    /// </summary>
    /// <param name="buffer">The compressed data.</param>
    /// <param name="options">Optional decompression options.</param>
    /// <returns>The decompressed data.</returns>
    private static byte[] InflateBytes(byte[] buffer, ZlibOptions? options = null)
    {
        if (buffer == null)
            throw new ArgumentNullException(nameof(buffer));
        ValidateZlibOptions(options);

        using var input = new MemoryStream(buffer);
        using var deflate = new ZLibStream(input, CompressionMode.Decompress);
        using var output = new BoundedMemoryStream(options?.maxOutputLength);

        deflate.CopyTo(output);
        return output.ToArray();
    }

    /// <summary>
    /// Compress data using Deflate without headers (raw deflate).
    /// </summary>
    /// <param name="buffer">The data to compress.</param>
    /// <param name="options">Optional compression options.</param>
    /// <returns>The compressed data.</returns>
    private static byte[] DeflateRawBytes(byte[] buffer, ZlibOptions? options = null)
    {
        if (buffer == null)
            throw new ArgumentNullException(nameof(buffer));
        ValidateZlibOptions(options);
        if (buffer.Length == 0)
            return [0x03, 0x00];
        var level = options?.level ?? -1;
        var compressionLevel = level switch
        {
            0 => CompressionLevel.NoCompression,
            >= 1 and <= 5 => CompressionLevel.Fastest,
            >= 6 and <= 9 or -1 => CompressionLevel.Optimal,
            _ => throw new ArgumentOutOfRangeException(nameof(options), "Zlib compression level must be -1 through 9."),
        };
        using var output = new BoundedMemoryStream(options?.maxOutputLength);
        using (var deflate = new DeflateStream(output, compressionLevel))
            deflate.Write(buffer, 0, buffer.Length);
        return output.ToArray();
    }

    /// <summary>
    /// Decompress raw Deflate data (without headers).
    /// </summary>
    /// <param name="buffer">The compressed data.</param>
    /// <param name="options">Optional decompression options.</param>
    /// <returns>The decompressed data.</returns>
    private static byte[] InflateRawBytes(byte[] buffer, ZlibOptions? options = null)
    {
        if (buffer == null)
            throw new ArgumentNullException(nameof(buffer));
        ValidateZlibOptions(options);
        using var input = new MemoryStream(buffer);
        using var deflate = new DeflateStream(input, CompressionMode.Decompress);
        using var output = new BoundedMemoryStream(options?.maxOutputLength);
        deflate.CopyTo(output);
        return output.ToArray();
    }

    /// <summary>
    /// Compress data using Brotli.
    /// </summary>
    /// <param name="buffer">The data to compress.</param>
    /// <param name="options">Optional compression options.</param>
    /// <returns>The compressed data.</returns>
    private static byte[] BrotliCompressBytes(byte[] buffer, BrotliOptions? options = null)
    {
        if (buffer == null)
            throw new ArgumentNullException(nameof(buffer));

        var quality = options?.quality ?? 11; // Default Brotli quality
        var compressionLevel = quality switch
        {
            >= 0 and <= 3 => CompressionLevel.Fastest,
            >= 4 and <= 8 => CompressionLevel.Optimal,
            >= 9 and <= 11 => CompressionLevel.SmallestSize,
            _ => throw new ArgumentOutOfRangeException(nameof(options), "Brotli quality must be 0 through 11.")
        };

        using var output = new BoundedMemoryStream(options?.maxOutputLength);
        using (var brotli = new BrotliStream(output, compressionLevel))
        {
            brotli.Write(buffer, 0, buffer.Length);
        }
        return output.ToArray();
    }

    /// <summary>
    /// Decompress Brotli data.
    /// </summary>
    /// <param name="buffer">The compressed data.</param>
    /// <param name="options">Optional decompression options.</param>
    /// <returns>The decompressed data.</returns>
    private static byte[] BrotliDecompressBytes(byte[] buffer, BrotliOptions? options = null)
    {
        if (buffer == null)
            throw new ArgumentNullException(nameof(buffer));

        using var input = new MemoryStream(buffer);
        using var brotli = new BrotliStream(input, CompressionMode.Decompress);
        using var output = new BoundedMemoryStream(options?.maxOutputLength);

        brotli.CopyTo(output);
        return output.ToArray();
    }

    /// <summary>
    /// Decompress data. Auto-detects Gzip or Deflate format.
    /// </summary>
    /// <param name="buffer">The compressed data.</param>
    /// <param name="options">Optional decompression options.</param>
    /// <returns>The decompressed data.</returns>
    private static byte[] UnzipBytes(byte[] buffer, ZlibOptions? options = null)
    {
        if (buffer == null)
            throw new ArgumentNullException(nameof(buffer));

        if (buffer.Length < 2)
            throw new ArgumentException("Buffer too small to determine compression format");

        // Detect format by magic bytes
        // Gzip: 0x1f 0x8b
        // Zlib (Deflate with header): 0x78 (multiple variations)
        if (buffer[0] == 0x1f && buffer[1] == 0x8b)
        {
            return GunzipBytes(buffer, options);
        }
        else if (buffer[0] == 0x78)
        {
            // Zlib format (deflate with header)
            return InflateBytes(buffer, options);
        }
        else
        {
            // Try raw deflate
            return InflateRawBytes(buffer, options);
        }
    }

    /// <summary>
    /// Calculate CRC32 checksum.
    /// </summary>
    /// <param name="data">The data to checksum.</param>
    /// <param name="value">Optional initial CRC value.</param>
    /// <returns>The CRC32 checksum as an unsigned 32-bit integer.</returns>
    public static uint crc32(byte[] data, uint value = 0)
    {
        if (data == null)
            throw new ArgumentNullException(nameof(data));

        // If value is provided, use it as the starting CRC (inverted)
        // Otherwise, start with 0xFFFFFFFF (standard CRC32 initial value)
        var initialCrc = value == 0 ? 0xFFFFFFFF : ~value;
        return ComputeCrc32(data, initialCrc);
    }

    private static uint ComputeCrc32(byte[] data, uint crc)
    {
        // CRC32 polynomial table
        var table = GetCrc32Table();

        foreach (var b in data)
        {
            crc = (crc >> 8) ^ table[(crc ^ b) & 0xFF];
        }

        return ~crc;
    }

    private static uint[] GetCrc32Table()
    {
        var table = new uint[256];
        const uint polynomial = 0xEDB88320;

        for (uint i = 0; i < 256; i++)
        {
            uint crc = i;
            for (uint j = 0; j < 8; j++)
            {
                crc = (crc & 1) == 1 ? (crc >> 1) ^ polynomial : crc >> 1;
            }
            table[i] = crc;
        }

        return table;
    }

    private static void ValidateZlibOptions(ZlibOptions? options)
    {
        if (options?.chunkSize is <= 0)
            throw new ArgumentOutOfRangeException(nameof(options), "Codec chunk size must be positive.");
        if (options?.maxOutputLength is <= 0)
            throw new ArgumentOutOfRangeException(nameof(options), "Maximum codec output length must be positive.");
    }

    /// <summary>
    /// Calculate CRC32 checksum for a string.
    /// </summary>
    /// <param name="data">The string to checksum.</param>
    /// <param name="value">Optional initial CRC value.</param>
    /// <returns>The CRC32 checksum as an unsigned 32-bit integer.</returns>
    public static uint crc32(string data, uint value = 0)
    {
        if (data == null)
            throw new ArgumentNullException(nameof(data));

        var bytes = System.Text.Encoding.UTF8.GetBytes(data);
        return crc32(bytes, value);
    }
}
