/**
 * Buffer encoding helpers -- normalizing encoding names and converting
 * between byte arrays and encoded string forms (hex, base64, base64url).
 *
 */

import type { byte, int } from "@tsonic/core/types.js";
import { Convert } from "@tsonic/dotnet/System.js";

export type BufferEncoding =
  | "utf8"
  | "utf-8"
  | "ascii"
  | "latin1"
  | "binary"
  | "utf16le"
  | "utf-16le"
  | "ucs2"
  | "ucs-2"
  | "base64"
  | "base64url"
  | "hex";

const HEX_DIGITS = "0123456789abcdef";

const toInt = (value: number): int => {
  if (Number.isInteger(value) && value >= -2147483648 && value <= 2147483647) {
    return value as int;
  }

  throw new RangeError("Expected Int32-compatible numeric value");
};

const copyRange = (
  bytes: Uint8Array,
  start: int,
  end: int,
): Uint8Array => {
  const safeStart = start < 0 ? 0 : start;
  const safeEnd = end < safeStart ? safeStart : end;
  const result = new Uint8Array((safeEnd - safeStart) as int);
  for (let index = 0; index < result.length; index += 1) {
    result[index] = bytes[safeStart + index]!;
  }
  return result;
};

const stripCharacters = (value: string, shouldStrip: (char: string) => boolean): string => {
  let result = "";
  for (let index = 0; index < value.length; index += 1) {
    const char = value.charAt(index);
    if (!shouldStrip(char)) {
      result += char;
    }
  }
  return result;
};

const toUint8Array = (values: readonly number[]): Uint8Array => {
  const bytes = new Uint8Array(values.length);
  for (let index = 0; index < values.length; index += 1) {
    bytes[index] = values[index]! & 0xff;
  }
  return bytes;
};

const fromCharCode = (code: int): string => Convert.ToChar(code).toString();

const utf8StringToBytes = (value: string): Uint8Array => {
  const encoded = encodeURIComponent(value);
  const bytes: number[] = [];

  for (let index = 0; index < encoded.length; index += 1) {
    const char = encoded.charAt(index);
    if (char === "%") {
      const hex = encoded.substring(index + 1, index + 3);
      bytes.push(Convert.ToByte(parseInt(hex, 16)));
      index += 2;
      continue;
    }

    bytes.push(encoded.charCodeAt(index));
  }

  return toUint8Array(bytes);
};

const utf8BytesToString = (bytes: Uint8Array): string => {
  let encoded = "";
  for (let index = 0; index < bytes.length; index += 1) {
    const value = bytes[index]!;
    encoded += `%${HEX_DIGITS.charAt((value >> 4) & 0x0f)}${HEX_DIGITS.charAt(value & 0x0f)}`;
  }
  return decodeURIComponent(encoded);
};

/**
 * Normalizes encoding name to a canonical lowercase form with no hyphens/underscores.
 */
export const normalizeEncoding = (encoding: string): string => {
  const lowered = encoding.toLowerCase();
  return stripCharacters(lowered, (char) => char === "-" || char === "_");
};

/**
 * Encodes a string to bytes using the specified encoding.
 * For standard text encodings this uses TextEncoder / manual loops;
 * for hex and base64 it decodes the *string* as that format into raw bytes.
 */
export const stringToBytes = (
  str: string,
  encoding: string,
): Uint8Array => {
  const norm = normalizeEncoding(encoding);

  switch (norm) {
    case "utf8":
      return utf8StringToBytes(str);

    case "ascii": {
      const out = new Uint8Array(str.length);
      for (let i = 0; i < str.length; i += 1) {
        out[i] = str.charCodeAt(i) & 0x7f;
      }
      return out;
    }

    case "latin1":
    case "binary": {
      const out = new Uint8Array(str.length);
      for (let i = 0; i < str.length; i += 1) {
        out[i] = str.charCodeAt(i) & 0xff;
      }
      return out;
    }

    case "utf16le":
    case "ucs2": {
      const out = new Uint8Array(str.length * 2);
      for (let i = 0; i < str.length; i += 1) {
        const code = str.charCodeAt(i);
        out[i * 2] = code & 0xff;
        out[i * 2 + 1] = (code >> 8) & 0xff;
      }
      return out;
    }

    case "hex":
      return hexToBytes(str);

    case "base64":
      return base64ToBytes(str);

    case "base64url":
      return base64ToBytes(base64UrlToBase64(str));

    default:
      // Fall back to utf-8
      return utf8StringToBytes(str);
  }
};

/**
 * Decodes a byte range to a string using the specified encoding.
 */
export const bytesToString = (
  bytes: Uint8Array,
  encoding: string,
  start: int,
  end: int,
): string => {
  const norm = normalizeEncoding(encoding);
  const slice = copyRange(bytes, start, end);

  switch (norm) {
    case "utf8":
      return utf8BytesToString(slice);

    case "ascii": {
      let result = "";
      for (let i = 0; i < slice.length; i += 1) {
        result += fromCharCode(toInt(slice[i]! & 0x7f));
      }
      return result;
    }

    case "latin1":
    case "binary": {
      let result = "";
      for (let i = 0; i < slice.length; i += 1) {
        result += fromCharCode(toInt(slice[i]!));
      }
      return result;
    }

    case "utf16le":
    case "ucs2": {
      let result = "";
      for (let i = 0; i + 1 < slice.length; i += 2) {
        result += fromCharCode(toInt(slice[i]! | (slice[i + 1]! << 8)));
      }
      return result;
    }

    case "hex":
      return bytesToHex(slice, 0, slice.length);

    case "base64":
      return bytesToBase64(slice);

    case "base64url":
      return base64ToBase64Url(bytesToBase64(slice));

    default:
      return utf8BytesToString(slice);
  }
};

