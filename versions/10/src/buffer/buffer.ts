/**
 * Buffer — fixed-length sequence of bytes wrapping Uint8Array with
 * Node.js-compatible APIs.
 *
 *
 * NOTE: This implementation wraps byte[]. The native surface wraps
 * Uint8Array, which is the natural JS equivalent.
 */

import { overloads as O } from "@tsonic/core/lang.js";
import type { byte, int, long, ulong } from "@tsonic/core/types.js";
import { Convert } from "@tsonic/dotnet/System.js";

import {
  byteLengthOfString,
  bytesToString,
  normalizeEncoding,
  stringToBytes,
  hexToBytes as encHexToBytes,
  base64ToBytes,
  base64UrlToBase64,
} from "./buffer-encoding.ts";

const UINT32_FACTOR = 4294967296;

export class BufferJson {
  type: string;
  data: number[];

  constructor(type: string, data: number[]) {
    this.type = type;
    this.data = data;
  }
}

class BufferInternals {
  static toInt(value: number): int {
    if (Number.isInteger(value) && value >= -2147483648 && value <= 2147483647) {
      return value as int;
    }

    throw new RangeError("Expected Int32-compatible numeric value");
  }

  static minInt(left: int, right: int): int {
    return left < right ? left : right;
  }

  static maxInt(left: int, right: int): int {
    return left > right ? left : right;
  }

  static clampInt(value: int, min: int, max: int): int {
    return BufferInternals.maxInt(min, BufferInternals.minInt(value, max));
  }

  static copyRange(
    source: Uint8Array,
    start: int,
    end: int,
  ): Uint8Array {
    const safeStart = start < 0 ? (0 as int) : start;
    const safeEnd = end < safeStart ? safeStart : end;
    const result = new Uint8Array(safeEnd - safeStart);
    let targetIndex = 0;
    for (let index = 0; index < source.length; index += 1) {
      if (index < safeStart || index >= safeEnd) {
        continue;
      }
      result[targetIndex] = source[index]!;
      targetIndex += 1;
    }
    return result;
  }

  static copyInto(
    source: Uint8Array,
    sourceStart: int,
    sourceEnd: int,
    target: Uint8Array,
    targetStart: int,
  ): void {
    const copied = BufferInternals.copyRange(source, sourceStart, sourceEnd);
    let writeIndex: int = targetStart;
    for (let index = 0; index < copied.length; index += 1) {
      target[writeIndex] = copied[index]!;
      writeIndex += 1;
    }
  }

  static toUint8Array(bytes: byte[]): Uint8Array {
    const result = new Uint8Array(bytes.length);
    for (let index = 0; index < bytes.length; index += 1) {
      result[index] = bytes[index]!;
    }
    return result;
  }

  static pow256(exponent: number): number {
    return Math.pow(256, exponent);
  }

  static pow2(exponent: number): number {
    return Math.pow(2, exponent);
  }

  static stripWhitespace(value: string): string {
    let result = "";
    for (let index = 0; index < value.length; index += 1) {
      const char = value.charAt(index);
      if (
        char !== " " &&
        char !== "\n" &&
        char !== "\r" &&
        char !== "\t"
      ) {
        result += char;
      }
    }
    return result;
  }

  static readUInt32(
    bytes: Uint8Array,
    offset: int,
    littleEndian: boolean,
  ): number {
    if (littleEndian) {
      return (
        (bytes[offset]! |
          (bytes[offset + 1]! << 8) |
          (bytes[offset + 2]! << 16) |
          (bytes[offset + 3]! << 24)) >>>
        0
      );
    }

    return (
      ((bytes[offset]! << 24) |
        (bytes[offset + 1]! << 16) |
        (bytes[offset + 2]! << 8) |
        bytes[offset + 3]!) >>>
      0
    );
  }

  static readInt32(
    bytes: Uint8Array,
    offset: int,
    littleEndian: boolean,
  ): number {
    if (littleEndian) {
      return (
        bytes[offset]! |
        (bytes[offset + 1]! << 8) |
        (bytes[offset + 2]! << 16) |
        (bytes[offset + 3]! << 24)
      );
    }

    return (
      (bytes[offset]! << 24) |
      (bytes[offset + 1]! << 16) |
      (bytes[offset + 2]! << 8) |
      bytes[offset + 3]!
    );
  }

  static writeUInt32(
    bytes: Uint8Array,
    offset: int,
    value: number,
    littleEndian: boolean,
  ): void {
    if (littleEndian) {
      bytes[offset] = Convert.ToByte(value & 0xff);
      bytes[offset + 1] = Convert.ToByte((value >>> 8) & 0xff);
      bytes[offset + 2] = Convert.ToByte((value >>> 16) & 0xff);
      bytes[offset + 3] = Convert.ToByte((value >>> 24) & 0xff);
      return;
    }

    bytes[offset] = Convert.ToByte((value >>> 24) & 0xff);
    bytes[offset + 1] = Convert.ToByte((value >>> 16) & 0xff);
    bytes[offset + 2] = Convert.ToByte((value >>> 8) & 0xff);
    bytes[offset + 3] = Convert.ToByte(value & 0xff);
  }

  static writeInt32(
    bytes: Uint8Array,
    offset: int,
    value: number,
    littleEndian: boolean,
  ): void {
    if (littleEndian) {
      bytes[offset] = Convert.ToByte(value & 0xff);
      bytes[offset + 1] = Convert.ToByte((value >> 8) & 0xff);
      bytes[offset + 2] = Convert.ToByte((value >> 16) & 0xff);
      bytes[offset + 3] = Convert.ToByte((value >> 24) & 0xff);
      return;
    }

    bytes[offset] = Convert.ToByte((value >> 24) & 0xff);
    bytes[offset + 1] = Convert.ToByte((value >> 16) & 0xff);
    bytes[offset + 2] = Convert.ToByte((value >> 8) & 0xff);
    bytes[offset + 3] = Convert.ToByte(value & 0xff);
  }

