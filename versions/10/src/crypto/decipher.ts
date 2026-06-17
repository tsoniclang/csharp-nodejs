import { overloads as O } from "@tsonic/core/lang.js";
import {
  concatBytes,
  decodeInputBytes,
  encodeOutputBytes,
  transformAes,
  transformAesGcmDecrypt,
} from "./crypto-helpers.ts";

/**
 * Node.js crypto Decipher class.
 *
 */

/**
 * Instances of the Decipher class are used to decrypt data.
 */
export class Decipher {
  _algorithm: string;
  _key: Uint8Array;
  _iv: Uint8Array | null;
  _isGcmMode: boolean;
  _chunks: Uint8Array[] = [];
  _gcmTag: Uint8Array | null = null;
  _gcmAad: Uint8Array | null = null;
  _finalized: boolean = false;

  constructor(algorithm: string, key: Uint8Array, iv: Uint8Array | null) {
    this._algorithm = algorithm;
    this._key = key;
    this._iv = iv;
    this._isGcmMode = algorithm.toLowerCase().includes("-gcm");
  }

  /**
   * Updates the decipher with data.
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
   * Returns any remaining deciphered contents.
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
   * When using an authenticated encryption mode, sets the authentication tag.
   */
  setAuthTag(buffer: Uint8Array): void {
    if (!this._isGcmMode) {
      throw new Error("setAuthTag is only supported for GCM modes");
    }

    if (this._finalized) {
      throw new Error("Cannot set auth tag after finalization");
    }

    this._gcmTag = buffer;
    void this._gcmTag;
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
      throw new Error("Decipher already finalized");
    }

    this._chunks.push(chunk);
  }

  finalizeBytes(): Uint8Array {
    if (this._finalized) {
      throw new Error("Decipher already finalized");
    }

    this._finalized = true;
    if (this._isGcmMode) {
      if (this._iv === null) {
        throw new Error(`Invalid IV for ${this._algorithm}`);
      }
      if (this._gcmTag === null) {
        throw new Error("Must call setAuthTag() before final() for GCM modes");
      }
      return transformAesGcmDecrypt(
        this._algorithm,
        this._key,
        this._iv,
        concatBytes(...this._chunks),
        this._gcmTag,
        {
          aad: this._gcmAad,
        },
      );
    }

    return transformAes(
      this._algorithm,
      this._key,
      this._iv,
      concatBytes(...this._chunks),
      false,
    );
  }
}

O<Decipher>().method(x => x.update_string).family(x => x.update);
O<Decipher>().method(x => x.update_bytes).family(x => x.update);
O<Decipher>().method(x => x.final_string).family(x => x.final);
O<Decipher>().method(x => x.final_bytes).family(x => x.final);
