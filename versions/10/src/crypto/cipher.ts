import type { int } from "@tsonic/core/types.js";
import { overloads as O } from "@tsonic/core/lang.js";
import {
  concatBytes,
  decodeInputBytes,
  encodeOutputBytes,
  toInt,
  transformAes,
  transformAesGcmEncrypt,
} from "./crypto-helpers.ts";

/**
 * Node.js crypto Cipher class.
 *
 */

/**
 * Instances of the Cipher class are used to encrypt data.
 */
export class Cipher {
  _algorithm: string;
  _key: Uint8Array;
  _iv: Uint8Array | null;
  _isGcmMode: boolean;
  _chunks: Uint8Array[] = [];
  _gcmTag: Uint8Array | null = null;
  _gcmAad: Uint8Array | null = null;
  _gcmTagLength: int = 16;
  _finalized: boolean = false;

  constructor(algorithm: string, key: Uint8Array, iv: Uint8Array | null) {
    this._algorithm = algorithm;
    this._key = key;
    this._iv = iv;
    this._isGcmMode = algorithm.toLowerCase().includes("-gcm");
  }

  /**
   * Updates the cipher with data.
   */
  update(data: string, inputEncoding?: string, outputEncoding?: string): string;
  update(data: Uint8Array, outputEncoding?: string): string;
  update(_data: any, _inputOrOutputEncoding?: any, _outputEncoding?: any): any {
    throw new Error("Unreachable overload stub");
  }

  update_string(
    data: string,
    inputEncoding?: string,
    _outputEncoding?: string,
  ): string {
    this.pushChunk(decodeInputBytes(data, inputEncoding ?? "utf8"));
    return "";
  }

  update_bytes(data: Uint8Array, _outputEncoding?: string): string {
    this.pushChunk(data);
    return "";
  }

  /**
   * Returns any remaining enciphered contents.
   */
  final(outputEncoding: string): string;
  final(): Uint8Array;
  final(_outputEncoding?: any): any {
    throw new Error("Unreachable overload stub");
  }

  final_string(outputEncoding: string): string {
    return encodeOutputBytes(this.finalizeBytes(), outputEncoding) as string;
  }

  final_bytes(): Uint8Array {
    return this.finalizeBytes();
  }

  /**
   * When using an authenticated encryption mode, sets the length of the authentication tag.
   */
  setAuthTag(tagLength: number): void {
    if (!this._isGcmMode) {
      throw new Error("setAuthTag is only supported for GCM modes");
    }
    if (this._finalized) {
      throw new Error("Cannot set auth tag length after finalization");
    }
    if (!Number.isInteger(tagLength) || tagLength < 4 || tagLength > 16) {
      throw new RangeError("GCM auth tag length must be an integer between 4 and 16");
    }
    const normalizedTagLength = Math.floor(tagLength);
    switch (normalizedTagLength) {
      case 4:
        this._gcmTagLength = 4;
        return;
      case 5:
        this._gcmTagLength = 5;
        return;
      case 6:
        this._gcmTagLength = 6;
        return;
      case 7:
        this._gcmTagLength = 7;
        return;
      case 8:
        this._gcmTagLength = 8;
        return;
      case 9:
        this._gcmTagLength = 9;
        return;
      case 10:
        this._gcmTagLength = 10;
        return;
      case 11:
        this._gcmTagLength = 11;
        return;
      case 12:
        this._gcmTagLength = 12;
        return;
      case 13:
        this._gcmTagLength = 13;
        return;
      case 14:
        this._gcmTagLength = 14;
        return;
      case 15:
        this._gcmTagLength = 15;
        return;
      case 16:
        this._gcmTagLength = 16;
        return;
      default:
        throw new RangeError("GCM auth tag length must be an integer between 4 and 16");
    }
  }

  /**
   * When using an authenticated encryption mode, returns the authentication tag.
   */
  getAuthTag(): Uint8Array {
    if (!this._isGcmMode) {
      throw new Error("getAuthTag is only supported for GCM modes");
    }

    if (!this._finalized) {
      throw new Error("Must call final() before getAuthTag()");
    }

    if (this._gcmTag === null) {
      throw new Error("No auth tag available");
    }

    return this._gcmTag;
  }

  /**
   * When using an authenticated encryption mode, sets AAD (Additional Authenticated Data).
   */
  setAAD(buffer: Uint8Array): void {
    if (!this._isGcmMode) {
      throw new Error("setAAD is only supported for GCM modes");
    }

    if (this._finalized) {
      throw new Error("Cannot set AAD after finalization");
    }

    this._gcmAad = buffer;
    void this._gcmAad;
  }

  pushChunk(chunk: Uint8Array): void {
    if (this._finalized) {
      throw new Error("Cipher already finalized");
    }

    this._chunks.push(chunk);
  }

  finalizeBytes(): Uint8Array {
    if (this._finalized) {
      throw new Error("Cipher already finalized");
    }

    this._finalized = true;
    if (this._isGcmMode) {
      if (this._iv === null) {
        throw new Error(`Invalid IV for ${this._algorithm}`);
      }
      const result = transformAesGcmEncrypt(
        this._algorithm,
        this._key,
        this._iv,
        concatBytes(...this._chunks),
        {
          aad: this._gcmAad,
          authTagLength: this._gcmTagLength,
        },
      );
      this._gcmTag = result.authTag;
      return result.ciphertext;
    }

    return transformAes(
      this._algorithm,
      this._key,
      this._iv,
      concatBytes(...this._chunks),
      true,
    );
  }
}

O<Cipher>().method(x => x.update_string).family(x => x.update);
O<Cipher>().method(x => x.update_bytes).family(x => x.update);
O<Cipher>().method(x => x.final_string).family(x => x.final);
O<Cipher>().method(x => x.final_bytes).family(x => x.final);