  static readIeee754(
    bytes: Uint8Array,
    littleEndian: boolean,
    mantissaBits: int,
    byteLength: int,
  ): number {
    const exponentBits: int = (byteLength * 8 - mantissaBits - 1) as int;
    const maxExponent = (1 << exponentBits) - 1;
    const exponentBias = maxExponent >> 1;
    let remainingBits = -7;
    let index = littleEndian ? byteLength - 1 : (0 as int);
    const delta = littleEndian ? -1 : 1;
    let initial = bytes[index]!;

    index = (index + delta) as int;

    let exponent = initial & ((1 << (-remainingBits)) - 1);
    const sign = initial >> (-remainingBits);

    remainingBits += exponentBits;
    for (; remainingBits > 0; remainingBits -= 8) {
      exponent = exponent * 256 + bytes[index]!;
      index = (index + delta) as int;
    }

    let mantissa: number = exponent & ((1 << (-remainingBits)) - 1);
    exponent >>= -remainingBits;
    remainingBits += mantissaBits;

    for (; remainingBits > 0; remainingBits -= 8) {
      mantissa = mantissa * 256 + bytes[index]!;
      index = (index + delta) as int;
    }

    if (exponent === 0) {
      exponent = 1 - exponentBias;
    } else if (exponent === maxExponent) {
      const zero = mantissa - mantissa;
      return mantissa !== 0 ? zero / zero : sign !== 0 ? -1 / zero : 1 / zero;
    } else {
      mantissa += Math.pow(2, mantissaBits);
      exponent -= exponentBias;
    }

    return (sign !== 0 ? -1 : 1) * mantissa * Math.pow(2, exponent - mantissaBits);
  }

  static writeIeee754(
    value: number,
    littleEndian: boolean,
    mantissaBits: int,
    byteLength: int,
  ): Uint8Array {
    const bytes = new Uint8Array(byteLength);
    const exponentBits: int = (byteLength * 8 - mantissaBits - 1) as int;
    const maxExponent = (1 << exponentBits) - 1;
    const exponentBias = maxExponent >> 1;
    const roundingTerm =
      mantissaBits === 23
        ? Math.pow(2, -24) - Math.pow(2, -77)
        : 0;
    let index = littleEndian ? (0 as int) : (byteLength - 1);
    const delta = littleEndian ? 1 : -1;
    const sign: number = value < 0 || (value === 0 && 1 / value < 0) ? 1 : 0;
    let exponent: number = 0;
    let mantissa: number = 0;
    let correction: number = 0;
    let absoluteValue = Math.abs(value);
    const isNotANumber = absoluteValue !== absoluteValue;
    const isInfinite = !isNotANumber && !isFinite(absoluteValue);

    if (isNotANumber || isInfinite) {
      mantissa = isNotANumber ? 1 : 0;
      exponent = maxExponent;
    } else {
      exponent = Math.floor(Math.log(absoluteValue) / Math.log(2));
      correction = Math.pow(2, -exponent);

      if (absoluteValue * correction < 1) {
        exponent -= 1;
        correction *= 2;
      }

      if (exponent + exponentBias >= 1) {
        absoluteValue += roundingTerm / correction;
      } else {
        absoluteValue += roundingTerm * Math.pow(2, 1 - exponentBias);
      }

      if (absoluteValue * correction >= 2) {
        exponent += 1;
        correction /= 2;
      }

      if (exponent + exponentBias >= maxExponent) {
        mantissa = 0;
        exponent = maxExponent;
      } else if (exponent + exponentBias >= 1) {
        mantissa = (absoluteValue * correction - 1) * Math.pow(2, mantissaBits);
        exponent += exponentBias;
      } else {
        mantissa =
          absoluteValue *
          Math.pow(2, exponentBias - 1) *
          Math.pow(2, mantissaBits);
        exponent = 0;
      }
    }

    let remainingMantissaBits: int = mantissaBits;
    let mantissaRemainder: number = mantissa;
    for (; remainingMantissaBits >= 8; remainingMantissaBits -= 8) {
      const mantissaByte = Convert.ToByte(Math.trunc(mantissaRemainder) & 0xff);
      bytes[index] = mantissaByte;
      index = (index + delta) as int;
      mantissaRemainder = Math.floor(mantissaRemainder / 256);
    }

    let combined: number =
      (Math.trunc(exponent) << remainingMantissaBits) |
      Math.trunc(mantissaRemainder);
    let remainingExponentBits: int = (exponentBits + remainingMantissaBits) as int;
    for (; remainingExponentBits > 0; remainingExponentBits -= 8) {
      const combinedByte = Convert.ToByte(Math.trunc(combined) & 0xff);
      bytes[index] = combinedByte;
      index = (index + delta) as int;
      combined = Math.floor(combined / 256);
    }

    const signIndex = (index - delta) as int;
    bytes[signIndex] = Convert.ToByte(bytes[signIndex]! | (sign * 128));

    return bytes;
  }

  static readUInt64(bytes: Uint8Array, littleEndian: boolean): ulong {
    const low = littleEndian
      ? BufferInternals.readUInt32(bytes, 0, true)
      : BufferInternals.readUInt32(bytes, 4, false);
    const high = littleEndian
      ? BufferInternals.readUInt32(bytes, 4, true)
      : BufferInternals.readUInt32(bytes, 0, false);
    const value: ulong = Convert.ToUInt64((high * UINT32_FACTOR) + low);
    return value;
  }

  static readInt64(bytes: Uint8Array, littleEndian: boolean): long {
    const low = littleEndian
      ? BufferInternals.readUInt32(bytes, 0, true)
      : BufferInternals.readUInt32(bytes, 4, false);
    const high = littleEndian
      ? BufferInternals.readInt32(bytes, 4, true)
      : BufferInternals.readInt32(bytes, 0, false);
    const value: long = Convert.ToInt64((high * UINT32_FACTOR) + low);
    return value;
  }

  static writeUInt64(value: ulong, littleEndian: boolean): Uint8Array {
    const bytes = new Uint8Array(8);
    const truncated = Math.trunc(value);
    const low = truncated >>> 0;
    const high = Math.floor(truncated / UINT32_FACTOR) >>> 0;

    if (littleEndian) {
      BufferInternals.writeUInt32(bytes, 0, low, true);
      BufferInternals.writeUInt32(bytes, 4, high, true);
      return bytes;
    }

    BufferInternals.writeUInt32(bytes, 0, high, false);
    BufferInternals.writeUInt32(bytes, 4, low, false);
    return bytes;
  }

  static writeInt64(value: long, littleEndian: boolean): Uint8Array {
    const bytes = new Uint8Array(8);
    const truncated = Math.trunc(value);
    const low = truncated >>> 0;
    const high = Math.floor(truncated / UINT32_FACTOR);

    if (littleEndian) {
      BufferInternals.writeUInt32(bytes, 0, low, true);
      BufferInternals.writeInt32(bytes, 4, high, true);
      return bytes;
    }

    BufferInternals.writeInt32(bytes, 0, high, false);
    BufferInternals.writeUInt32(bytes, 4, low, false);
    return bytes;
  }
}

export class Buffer {
  [index: number]: byte;

  /** The underlying typed array. */
  _data: Uint8Array;

  /** Gets the length of the buffer in bytes. */
  get length(): int {
    return this._data.length;
  }

