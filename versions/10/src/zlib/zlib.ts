/**
 * Node.js zlib module — compression functionality using Gzip, Deflate, and Brotli.
 *
 *
 * NOTE: This implementation uses System.IO.Compression (GZipStream, DeflateStream,
 * BrotliStream). The native implementation delegates to the platform's native
 * zlib / brotli via CompressionStream/DecompressionStream where available,
 * with TODO placeholders for sync variants that require native bindings.
 */
import type { BrotliOptions } from "./brotli-options.ts";
import type { ZlibOptions } from "./zlib-options.ts";
import type { byte, int } from "@tsonic/core/types.js";
import { Convert } from "@tsonic/dotnet/System.js";
import {
  BrotliStream,
  CompressionLevel,
  CompressionMode,
  DeflateStream,
  GZipStream,
} from "@tsonic/dotnet/System.IO.Compression.js";
import { List } from "@tsonic/dotnet/System.Collections.Generic.js";
import { MemoryStream } from "@tsonic/dotnet/System.IO.js";
import { stringToBytes } from "../buffer/buffer-encoding.ts";

// ---------------------------------------------------------------------------
// CRC-32 table (computed once, standard IEEE polynomial 0xEDB88320)
// ---------------------------------------------------------------------------

const buildCrc32Table = (): number[] => {
  const table = new List<number>();
  const polynomial = 0xedb88320;
  for (let i = 0; i < 256; i += 1) {
    let crc = i;
    for (let j = 0; j < 8; j += 1) {
      crc = (crc & 1) === 1 ? (crc >>> 1) ^ polynomial : crc >>> 1;
    }
    table.Add(crc < 0 ? crc + 4294967296 : crc);
  }
  return table.ToArray();
};

const CRC32_TABLE: number[] = buildCrc32Table();

const toInt = (value: number): int => {
  if (
    Number.isInteger(value) &&
    value >= -2147483648 &&
    value <= 2147483647
  ) {
    return value as int;
  }

  throw new RangeError("Expected Int32-compatible numeric value");
};

const toByteArray = (buffer: Uint8Array): byte[] => {
  return Array.from(buffer, (value) => Convert.ToByte(value));
};

const fromByteArray = (buffer: byte[]): Uint8Array => {
  return new Uint8Array(buffer);
};

const toCompressionLevel = (level?: number): CompressionLevel => {
  if (level === null || level === undefined) {
    return CompressionLevel.Optimal;
  }
  if (level === 0) {
    return CompressionLevel.NoCompression;
  }
  if (level <= 1) {
    return CompressionLevel.Fastest;
  }
  if (level >= 9) {
    return CompressionLevel.SmallestSize;
  }
  return CompressionLevel.Optimal;
};

const normalizeUnsigned32 = (value: number): number => {
  if (value < 0) {
    return value + 4294967296.0;
  }
  return value;
};

const invertUnsigned32 = (value: number): number => {
  const normalized = normalizeUnsigned32(value);
  return normalizeUnsigned32(4294967295.0 - normalized);
};

// ---------------------------------------------------------------------------
// crc32
// ---------------------------------------------------------------------------

/**
 * Calculate CRC32 checksum for a Uint8Array.
 *
 * @param data  The data to checksum.
 * @param value Optional initial CRC value (for incremental computation).
 * @returns The CRC32 checksum as an unsigned 32-bit integer.
 */
export const crc32 = (data: Uint8Array, value: number = 0): number => {
  if (data === null || data === undefined) {
    throw new Error("data must not be null");
  }

  const initialCrc = value === 0 ? 4294967295.0 : invertUnsigned32(value);
  const result = computeCrc32(data, normalizeUnsigned32(initialCrc));
  return normalizeUnsigned32(result);
};

/**
 * Calculate CRC32 checksum for a string (UTF-8 encoded).
 *
 * @param data  The string to checksum.
 * @param value Optional initial CRC value.
 * @returns The CRC32 checksum as an unsigned 32-bit integer.
 */
export const crc32String = (data: string, value: number = 0): number => {
  if (data === null || data === undefined) {
    throw new Error("data must not be null");
  }

  const bytes = stringToBytes(data, "utf8");
  return crc32(bytes, value);
};

const computeCrc32 = (data: Uint8Array, crc: number): number => {
  let current = crc;
  for (let i = 0; i < data.length; i += 1) {
    const tableIndex = toInt((current ^ data[i]!) & 0xff);
    const tableValue = CRC32_TABLE[tableIndex] ?? 0;
    current = (current >>> 8) ^ tableValue;
  }
  return invertUnsigned32(current);
};

// ---------------------------------------------------------------------------
// Gzip
// ---------------------------------------------------------------------------

/**
 * Compress data using Gzip.
 *
 * @param buffer  The data to compress.
 * @param options Optional compression options.
 * @returns The compressed data.
 */
export const gzipSync = (
  buffer: Uint8Array,
  options?: ZlibOptions,
): Uint8Array => {
  if (buffer === null || buffer === undefined) {
    throw new Error("buffer must not be null");
  }

  const output = new MemoryStream();
  const level = options !== undefined ? options.level : undefined;
  const stream = new GZipStream(output, toCompressionLevel(level), true);
  const inputBytes = toByteArray(buffer);
  stream.Write(inputBytes, 0 as int, toInt(inputBytes.length));
  stream.Dispose();
  return fromByteArray(output.ToArray());
};

/**
 * Decompress Gzip data.
 *
 * @param buffer  The compressed data.
 * @param options Optional decompression options.
 * @returns The decompressed data.
 */
