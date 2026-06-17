import { overloads as O } from "@tsonic/core/lang.js";
import {
  computeHmacBytes,
  concatBytes,
  decodeInputBytes,
  encodeOutputBytes,
} from "./crypto-helpers.ts";

/**
 * Node.js crypto Hmac class.
 *
 */

/**
 * The Hmac class is a utility for creating cryptographic HMAC digests.
 */
export class Hmac {
  _algorithm: string;
  _key: Uint8Array;
  _chunks: Uint8Array[] = [];
  _finalized: boolean = false;

  constructor(algorithm: string, key: Uint8Array) {
    this._algorithm = algorithm;
    this._key = key;
  }

  /**
   * Updates the Hmac content with the given data.
   */
  update(data: string, inputEncoding?: string): Hmac;
  update(data: Uint8Array): Hmac;
  update(_data: any, _inputEncoding?: any): any {
    throw new Error("Unreachable overload stub");
  }

  update_string(data: string, inputEncoding?: string): Hmac {
    this.pushChunk(decodeInputBytes(data, inputEncoding ?? "utf8"));
    return this;
  }

  update_bytes(data: Uint8Array): Hmac {
    this.pushChunk(data);
    return this;
  }

  /**
   * Calculates the HMAC digest of all the data passed.
   */
  digest(encoding: string): string;
  digest(): Uint8Array;
  digest(_encoding?: any): any {
    throw new Error("Unreachable overload stub");
  }

  digest_encoding(encoding: string): string {
    return encodeOutputBytes(this.finalizeDigest(), encoding) as string;
  }

  digest_bytes(): Uint8Array {
    return this.finalizeDigest();
  }

  pushChunk(chunk: Uint8Array): void {
    if (this._finalized) {
      throw new Error("Digest already called");
    }

    this._chunks.push(chunk);
  }

  finalizeDigest(): Uint8Array {
    if (this._finalized) {
      throw new Error("Digest already called");
    }

    this._finalized = true;
    return computeHmacBytes(
      this._algorithm,
      this._key,
      concatBytes(...this._chunks),
    );
  }
}

O<Hmac>().method(x => x.update_string).family(x => x.update);
O<Hmac>().method(x => x.update_bytes).family(x => x.update);
O<Hmac>().method(x => x.digest_encoding).family(x => x.digest);
O<Hmac>().method(x => x.digest_bytes).family(x => x.digest);
