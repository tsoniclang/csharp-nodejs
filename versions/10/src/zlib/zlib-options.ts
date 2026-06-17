/**
 * Options for Gzip and Deflate compression operations.
 *
 */
export interface ZlibOptions {
  /**
   * Compression level. Range: -1 (default), 0 (no compression) to 9 (max compression).
   * Default: -1 (optimal balance)
   */
  level?: number;

  /**
   * Chunk size for internal buffer. Default: 16*1024 (16 KB).
   */
  chunkSize?: number;

  /**
   * Window size (8-15). Larger values use more memory but may improve compression.
   */
  windowBits?: number;

  /**
   * Memory level (1-9). Higher values use more memory for better compression.
   */
  memLevel?: number;

  /**
   * Compression strategy.
   */
  strategy?: number;

  /**
   * Maximum output length to prevent excessive memory usage.
   */
  maxOutputLength?: number;
}