  /**
   * The size (in bytes) of pre-allocated internal Buffer instances used for pooling.
   * This value may be modified.
   */
  static poolSize: int = 8192;

  constructor(data: Uint8Array) {
    this._data = data;
  }

  // ---- indexer-style access ----

  /**
   * Returns the byte at `index`.  For performance the caller should
   * bounds-check; out-of-range returns undefined like a typed-array.
   */
  at(index: int): number | undefined {
    return this._data[index];
  }

  *entries(): Generator<[int, byte], undefined, undefined> {
    for (let index = 0 as int; index < this.length; index = (index + 1) as int) {
      yield [index, this._data[index]!];
    }
  }

  *keys(): Generator<int, undefined, undefined> {
    for (let index = 0 as int; index < this.length; index = (index + 1) as int) {
      yield index;
    }
  }

  *values(): Generator<byte, undefined, undefined> {
    for (let index = 0 as int; index < this.length; index = (index + 1) as int) {
      yield this._data[index]!;
    }
  }

  [Symbol.iterator](): Generator<byte, undefined, undefined> {
    return this.values();
  }

  /**
   * Sets the byte at `index` (value is truncated to 0-255).
   */
  set(index: int, value: number): void {
    this._data[index] = Convert.ToByte(value & 0xff);
  }

  /**
   * Proxy-based indexer is not feasible in a Tsonic source package,
   * so we expose the underlying Uint8Array for bracket access.
   */
  get buffer(): Uint8Array {
    return this._data;
  }

  // ---- static factory: alloc / allocUnsafe / allocUnsafeSlow ----

  /**
   * Allocates a new Buffer of `size` bytes.
   * If fill is undefined, the Buffer will be zero-filled.
   */
  static alloc(
    size: int,
    fill?: number | string | Buffer,
    encoding?: string,
  ): Buffer {
    if (size < 0) {
      throw new RangeError("Buffer size must be non-negative");
    }
    const buf = new Buffer(new Uint8Array(size));

    if (fill !== undefined) {
      if (typeof fill === "number") {
        buf.fill_number(fill, 0, size);
      } else if (fill instanceof Buffer) {
        buf.fill_buffer(fill, 0, size);
      } else {
        buf.fill_string(fill, 0, size, encoding ?? "utf8");
      }
    }

    return buf;
  }

  /**
   * Allocates a new Buffer of `size` bytes without zero-filling.
   * In JS-land this still zero-fills because Uint8Array is always zeroed.
   */
  static allocUnsafe(size: int): Buffer {
    if (size < 0) {
      throw new RangeError("Buffer size must be non-negative");
    }
    return new Buffer(new Uint8Array(size));
  }

  /**
   * Allocates a new non-pooled Buffer of `size` bytes.
   */
  static allocUnsafeSlow(size: int): Buffer {
    if (size < 0) {
      throw new RangeError("Buffer size must be non-negative");
    }
    return new Buffer(new Uint8Array(size));
  }

  // ---- static factory: from / of ----

  /**
   * Creates a new Buffer from a string.
   */
  static fromString(str: string, encoding?: string): Buffer {
    const bytes = stringToBytes(str, encoding ?? "utf8");
    return new Buffer(bytes);
  }

  /**
   * Creates a new Buffer from an array of numbers (values truncated to 0-255).
   */
  static fromArray(array: number[]): Buffer {
    const bytes = new Uint8Array(array.length);
    for (let i = 0; i < array.length; i += 1) {
      bytes[i] = Convert.ToByte((array[i]!) & 0xff);
    }
    return new Buffer(bytes);
  }

  /**
   * Creates a new Buffer by copying an existing Buffer.
   */
  static fromBuffer(buffer: Buffer): Buffer {
    const copy = new Uint8Array(buffer._data.length);
    copy.set(buffer._data);
    return new Buffer(copy);
  }

  /**
   * Creates a new Buffer from a Uint8Array (makes a copy).
   */
  static fromUint8Array(arr: Uint8Array): Buffer {
    const copy = new Uint8Array(arr.length);
    copy.set(arr);
    return new Buffer(copy);
  }

  /**
   * Overloaded `from` matching Node.js signatures.
   */
  static from(value: string, encoding?: string): Buffer;
  static from(value: number[]): Buffer;
  static from(value: Buffer): Buffer;
  static from(value: Uint8Array): Buffer;
  static from(_value: any, _encoding?: any): Buffer {
    throw new Error("Unreachable overload stub");
  }

  static fromBytes(bytes: byte[]): Buffer {
    return new Buffer(BufferInternals.toUint8Array(bytes));
  }

  /**
   * Creates a Buffer from variadic number arguments.
   */
  static of(...items: number[]): Buffer {
    return Buffer.fromArray(items);
  }

  // ---- static utilities ----

  /**
   * Returns true if obj is a Buffer.
   */
  static isBuffer(obj: object | null | undefined): obj is Buffer {
    return obj instanceof Buffer;
  }

  /**
   * Returns true if encoding is a supported character encoding name.
   */
  static isEncoding(encoding: string): boolean {
    const norm = normalizeEncoding(encoding);
    switch (norm) {
      case "utf8":
      case "ascii":
      case "latin1":
      case "binary":
      case "base64":
      case "base64url":
      case "hex":
      case "utf16le":
      case "ucs2":
        return true;
      default:
        return false;
    }
  }

  /**
   * Returns the byte length of a string when encoded.
   */
  static byteLength(str: string, encoding: string = "utf8"): number {
    return byteLengthOfString(str, encoding);
  }

  /**
   * Compares two Buffers, returning -1, 0, or 1.
   */
  static compare(buf1: Buffer, buf2: Buffer): number {
    return buf1.compare(buf2);
  }

  /**
   * Concatenates an array of Buffers.
   */
  static concat(list: Buffer[], totalLength?: number): Buffer {
    if (list.length === 0) {
      return Buffer.alloc(0);
    }

    let len = 0;
    if (totalLength !== undefined) {
      len = BufferInternals.toInt(totalLength);
    } else {
      for (let i = 0; i < list.length; i += 1) {
        len += list[i]!.length;
      }
    }

    const result = Buffer.alloc(len);
    let offset = 0;

    for (let i = 0; i < list.length; i += 1) {
      if (offset >= len) break;
      const buf = list[i]!;
      const copyLen = BufferInternals.minInt(buf.length, (len - offset) as int);
      BufferInternals.copyInto(buf._data, 0, copyLen, result._data, offset);
      offset += copyLen;
    }

    return result;
  }

  // ---- instance: compare / equals / indexOf / lastIndexOf / includes ----

