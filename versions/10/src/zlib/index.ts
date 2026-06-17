/**
 * Node.js zlib module.
 *
 */

import type {} from "../type-bootstrap.ts";

export type { BrotliOptions } from "./brotli-options.ts";
export type { ZlibOptions } from "./zlib-options.ts";

export {
  brotliCompressSync,
  brotliDecompressSync,
  crc32,
  crc32String,
  deflateRawSync,
  deflateSync,
  gunzipSync,
  gzipSync,
  inflateRawSync,
  inflateSync,
  unzipSync,
} from "./zlib.ts";
