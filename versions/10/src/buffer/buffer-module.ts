/**
 * Buffer module-level helpers: constants, atob/btoa, isAscii, isUtf8,
 * transcode, SlowBuffer, etc.
 *
 */

import { Buffer } from "./buffer.ts";
import type { int } from "@tsonic/core/types.js";
import { Convert } from "@tsonic/dotnet/System.js";
import {
  base64ToBytes,
  bytesToBase64,
  bytesToString,
  stringToBytes,
} from "./buffer-encoding.ts";

class NodeTypeError extends Error {
  constructor(message: string) {
    super(message);
  }
}

/**
 * Constants for the buffer module.
 */
export class BufferConstants {
  get MAX_LENGTH(): number {
    return kMaxLength;
  }

  get MAX_STRING_LENGTH(): number {
    return kStringMaxLength;
  }
}

/**
 * Maximum number of bytes for util.inspect truncation.
 */
export let INSPECT_MAX_BYTES: number = 50;

/**
 * Buffer constants object.
 */
export const constants: BufferConstants = new BufferConstants();

/**
 * Maximum buffer length.
 * In JS-land, ArrayBuffer is limited to ~2 GiB (2^31 - 1 on most engines).
 */
export const kMaxLength: int = 2147483647 as int;

/**
 * Maximum string length.
 */
export const kStringMaxLength: int = 1073741823 as int;

/**
 * SlowBuffer compatibility helper.
 */
export const SlowBuffer = (size: number): Buffer => {
  if (Number.isInteger(size) && size >= 0 && size <= kMaxLength) {
    return Buffer.alloc(Convert.ToInt32(size));
  }

  throw new RangeError("Buffer size must be a non-negative Int32-compatible value");
};

/**
 * Decodes a base64 string to a latin1 string.
 */
export const atob = (data: string): string => {
  if (data === null || data === undefined) {
    throw new NodeTypeError("The argument must be a string");
  }
  const decoded = base64ToBytes(data);
  return bytesToString(decoded, "latin1", 0, decoded.length);
};

/**
 * Encodes a latin1 string to base64.
 */
export const btoa = (data: string): string => {
  if (data === null || data === undefined) {
    throw new NodeTypeError("The argument must be a string");
  }
  return bytesToBase64(stringToBytes(data, "latin1"));
};

/**
 * Returns true if all bytes in the buffer are valid ASCII (0x00-0x7F).
 */
export const isAscii = (value: Buffer | Uint8Array): boolean => {
  if (value === null || value === undefined) {
    throw new NodeTypeError("The argument must be a Buffer or Uint8Array");
  }

  const data: Uint8Array = value instanceof Buffer ? value.buffer : value;
  for (let i = 0; i < data.length; i += 1) {
    if (data[i]! > 0x7f) return false;
  }
  return true;
};

/**
 * Returns true if the bytes are valid UTF-8.
 */
export const isUtf8 = (value: Buffer | Uint8Array): boolean => {
  if (value === null || value === undefined) {
    throw new NodeTypeError("The argument must be a Buffer or Uint8Array");
  }

  const data: Uint8Array = value instanceof Buffer ? value.buffer : value;

  // Validate UTF-8 byte sequences manually
  let i = 0;
  while (i < data.length) {
    const byte = data[i]!;

    if (byte <= 0x7f) {
      // Single-byte (ASCII)
      i += 1;
    } else if ((byte & 0xe0) === 0xc0) {
      // 2-byte sequence
      if (i + 1 >= data.length) return false;
      if ((data[i + 1]! & 0xc0) !== 0x80) return false;
      // Check for overlong encoding
      if (byte < 0xc2) return false;
      i += 2;
    } else if ((byte & 0xf0) === 0xe0) {
      // 3-byte sequence
      if (i + 2 >= data.length) return false;
      if ((data[i + 1]! & 0xc0) !== 0x80) return false;
      if ((data[i + 2]! & 0xc0) !== 0x80) return false;
      // Check for overlong encoding
      if (byte === 0xe0 && data[i + 1]! < 0xa0) return false;
      // Check for surrogates
      if (byte === 0xed && data[i + 1]! >= 0xa0) return false;
      i += 3;
    } else if ((byte & 0xf8) === 0xf0) {
      // 4-byte sequence
      if (i + 3 >= data.length) return false;
      if ((data[i + 1]! & 0xc0) !== 0x80) return false;
      if ((data[i + 2]! & 0xc0) !== 0x80) return false;
      if ((data[i + 3]! & 0xc0) !== 0x80) return false;
      // Check for overlong encoding
      if (byte === 0xf0 && data[i + 1]! < 0x90) return false;
      // Check for values > U+10FFFF
      if (byte > 0xf4) return false;
      if (byte === 0xf4 && data[i + 1]! > 0x8f) return false;
      i += 4;
    } else {
      // Invalid start byte
      return false;
    }
  }

  return true;
};

/**
 * Transcodes a buffer between encodings.
 */
export const transcode = (
  source: Buffer,
  fromEncoding: string,
  toEncoding: string,
): Buffer => {
  if (source === null || source === undefined) {
    throw new NodeTypeError("The source argument must be a Buffer");
  }

  // Decode from source encoding, re-encode to target encoding
  const text = source.toString(fromEncoding);
  return Buffer.from(text, toEncoding);
};

/**
 * resolveObjectURL is currently not implemented.
 */
export const resolveObjectURL = (_id: string): null => {
  return null;
};