  /**
   * Returns true if this buffer and `otherBuffer` have identical bytes.
   */
  equals(otherBuffer: Buffer): boolean {
    if (this.length !== otherBuffer.length) return false;
    for (let i = 0; i < this.length; i += 1) {
      if (this._data[i] !== otherBuffer._data[i]) return false;
    }
    return true;
  }

  /**
   * Compares this buffer with `target`, returning -1, 0, or 1.
   */
  compare(
    target: Buffer,
    targetStart?: int,
    targetEnd?: int,
    sourceStart?: int,
    sourceEnd?: int,
  ): number {
    let tStart = targetStart ?? 0;
    let tEnd = targetEnd ?? target.length;
    let sStart = sourceStart ?? 0;
    let sEnd = sourceEnd ?? this.length;

    tStart = BufferInternals.clampInt(tStart, 0 as int, target.length);
    tEnd = BufferInternals.maxInt(tStart, BufferInternals.minInt(tEnd, target.length));
    sStart = BufferInternals.clampInt(sStart, 0 as int, this.length);
    sEnd = BufferInternals.maxInt(sStart, BufferInternals.minInt(sEnd, this.length));

    const sourceLength = sEnd - sStart;
    const targetLength = tEnd - tStart;
    const minLength = BufferInternals.minInt(
      sourceLength as int,
      targetLength as int,
    );

    for (let i = 0; i < minLength; i += 1) {
      const a = this._data[sStart + i]!;
      const b = target._data[tStart + i]!;
      if (a < b) return -1;
      if (a > b) return 1;
    }

    if (sourceLength < targetLength) return -1;
    if (sourceLength > targetLength) return 1;
    return 0;
  }

  /**
   * Returns the index of the first occurrence of `value`, or -1.
   */
  indexOf(value: string, byteOffset?: int, encoding?: string): number;
  indexOf(value: number, byteOffset?: int): number;
  indexOf(value: Buffer, byteOffset?: int): number;
  indexOf(_value: any, _byteOffset?: any, _encoding?: any): number {
    throw new Error("Unreachable overload stub");
  }

  indexOf_string(
    value: string,
    byteOffset?: int,
    encoding?: string,
  ): number {
    return this.indexOfBytes(stringToBytes(value, encoding ?? "utf8"), byteOffset ?? (0 as int));
  }

  indexOf_number(value: number, byteOffset?: int): number {
    const byteCell = new Uint8Array(1);
    byteCell[0] = Convert.ToByte(value & 0xff);
    return this.indexOfBytes(byteCell, byteOffset ?? (0 as int));
  }

  indexOf_buffer(value: Buffer, byteOffset?: int): number {
    return this.indexOfBytes(value._data, byteOffset ?? (0 as int));
  }

  indexOfBytes(
    searchBytes: Uint8Array,
    byteOffset: int,
  ): number {
    let offset = byteOffset;
    if (offset < 0) offset = BufferInternals.maxInt(0 as int, (this.length + offset) as int);
    if (offset >= this.length) return -1;

    if (searchBytes.length === 0) return offset;

    for (let i = offset; i <= this.length - searchBytes.length; i += 1) {
      let found = true;
      for (let j = 0; j < searchBytes.length; j += 1) {
        if (this._data[i + j] !== searchBytes[j]) {
          found = false;
          break;
        }
      }
      if (found) return i;
    }

    return -1;
  }

  /**
   * Returns the index of the last occurrence of `value`, or -1.
   */
  lastIndexOf(value: string, byteOffset?: int, encoding?: string): number;
  lastIndexOf(value: number, byteOffset?: int): number;
  lastIndexOf(value: Buffer, byteOffset?: int): number;
  lastIndexOf(_value: any, _byteOffset?: any, _encoding?: any): number {
    throw new Error("Unreachable overload stub");
  }

  lastIndexOf_string(
    value: string,
    byteOffset?: int,
    encoding?: string,
  ): number {
    return this.lastIndexOfBytes(stringToBytes(value, encoding ?? "utf8"), byteOffset);
  }

  lastIndexOf_number(value: number, byteOffset?: int): number {
    const byteCell = new Uint8Array(1);
    byteCell[0] = Convert.ToByte(value & 0xff);
    return this.lastIndexOfBytes(byteCell, byteOffset);
  }

  lastIndexOf_buffer(value: Buffer, byteOffset?: int): number {
    return this.lastIndexOfBytes(value._data, byteOffset);
  }

  lastIndexOfBytes(
    searchBytes: Uint8Array,
    byteOffset: int | undefined,
  ): number {
    let offset = byteOffset ?? this.length - 1;
    if (offset < 0) offset = BufferInternals.maxInt(0 as int, (this.length + offset) as int);
    if (offset >= this.length) offset = this.length - 1;

    if (searchBytes.length === 0) return offset;

    for (let i = offset; i >= 0; i -= 1) {
      if (i + searchBytes.length > this.length) {
        continue;
      }
      let found = true;
      for (let j = 0; j < searchBytes.length; j += 1) {
        if (this._data[i + j] !== searchBytes[j]) {
          found = false;
          break;
        }
      }
      if (found) return i;
    }

    return -1;
  }

  /**
   * Equivalent to indexOf(value) !== -1.
   */
  includes(value: string, byteOffset?: int, encoding?: string): boolean;
  includes(value: number, byteOffset?: int): boolean;
  includes(value: Buffer, byteOffset?: int): boolean;
  includes(_value: any, _byteOffset?: any, _encoding?: any): boolean {
    throw new Error("Unreachable overload stub");
  }

  includes_string(
    value: string,
    byteOffset?: int,
    encoding?: string,
  ): boolean {
    return this.indexOf_string(value, byteOffset, encoding) !== -1;
  }

  includes_number(value: number, byteOffset?: int): boolean {
    return this.indexOf_number(value, byteOffset) !== -1;
  }

  includes_buffer(value: Buffer, byteOffset?: int): boolean {
    return this.indexOf_buffer(value, byteOffset) !== -1;
  }

  // ---- instance: fill ----

  /**
   * Fills the buffer with the specified value.
   */
  fill(value: string, offset?: int, end?: int, encoding?: string): Buffer;
  fill(value: number, offset?: int, end?: int): Buffer;
  fill(value: Buffer, offset?: int, end?: int): Buffer;
  fill(_value: any, _offset?: any, _end?: any, _encoding?: any): Buffer {
    throw new Error("Unreachable overload stub");
  }