/**
 * Returns the byte count that a string would take under the given encoding.
 */
export const byteLengthOfString = (
  str: string,
  encoding: string,
): number => {
  const norm = normalizeEncoding(encoding);

  switch (norm) {
    case "utf8":
      return utf8StringToBytes(str).length;

    case "ascii":
    case "latin1":
    case "binary":
      return str.length;

    case "utf16le":
    case "ucs2":
      return str.length * 2;

    case "hex":
      return toInt(Math.floor(str.length / 2));

    case "base64":
    case "base64url": {
      // Base64 encodes 3 bytes per 4 chars; strip padding for size calculation
      let stripped = str;
      if (norm === "base64url") {
        stripped = base64UrlToBase64(stripped);
      }
      const pad = stripped.endsWith("==") ? 2 : stripped.endsWith("=") ? 1 : 0;
      return toInt(Math.floor((stripped.length * 3) / 4) - pad);
    }

    default:
      return utf8StringToBytes(str).length;
  }
};

// ---- hex helpers ----

export const hexToBytes = (hex: string): Uint8Array => {
  const cleaned = stripCharacters(hex, (char) => char === " " || char === "\n" || char === "\r" || char === "\t");
  const bytes = new Uint8Array(cleaned.length >> 1);
  for (let i = 0; i < bytes.length; i += 1) {
    bytes[i] = Convert.ToByte(parseInt(cleaned.substring(i * 2, i * 2 + 2), 16) ?? 0);
  }
  return bytes;
};

export const bytesToHex = (
  bytes: Uint8Array,
  start: int,
  end: int,
): string => {
  let hex = "";
  for (let i = start; i < end; i += 1) {
    const value = bytes[i]!;
    hex += HEX_DIGITS.charAt((value >> 4) & 0x0f);
    hex += HEX_DIGITS.charAt(value & 0x0f);
  }
  return hex;
};

// ---- base64 helpers ----

const BASE64_CHARS =
  "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789+/";

export const bytesToBase64 = (bytes: Uint8Array): string => {
  let result = "";
  for (let i = 0; i < bytes.length; i += 3) {
    const b0 = bytes[i]! as byte;
    const b1 = i + 1 < bytes.length ? (bytes[i + 1]! as byte) : (0 as byte);
    const b2 = i + 2 < bytes.length ? (bytes[i + 2]! as byte) : (0 as byte);

    result += BASE64_CHARS[(b0 >> 2) & 0x3f];
    result += BASE64_CHARS[((b0 << 4) | (b1 >> 4)) & 0x3f];
    result += i + 1 < bytes.length ? BASE64_CHARS[((b1 << 2) | (b2 >> 6)) & 0x3f] : "=";
    result += i + 2 < bytes.length ? BASE64_CHARS[b2 & 0x3f] : "=";
  }
  return result;
};

const BASE64_LOOKUP: Record<string, int> = {};
for (let i = 0; i < BASE64_CHARS.length; i += 1) {
  BASE64_LOOKUP[BASE64_CHARS[i]!] = i as int;
}

export const base64ToBytes = (base64: string): Uint8Array => {
  let stripped = base64;
  while (stripped.endsWith("=")) {
    stripped = stripped.slice(0, stripped.length - 1);
  }
  const byteLength = (stripped.length * 3) >> 2;
  const bytes = new Uint8Array(byteLength);
  let offset: int = 0 as int;

  for (let i = 0; i < stripped.length; i += 4) {
    const a = BASE64_LOOKUP[stripped[i]!] ?? (0 as int);
    const b = BASE64_LOOKUP[stripped[i + 1]!] ?? (0 as int);
    const c =
      i + 2 < stripped.length
        ? (BASE64_LOOKUP[stripped[i + 2]!] ?? (0 as int))
        : (0 as int);
    const d =
      i + 3 < stripped.length
        ? (BASE64_LOOKUP[stripped[i + 3]!] ?? (0 as int))
        : (0 as int);

    bytes[offset] = ((a << 2) | (b >> 4)) & 0xff;
    offset += 1;
    if (offset < byteLength) {
      bytes[offset] = ((b << 4) | (c >> 2)) & 0xff;
      offset += 1;
    }
    if (offset < byteLength) {
      bytes[offset] = ((c << 6) | d) & 0xff;
      offset += 1;
    }
  }

  return bytes;
};

export const base64UrlToBase64 = (base64url: string): string => {
  let base64 = "";
  for (let index = 0; index < base64url.length; index += 1) {
    const char = base64url.charAt(index);
    if (char === "-") {
      base64 += "+";
    } else if (char === "_") {
      base64 += "/";
    } else {
      base64 += char;
    }
  }
  const padding = (4 - (base64.length % 4)) % 4;
  for (let index = 0; index < padding; index += 1) {
    base64 += "=";
  }
  return base64;
};

export const base64ToBase64Url = (base64: string): string => {
  let result = "";
  for (let index = 0; index < base64.length; index += 1) {
    const char = base64.charAt(index);
    if (char === "+") {
      result += "-";
    } else if (char === "/") {
      result += "_";
    } else if (char !== "=") {
      result += char;
    }
  }
  return result;
};