export const gunzipSync = (
  buffer: Uint8Array,
  _options?: ZlibOptions,
): Uint8Array => {
  if (buffer === null || buffer === undefined) {
    throw new Error("buffer must not be null");
  }

  const input = new MemoryStream(toByteArray(buffer));
  const stream = new GZipStream(input, CompressionMode.Decompress, true);
  const output = new MemoryStream();
  stream.CopyTo(output);
  stream.Dispose();
  return fromByteArray(output.ToArray());
};

// ---------------------------------------------------------------------------
// Deflate
// ---------------------------------------------------------------------------

/**
 * Compress data using Deflate.
 *
 * @param buffer  The data to compress.
 * @param options Optional compression options.
 * @returns The compressed data.
 */
export const deflateSync = (
  buffer: Uint8Array,
  options?: ZlibOptions,
): Uint8Array => {
  if (buffer === null || buffer === undefined) {
    throw new Error("buffer must not be null");
  }

  const output = new MemoryStream();
  const level = options !== undefined ? options.level : undefined;
  const stream = new DeflateStream(output, toCompressionLevel(level), true);
  const inputBytes = toByteArray(buffer);
  stream.Write(inputBytes, 0 as int, toInt(inputBytes.length));
  stream.Dispose();
  return fromByteArray(output.ToArray());
};

/**
 * Decompress Deflate data.
 *
 * @param buffer  The compressed data.
 * @param options Optional decompression options.
 * @returns The decompressed data.
 */
export const inflateSync = (
  buffer: Uint8Array,
  _options?: ZlibOptions,
): Uint8Array => {
  if (buffer === null || buffer === undefined) {
    throw new Error("buffer must not be null");
  }

  const input = new MemoryStream(toByteArray(buffer));
  const stream = new DeflateStream(input, CompressionMode.Decompress, true);
  const output = new MemoryStream();
  stream.CopyTo(output);
  stream.Dispose();
  return fromByteArray(output.ToArray());
};

/**
 * Compress data using raw Deflate (no zlib wrapper headers).
 *
 * @param buffer  The data to compress.
 * @param options Optional compression options.
 * @returns The compressed data.
 */
export const deflateRawSync = (
  buffer: Uint8Array,
  options?: ZlibOptions,
): Uint8Array => {
  // .NET's DeflateStream is already "raw" deflate (no zlib wrapper).
  // Same applies to the native implementation.
  return deflateSync(buffer, options);
};

/**
 * Decompress raw Deflate data (without zlib wrapper headers).
 *
 * @param buffer  The compressed data.
 * @param options Optional decompression options.
 * @returns The decompressed data.
 */
export const inflateRawSync = (
  buffer: Uint8Array,
  options?: ZlibOptions,
): Uint8Array => {
  // .NET's DeflateStream is already "raw" deflate (no zlib wrapper).
  return inflateSync(buffer, options);
};

// ---------------------------------------------------------------------------
// Brotli
// ---------------------------------------------------------------------------

/**
 * Compress data using Brotli.
 *
 * @param buffer  The data to compress.
 * @param options Optional compression options.
 * @returns The compressed data.
 */
export const brotliCompressSync = (
  buffer: Uint8Array,
  options?: BrotliOptions,
): Uint8Array => {
  if (buffer === null || buffer === undefined) {
    throw new Error("buffer must not be null");
  }

  const quality = options !== undefined ? options.quality : undefined;
  const level =
    quality !== undefined && quality <= 2
      ? CompressionLevel.Fastest
      : CompressionLevel.SmallestSize;
  const output = new MemoryStream();
  const stream = new BrotliStream(output, level, true);
  const inputBytes = toByteArray(buffer);
  stream.Write(inputBytes, 0 as int, toInt(inputBytes.length));
  stream.Dispose();
  return fromByteArray(output.ToArray());
};

/**
 * Decompress Brotli data.
 *
 * @param buffer  The compressed data.
 * @param options Optional decompression options.
 * @returns The decompressed data.
 */
export const brotliDecompressSync = (
  buffer: Uint8Array,
  _options?: BrotliOptions,
): Uint8Array => {
  if (buffer === null || buffer === undefined) {
    throw new Error("buffer must not be null");
  }

  const input = new MemoryStream(toByteArray(buffer));
  const stream = new BrotliStream(input, CompressionMode.Decompress, true);
  const output = new MemoryStream();
  stream.CopyTo(output);
  stream.Dispose();
  return fromByteArray(output.ToArray());
};

// ---------------------------------------------------------------------------
// unzipSync — auto-detect format
// ---------------------------------------------------------------------------

/**
 * Decompress data. Auto-detects Gzip or Deflate format by magic bytes.
 *
 * @param buffer  The compressed data.
 * @param options Optional decompression options.
 * @returns The decompressed data.
 */
export const unzipSync = (
  buffer: Uint8Array,
  options?: ZlibOptions,
): Uint8Array => {
  if (buffer === null || buffer === undefined) {
    throw new Error("buffer must not be null");
  }

  if (buffer.length < 2) {
    throw new Error("Buffer too small to determine compression format");
  }

  // Detect format by magic bytes
  // Gzip: 0x1f 0x8b
  if (buffer[0] === 0x1f && buffer[1] === 0x8b) {
    return gunzipSync(buffer, options);
  }

  // Zlib format (deflate with header): 0x78 (multiple variations)
  if (buffer[0] === 0x78) {
    // Skip the 2-byte zlib header and use raw deflate
    const deflateData = buffer.slice(2);
    return inflateSync(deflateData, options);
  }

  // Try raw deflate
  return inflateSync(buffer, options);
};