  fill_string(
    value: string,
    offset?: int,
    end?: int,
    encoding?: string,
  ): Buffer {
    let startIndex = offset ?? (0 as int);
    let endIndex = end ?? this.length;
    startIndex = BufferInternals.clampInt(startIndex, 0 as int, this.length);
    endIndex = BufferInternals.maxInt(startIndex, BufferInternals.minInt(endIndex, this.length));

    if (startIndex >= endIndex) return this;

    if (value.length === 0) return this;
    const bytes = stringToBytes(value, encoding ?? "utf8");
    if (bytes.length === 0) return this;
    for (let i = startIndex; i < endIndex; i += 1) {
      this._data[i] = bytes[(i - startIndex) % bytes.length]!;
    }

    return this;
  }

  fill_number(
    value: number,
    offset?: int,
    end?: int,
  ): Buffer {
    let startIndex = offset ?? (0 as int);
    let endIndex = end ?? this.length;
    startIndex = BufferInternals.clampInt(startIndex, 0 as int, this.length);
    endIndex = BufferInternals.maxInt(startIndex, BufferInternals.minInt(endIndex, this.length));

    const byteCell = new Uint8Array(1);
    byteCell[0] = Convert.ToByte(value & 0xff);
    const byteValue = byteCell[0]!;
    for (let i = startIndex; i < endIndex; i += 1) {
      this._data[i] = byteValue;
    }

    return this;
  }

  fill_buffer(
    value: Buffer,
    offset?: int,
    end?: int,
  ): Buffer {
    let startIndex = offset ?? (0 as int);
    let endIndex = end ?? this.length;
    startIndex = BufferInternals.clampInt(startIndex, 0 as int, this.length);
    endIndex = BufferInternals.maxInt(startIndex, BufferInternals.minInt(endIndex, this.length));

    const sourceLength = value._data.length;
    if (sourceLength === 0) return this;
    for (let i = startIndex; i < endIndex; i += 1) {
      this._data[i] = value._data[(i - startIndex) % sourceLength]!;
    }

    return this;
  }

  // ---- instance: slice / subarray / copy ----

  /**
   * Returns a new Buffer referencing the same memory, offset and cropped.
   * In this native implementation a copy is returned for safety.
   */
  slice(start?: int, end?: int): Buffer {
    let startIndex = start ?? 0;
    let endIndex = end ?? this.length;

    if (startIndex < 0) startIndex = BufferInternals.maxInt(0 as int, (this.length + startIndex) as int);
    if (endIndex < 0) endIndex = BufferInternals.maxInt(0 as int, (this.length + endIndex) as int);

    startIndex = BufferInternals.clampInt(startIndex, 0 as int, this.length);
    endIndex = BufferInternals.maxInt(startIndex, BufferInternals.minInt(endIndex, this.length));

    const sliceLen = endIndex - startIndex;
    const newData = new Uint8Array(sliceLen);
    BufferInternals.copyInto(this._data, startIndex, endIndex, newData, 0);
    return new Buffer(newData);
  }

  /**
   * Returns a view of the buffer (same semantics as slice in this implementation).
   */
  subarray(start?: int, end?: int): Buffer {
    return this.slice(start, end);
  }

  /**
   * Copies data from this buffer to `target`. Returns number of bytes copied.
   */
  copy(
    target: Buffer,
    targetStart: int = 0,
    sourceStart?: int,
    sourceEnd?: int,
  ): number {
    let srcStart = sourceStart ?? 0;
    let srcEnd = sourceEnd ?? this.length;

    srcStart = BufferInternals.clampInt(srcStart, 0 as int, this.length);
    srcEnd = BufferInternals.maxInt(srcStart, BufferInternals.minInt(srcEnd, this.length));

    if (targetStart < 0 || targetStart >= target.length) return 0;

    const bytesToCopy = BufferInternals.minInt(
      (srcEnd - srcStart) as int,
      (target.length - targetStart) as int,
    );
    if (bytesToCopy <= 0) return 0;

    BufferInternals.copyInto(
      this._data,
      srcStart,
      srcStart + bytesToCopy,
      target._data,
      targetStart
    );
    return bytesToCopy;
  }

  // ---- instance: toString / toJSON / write ----

  /**
   * Decodes the buffer to a string.
   */
  toString(encoding: string = "utf8", start: int = 0, end?: int): string {
    let endIndex = end ?? this.length;
    if (start < 0) start = 0;
    if (endIndex > this.length) endIndex = this.length;
    if (start >= endIndex) return "";

    return bytesToString(this._data, encoding, start, endIndex);
  }

  /**
   * Returns a JSON representation of the buffer.
   */
  toJSON(): BufferJson {
    const data: number[] = [];
    for (let i = 0; i < this._data.length; i += 1) {
      data.push(this._data[i]!);
    }
    return new BufferJson("Buffer", data);
  }

  /**
   * Writes a string to the buffer at `offset`. Returns number of bytes written.
   */
  write(
    str: string,
    offset: int = 0,
    length?: int,
    encoding: string = "utf8",
  ): number {
    if (offset < 0 || offset >= this.length) return 0;

    const maxLength = length ?? (this.length - offset);
    if (maxLength <= 0) return 0;

    const norm = normalizeEncoding(encoding);

    if (norm === "hex") {
      return this.writeHex(str, offset, maxLength);
    }

    if (norm === "base64" || norm === "base64url") {
      return this.writeBase64(str, offset, maxLength, norm === "base64url");
    }

    const bytes = stringToBytes(str, encoding);
    const bytesToWrite = BufferInternals.minInt(bytes.length, maxLength);
    BufferInternals.copyInto(bytes, 0, bytesToWrite, this._data, offset);
    return bytesToWrite;
  }

  // ---- instance: swap16 / swap32 / swap64 / reverse ----

  /**
   * Reverses the buffer in-place.
   */
  reverse(): Buffer {
    this._data.reverse();
    return this;
  }

  /**
   * Swaps byte order for 16-bit elements in-place.
   */
  swap16(): Buffer {
    if (this.length % 2 !== 0) {
      throw new RangeError("Buffer size must be a multiple of 16-bits");
    }
    for (let i = 0; i < this.length; i += 2) {
      const tmp = this._data[i]!;
      this._data[i] = this._data[i + 1]!;
      this._data[i + 1] = tmp;
    }
    return this;
  }

  /**
   * Swaps byte order for 32-bit elements in-place.
   */
  swap32(): Buffer {
    if (this.length % 4 !== 0) {
      throw new RangeError("Buffer size must be a multiple of 32-bits");
    }
    for (let i = 0; i < this.length; i += 4) {
      const t0 = this._data[i]!;
      const t1 = this._data[i + 1]!;
      this._data[i] = this._data[i + 3]!;
      this._data[i + 1] = this._data[i + 2]!;
      this._data[i + 2] = t1;
      this._data[i + 3] = t0;
    }
    return this;
  }

