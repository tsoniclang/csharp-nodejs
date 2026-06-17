/**
 * StringDecoder — decodes Buffer objects into strings preserving
 * incomplete multi-byte UTF-8 and UTF-16 characters across write() calls.
 *
 *
 * NOTE: This implementation uses System.Text.Decoder which natively handles
 * incomplete multibyte sequences. The native implementation uses
 * TextDecoder with stream mode where available, with a manual fallback
 * for environments that don't support the stream option.
 */

import { bytesToString } from "../buffer/buffer-encoding.ts";
import { Convert } from "@tsonic/dotnet/System.js";
import type { int } from "@tsonic/core/types.js";

const fromCharCode = (code: number): string =>
  Convert.ToChar(code).toString();

export class StringDecoder {
  encoding: string;
  pendingBytes: Uint8Array = new Uint8Array(0);

  constructor(encoding?: string | null) {
    const normalized = (encoding ?? "utf8").toLowerCase();
    switch (normalized) {
      case "utf8":
      case "utf-8":
        this.encoding = "utf-8";
        break;
      case "utf16le":
      case "utf-16le":
      case "ucs2":
      case "ucs-2":
        this.encoding = "utf-16le";
        break;
      case "ascii":
        this.encoding = "ascii";
        break;
      case "latin1":
      case "binary":
        this.encoding = "latin1";
        break;
      default:
        this.encoding = "utf-8";
        break;
    }
  }

  /**
   * Returns a decoded string, ensuring that any incomplete multibyte
   * characters at the end are omitted and stored for the next call.
   */
  write(buffer: Uint8Array | null): string {
    if (buffer === null || buffer === undefined || buffer.length === 0) {
      return "";
    }

    // Combine pending bytes with new buffer
    const combined = this.combineWithPending(buffer);

    if (this.encoding === "ascii" || this.encoding === "latin1") {
      // Single-byte encodings: no incomplete sequences possible
      this.pendingBytes = new Uint8Array(0);
      return this.decodeSingleByte(combined);
    }

    if (this.encoding === "utf-16le") {
      return this.writeUtf16le(combined);
    }

    // UTF-8: find the boundary of complete characters
    return this.writeUtf8(combined);
  }

  /**
   * Returns any remaining input stored in the internal buffer as a string.
   * After end() is called, the StringDecoder can be reused for new input.
   */
  end(buffer?: Uint8Array | null): string {
    let result = "";

    if (buffer !== undefined && buffer !== null && buffer.length > 0) {
      const combined = this.combineWithPending(buffer);
      // Decode everything including incomplete sequences (flush)
      result = this.decodeAll(combined);
    } else if (this.pendingBytes.length > 0) {
      // Flush any remaining incomplete bytes
      result = this.decodeAll(this.pendingBytes);
    }

    // Reset for potential reuse
    this.pendingBytes = new Uint8Array(0);
    return result;
  }

  combineWithPending(buffer: Uint8Array): Uint8Array {
    if (this.pendingBytes.length === 0) {
      return buffer;
    }
    const combined = new Uint8Array(this.pendingBytes.length + buffer.length);
    combined.set(this.pendingBytes);
    combined.set(buffer, this.pendingBytes.length);
    this.pendingBytes = new Uint8Array(0);
    return combined;
  }

  decodeSingleByte(buffer: Uint8Array): string {
    let result = "";
    for (let i = 0; i < buffer.length; i += 1) {
      result += fromCharCode(buffer[i]!);
    }
    return result;
  }

  writeUtf8(buffer: Uint8Array): string {
    // Find the last complete character boundary
    const boundary = this.findUtf8Boundary(buffer);
    if (boundary === buffer.length) {
      this.pendingBytes = new Uint8Array(0);
      return this.decodeAll(buffer);
    }

    // Store incomplete bytes
    this.pendingBytes = this.copyRange(buffer, boundary, buffer.length);
    if (boundary === 0) {
      return "";
    }
    return this.decodeAll(this.copyRange(buffer, 0, boundary));
  }

  writeUtf16le(buffer: Uint8Array): string {
    // UTF-16LE: each code unit is 2 bytes
    // If odd number of bytes, last byte is incomplete
    const completeBytes = (buffer.length - (buffer.length % 2)) as int;

    if (completeBytes === 0) {
      this.pendingBytes = this.copyRange(buffer, 0, buffer.length);
      return "";
    }

    if (completeBytes < buffer.length) {
      this.pendingBytes = this.copyRange(buffer, completeBytes, buffer.length);
    } else {
      this.pendingBytes = new Uint8Array(0);
    }

    // Check for incomplete surrogate pairs at the end
    if (completeBytes >= 2) {
      const lastCodeUnit =
        buffer[completeBytes - 2]! | (buffer[completeBytes - 1]! << 8);
      // High surrogate without following low surrogate
      if (lastCodeUnit >= 0xd800 && lastCodeUnit <= 0xdbff) {
        // Move the high surrogate to pending
        const pending = new Uint8Array(
          (2 + (buffer.length - completeBytes)) as int
        );
        pending.set(this.copyRange(buffer, completeBytes - 2, buffer.length));
        this.pendingBytes = pending;
        if (completeBytes - 2 === 0) {
          return "";
        }
        return this.decodeAll(this.copyRange(buffer, 0, completeBytes - 2));
      }
    }

    return this.decodeAll(this.copyRange(buffer, 0, completeBytes));
  }

  findUtf8Boundary(buffer: Uint8Array): int {
    let i = buffer.length as int;
    // Walk backwards to find the start of the last (possibly incomplete) character
    while (i > 0) {
      i = (i - 1) as int;
      const byte = buffer[i]!;

      // Single-byte (0xxxxxxx) — this is a complete character
      if ((byte & 0x80) === 0) {
        return (i + 1) as int;
      }

      // Continuation byte (10xxxxxx) — keep walking back
      if ((byte & 0xc0) === 0x80) {
        continue;
      }

      // Start byte — determine expected length
      let expectedLen = 0;
      if ((byte & 0xe0) === 0xc0) expectedLen = 2;
      else if ((byte & 0xf0) === 0xe0) expectedLen = 3;
      else if ((byte & 0xf8) === 0xf0) expectedLen = 4;

      const available = buffer.length - i;
      if (available >= expectedLen) {
        // Complete character
        return i + expectedLen;
      }

      // Incomplete — boundary is at this start byte
      return i;
    }

    return 0;
  }

  decodeAll(buffer: Uint8Array): string {
    if (buffer.length === 0) {
      return "";
    }

    if (this.encoding === "ascii" || this.encoding === "latin1") {
      return this.decodeSingleByte(buffer);
    }

    return bytesToString(buffer, this.encoding, 0, buffer.length);
  }

  copyRange(buffer: Uint8Array, start: int, end: int): Uint8Array {
    const result = new Uint8Array((end - start) as int);
    let targetIndex = 0 as int;
    for (let index = 0 as int; index < buffer.length; index += 1) {
      if (index < start || index >= end) {
        continue;
      }
      result[targetIndex] = buffer[index]!;
      targetIndex = (targetIndex + 1) as int;
    }
    return result;
  }
}