  /**
   * Swaps byte order for 64-bit elements in-place.
   */
  swap64(): Buffer {
    if (this.length % 8 !== 0) {
      throw new RangeError("Buffer size must be a multiple of 64-bits");
    }
    for (let i = 0; i < this.length; i += 8) {
      const t0 = this._data[i]!;
      const t1 = this._data[i + 1]!;
      const t2 = this._data[i + 2]!;
      const t3 = this._data[i + 3]!;
      this._data[i] = this._data[i + 7]!;
      this._data[i + 1] = this._data[i + 6]!;
      this._data[i + 2] = this._data[i + 5]!;
      this._data[i + 3] = this._data[i + 4]!;
      this._data[i + 4] = t3;
      this._data[i + 5] = t2;
      this._data[i + 6] = t1;
      this._data[i + 7] = t0;
    }
    return this;
  }

  // ---- instance: read* methods ----

  readUInt8(offset: int = 0): number {
    return this._data[offset]!;
  }

  readUint8(offset: int = 0): number {
    return this.readUInt8(offset);
  }

  readInt8(offset: int = 0): number {
    const val = this._data[offset]!;
    return val >= 0x80 ? val - 0x100 : val;
  }

  readUInt16LE(offset: int = 0): number {
    return this._data[offset]! | (this._data[offset + 1]! << 8);
  }

  readUint16LE(offset: int = 0): number {
    return this.readUInt16LE(offset);
  }

  readInt16LE(offset: int = 0): number {
    const val = this.readUInt16LE(offset);
    return val >= 0x8000 ? val - 0x10000 : val;
  }

  readUInt16BE(offset: int = 0): number {
    return (this._data[offset]! << 8) | this._data[offset + 1]!;
  }

  readUint16BE(offset: int = 0): number {
    return this.readUInt16BE(offset);
  }

  readInt16BE(offset: int = 0): number {
    const val = this.readUInt16BE(offset);
    return val >= 0x8000 ? val - 0x10000 : val;
  }

  readUInt32LE(offset: int = 0): number {
    return (
      (this._data[offset]! |
        (this._data[offset + 1]! << 8) |
        (this._data[offset + 2]! << 16) |
        (this._data[offset + 3]! << 24)) >>>
      0
    );
  }

  readUint32LE(offset: int = 0): number {
    return this.readUInt32LE(offset);
  }

  readInt32LE(offset: int = 0): number {
    return (
      this._data[offset]! |
      (this._data[offset + 1]! << 8) |
      (this._data[offset + 2]! << 16) |
      (this._data[offset + 3]! << 24)
    );
  }

  readUInt32BE(offset: int = 0): number {
    return (
      ((this._data[offset]! << 24) |
        (this._data[offset + 1]! << 16) |
        (this._data[offset + 2]! << 8) |
        this._data[offset + 3]!) >>>
      0
    );
  }

  readUint32BE(offset: int = 0): number {
    return this.readUInt32BE(offset);
  }

  readInt32BE(offset: int = 0): number {
    return (
      (this._data[offset]! << 24) |
      (this._data[offset + 1]! << 16) |
      (this._data[offset + 2]! << 8) |
      this._data[offset + 3]!
    );
  }

  /**
   * Reads a float (32-bit) at offset, little-endian.
   */
  readFloatLE(offset: int = 0): number {
    const bytes = BufferInternals.copyRange(this._data, offset, offset + 4);
    return BufferInternals.readIeee754(bytes, true, 23, 4);
  }

  /**
   * Reads a float (32-bit) at offset, big-endian.
   */
  readFloatBE(offset: int = 0): number {
    const bytes = BufferInternals.copyRange(this._data, offset, offset + 4);
    return BufferInternals.readIeee754(bytes, false, 23, 4);
  }

  /**
   * Reads a double (64-bit) at offset, little-endian.
   */
  readDoubleLE(offset: int = 0): number {
    const bytes = BufferInternals.copyRange(this._data, offset, offset + 8);
    return BufferInternals.readIeee754(bytes, true, 52, 8);
  }

  /**
   * Reads a double (64-bit) at offset, big-endian.
   */
  readDoubleBE(offset: int = 0): number {
    const bytes = BufferInternals.copyRange(this._data, offset, offset + 8);
    return BufferInternals.readIeee754(bytes, false, 52, 8);
  }

  /**
   * Reads a 64-bit unsigned integer at offset, little-endian.
   */
  readBigUInt64LE(offset: int = 0): ulong {
    const bytes = BufferInternals.copyRange(this._data, offset, offset + 8);
    return BufferInternals.readUInt64(bytes, true);
  }

  readBigUint64LE(offset: int = 0): ulong {
    return this.readBigUInt64LE(offset);
  }

  /**
   * Reads a 64-bit signed integer at offset, little-endian.
   */
  readBigInt64LE(offset: int = 0): long {
    const bytes = BufferInternals.copyRange(this._data, offset, offset + 8);
    return BufferInternals.readInt64(bytes, true);
  }

  /**
   * Reads a 64-bit unsigned integer at offset, big-endian.
   */
  readBigUInt64BE(offset: int = 0): ulong {
    const bytes = BufferInternals.copyRange(this._data, offset, offset + 8);
    return BufferInternals.readUInt64(bytes, false);
  }

  readBigUint64BE(offset: int = 0): ulong {
    return this.readBigUInt64BE(offset);
  }

  /**
   * Reads a 64-bit signed integer at offset, big-endian.
   */
  readBigInt64BE(offset: int = 0): long {
    const bytes = BufferInternals.copyRange(this._data, offset, offset + 8);
    return BufferInternals.readInt64(bytes, false);
  }

  /**
   * Reads `byteLength` bytes from offset as unsigned int (up to 48-bit), LE.
   */
  readUIntLE(offset: int, byteLength: int): number {
    if (byteLength < 1 || byteLength > 6) {
      throw new RangeError("byteLength must be between 1 and 6");
    }
    let value: number = 0;
    for (let i = 0; i < byteLength; i += 1) {
      value += this._data[offset + i]! * BufferInternals.pow256(i);
    }
    return value;
  }

  readUintLE(offset: int, byteLength: int): number {
    return this.readUIntLE(offset, byteLength);
  }

  /**
   * Reads `byteLength` bytes from offset as signed int (up to 48-bit), LE.
   */
  readIntLE(offset: int, byteLength: int): number {
    if (byteLength < 1 || byteLength > 6) {
      throw new RangeError("byteLength must be between 1 and 6");
    }
    let value: number = 0;
    for (let i = 0; i < byteLength; i += 1) {
      value += this._data[offset + i]! * BufferInternals.pow256(i);
    }
    // Sign extend
    const limit = BufferInternals.pow2(byteLength * 8 - 1);
    if (value >= limit) {
      value -= BufferInternals.pow2(byteLength * 8);
    }
    return value;
  }

  /**
   * Reads `byteLength` bytes from offset as unsigned int (up to 48-bit), BE.
   */
  readUIntBE(offset: int, byteLength: int): number {
    if (byteLength < 1 || byteLength > 6) {
      throw new RangeError("byteLength must be between 1 and 6");
    }
    let value: number = 0;
    for (let i = 0; i < byteLength; i += 1) {
      value = value * 256 + this._data[offset + i]!;
    }
    return value;
  }

  readUintBE(offset: int, byteLength: int): number {
    return this.readUIntBE(offset, byteLength);
  }

  /**
   * Reads `byteLength` bytes from offset as signed int (up to 48-bit), BE.
   */
  readIntBE(offset: int, byteLength: int): number {
    if (byteLength < 1 || byteLength > 6) {
      throw new RangeError("byteLength must be between 1 and 6");
    }
    let value: number = 0;
    for (let i = 0; i < byteLength; i += 1) {
      value = value * 256 + this._data[offset + i]!;
    }
    const limit = BufferInternals.pow2(byteLength * 8 - 1);
    if (value >= limit) {
      value -= BufferInternals.pow2(byteLength * 8);
    }
    return value;
  }

  // ---- instance: write* methods ----

  writeUInt8(value: number, offset: int = 0): number {
    this._data[offset] = Convert.ToByte(value & 0xff);
    return offset + 1;
  }

  writeUint8(value: number, offset: int = 0): number {
    return this.writeUInt8(value, offset);
  }

  writeInt8(value: number, offset: int = 0): number {
    this._data[offset] = Convert.ToByte(value & 0xff);
    return offset + 1;
  }

  writeUInt16LE(value: number, offset: int = 0): number {
    this._data[offset] = Convert.ToByte(value & 0xff);
    this._data[offset + 1] = Convert.ToByte((value >> 8) & 0xff);
    return offset + 2;
  }

  writeUint16LE(value: number, offset: int = 0): number {
    return this.writeUInt16LE(value, offset);
  }

  writeInt16LE(value: number, offset: int = 0): number {
    this._data[offset] = Convert.ToByte(value & 0xff);
    this._data[offset + 1] = Convert.ToByte((value >> 8) & 0xff);
    return offset + 2;
  }

  writeUInt16BE(value: number, offset: int = 0): number {
    this._data[offset] = Convert.ToByte((value >> 8) & 0xff);
    this._data[offset + 1] = Convert.ToByte(value & 0xff);
    return offset + 2;
  }

  writeUint16BE(value: number, offset: int = 0): number {
    return this.writeUInt16BE(value, offset);
  }

  writeInt16BE(value: number, offset: int = 0): number {
    this._data[offset] = Convert.ToByte((value >> 8) & 0xff);
    this._data[offset + 1] = Convert.ToByte(value & 0xff);
    return offset + 2;
  }

  writeUInt32LE(value: number, offset: int = 0): number {
    this._data[offset] = Convert.ToByte(value & 0xff);
    this._data[offset + 1] = Convert.ToByte((value >>> 8) & 0xff);
    this._data[offset + 2] = Convert.ToByte((value >>> 16) & 0xff);
    this._data[offset + 3] = Convert.ToByte((value >>> 24) & 0xff);
    return offset + 4;
  }

  writeUint32LE(value: number, offset: int = 0): number {
    return this.writeUInt32LE(value, offset);
  }

  writeInt32LE(value: number, offset: int = 0): number {
    this._data[offset] = Convert.ToByte(value & 0xff);
    this._data[offset + 1] = Convert.ToByte((value >>> 8) & 0xff);
    this._data[offset + 2] = Convert.ToByte((value >>> 16) & 0xff);
    this._data[offset + 3] = Convert.ToByte((value >>> 24) & 0xff);
    return offset + 4;
  }

  writeUInt32BE(value: number, offset: int = 0): number {
    this._data[offset] = Convert.ToByte((value >>> 24) & 0xff);
    this._data[offset + 1] = Convert.ToByte((value >>> 16) & 0xff);
    this._data[offset + 2] = Convert.ToByte((value >>> 8) & 0xff);
    this._data[offset + 3] = Convert.ToByte(value & 0xff);
    return offset + 4;
  }

  writeUint32BE(value: number, offset: int = 0): number {
    return this.writeUInt32BE(value, offset);
  }

  writeInt32BE(value: number, offset: int = 0): number {
    this._data[offset] = Convert.ToByte((value >>> 24) & 0xff);
    this._data[offset + 1] = Convert.ToByte((value >>> 16) & 0xff);
    this._data[offset + 2] = Convert.ToByte((value >>> 8) & 0xff);
    this._data[offset + 3] = Convert.ToByte(value & 0xff);
    return offset + 4;
  }

  /**
   * Writes a float (32-bit) at offset, little-endian.
   */
  writeFloatLE(value: number, offset: int = 0): number {
    const bytes = BufferInternals.writeIeee754(value, true, 23, 4);
    BufferInternals.copyInto(bytes, 0, bytes.length, this._data, offset);
    return offset + 4;
  }

  /**
   * Writes a float (32-bit) at offset, big-endian.
   */
  writeFloatBE(value: number, offset: int = 0): number {
    const bytes = BufferInternals.writeIeee754(value, false, 23, 4);
    BufferInternals.copyInto(bytes, 0, bytes.length, this._data, offset);
    return offset + 4;
  }

  /**
   * Writes a double (64-bit) at offset, little-endian.
   */
  writeDoubleLE(value: number, offset: int = 0): number {
    const bytes = BufferInternals.writeIeee754(value, true, 52, 8);
    BufferInternals.copyInto(bytes, 0, bytes.length, this._data, offset);
    return offset + 8;
  }

  /**
   * Writes a double (64-bit) at offset, big-endian.
   */
  writeDoubleBE(value: number, offset: int = 0): number {
    const bytes = BufferInternals.writeIeee754(value, false, 52, 8);
    BufferInternals.copyInto(bytes, 0, bytes.length, this._data, offset);
    return offset + 8;
  }

  /**
   * Writes a 64-bit unsigned integer at offset, little-endian.
   */
  writeBigUInt64LE(value: ulong, offset: int = 0): number {
    const bytes = BufferInternals.writeUInt64(value, true);
    BufferInternals.copyInto(bytes, 0, bytes.length, this._data, offset);
    return offset + 8;
  }

  writeBigUint64LE(value: ulong, offset: int = 0): number {
    return this.writeBigUInt64LE(value, offset);
  }

  /**
   * Writes a 64-bit signed integer at offset, little-endian.
   */
  writeBigInt64LE(value: long, offset: int = 0): number {
    const bytes = BufferInternals.writeInt64(value, true);
    BufferInternals.copyInto(bytes, 0, bytes.length, this._data, offset);
    return offset + 8;
  }

  /**
   * Writes a 64-bit unsigned integer at offset, big-endian.
   */
  writeBigUInt64BE(value: ulong, offset: int = 0): number {
    const bytes = BufferInternals.writeUInt64(value, false);
    BufferInternals.copyInto(bytes, 0, bytes.length, this._data, offset);
    return offset + 8;
  }

  writeBigUint64BE(value: ulong, offset: int = 0): number {
    return this.writeBigUInt64BE(value, offset);
  }

  /**
   * Writes a 64-bit signed integer at offset, big-endian.
   */
  writeBigInt64BE(value: long, offset: int = 0): number {
    const bytes = BufferInternals.writeInt64(value, false);
    BufferInternals.copyInto(bytes, 0, bytes.length, this._data, offset);
    return offset + 8;
  }

  /**
   * Writes `byteLength` bytes of unsigned `value` at offset, little-endian.
   */
  writeUIntLE(value: number, offset: int, byteLength: int): number {
    if (byteLength < 1 || byteLength > 6) {
      throw new RangeError("byteLength must be between 1 and 6");
    }
    let v: number = value;
    for (let i = 0; i < byteLength; i += 1) {
      this._data[offset + i] = Convert.ToByte(v & 0xff);
      v = Math.floor(v / 256);
    }
    return offset + byteLength;
  }

  writeUintLE(value: number, offset: int, byteLength: int): number {
    return this.writeUIntLE(value, offset, byteLength);
  }

  /**
   * Writes `byteLength` bytes of signed `value` at offset, little-endian.
   */
  writeIntLE(value: number, offset: int, byteLength: int): number {
    if (byteLength < 1 || byteLength > 6) {
      throw new RangeError("byteLength must be between 1 and 6");
    }
    let v: number = value < 0 ? value + BufferInternals.pow2(byteLength * 8) : value;
    for (let i = 0; i < byteLength; i += 1) {
      this._data[offset + i] = Convert.ToByte(v & 0xff);
      v = Math.floor(v / 256);
    }
    return offset + byteLength;
  }

  /**
   * Writes `byteLength` bytes of unsigned `value` at offset, big-endian.
   */
  writeUIntBE(value: number, offset: int, byteLength: int): number {
    if (byteLength < 1 || byteLength > 6) {
      throw new RangeError("byteLength must be between 1 and 6");
    }
    let v: number = value;
    for (let i = byteLength - 1; i >= 0; i -= 1) {
      this._data[offset + i] = Convert.ToByte(v & 0xff);
      v = Math.floor(v / 256);
    }
    return offset + byteLength;
  }

  writeUintBE(value: number, offset: int, byteLength: int): number {
    return this.writeUIntBE(value, offset, byteLength);
  }

  /**
   * Writes `byteLength` bytes of signed `value` at offset, big-endian.
   */
  writeIntBE(value: number, offset: int, byteLength: int): number {
    if (byteLength < 1 || byteLength > 6) {
      throw new RangeError("byteLength must be between 1 and 6");
    }
    let v: number = value < 0 ? value + BufferInternals.pow2(byteLength * 8) : value;
    for (let i = byteLength - 1; i >= 0; i -= 1) {
      this._data[offset + i] = Convert.ToByte(v & 0xff);
      v = Math.floor(v / 256);
    }
    return offset + byteLength;
  }

  // ---- private helpers ----

  writeHex(hex: string, offset: int, maxLength: int): number {
    const cleaned = BufferInternals.stripWhitespace(hex);
    const wholeHexBytes: number = Math.floor(cleaned.length / 2.0);
    const bytesToWrite = BufferInternals.toInt(
      wholeHexBytes < maxLength ? wholeHexBytes : maxLength
    );
    for (let i = 0; i < bytesToWrite; i += 1) {
      this._data[offset + i] =
        Convert.ToByte(
          (parseInt(cleaned.substring(i * 2, i * 2 + 2), 16) ?? 0) & 0xff
        );
    }
    return bytesToWrite;
  }

  writeBase64(
    b64: string,
    offset: int,
    maxLength: int,
    isBase64Url: boolean,
  ): number {
    const decoded = isBase64Url
      ? base64ToBytes(base64UrlToBase64(b64))
      : base64ToBytes(b64);
    const bytesToWrite = BufferInternals.minInt(decoded.length, maxLength);
    BufferInternals.copyInto(decoded, 0, bytesToWrite, this._data, offset);
    return bytesToWrite;
  }
}

O<typeof Buffer>().method((buffer) => buffer.fromString).family((buffer) => buffer.from);
O<typeof Buffer>().method((buffer) => buffer.fromArray).family((buffer) => buffer.from);
O<typeof Buffer>().method((buffer) => buffer.fromBuffer).family((buffer) => buffer.from);
O<typeof Buffer>().method((buffer) => buffer.fromUint8Array).family((buffer) => buffer.from);
O<Buffer>().method((buffer) => buffer.indexOf_string).family((buffer) => buffer.indexOf);
O<Buffer>().method((buffer) => buffer.indexOf_number).family((buffer) => buffer.indexOf);
O<Buffer>().method((buffer) => buffer.indexOf_buffer).family((buffer) => buffer.indexOf);
O<Buffer>().method((buffer) => buffer.lastIndexOf_string).family((buffer) => buffer.lastIndexOf);
O<Buffer>().method((buffer) => buffer.lastIndexOf_number).family((buffer) => buffer.lastIndexOf);
O<Buffer>().method((buffer) => buffer.lastIndexOf_buffer).family((buffer) => buffer.lastIndexOf);
O<Buffer>().method((buffer) => buffer.includes_string).family((buffer) => buffer.includes);
O<Buffer>().method((buffer) => buffer.includes_number).family((buffer) => buffer.includes);
O<Buffer>().method((buffer) => buffer.includes_buffer).family((buffer) => buffer.includes);
O<Buffer>().method((buffer) => buffer.fill_string).family((buffer) => buffer.fill);
O<Buffer>().method((buffer) => buffer.fill_number).family((buffer) => buffer.fill);
O<Buffer>().method((buffer) => buffer.fill_buffer).family((buffer) => buffer.fill);
