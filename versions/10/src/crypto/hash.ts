import { overloads as O } from "@tsonic/core/lang.js";
import {
  computeHashBytes,
  concatBytes,
  decodeInputBytes,
  encodeOutputString,
} from "./crypto-helpers.ts";

/**
 * Node.js crypto Hash class.
 *
 */

/**
 * The Hash class is a utility for creating hash digests of data.
 */
export class Hash {
  _algorithm: string;
  _chunks: Uint8Array[] = [];
  _finalized: boolean = false;

  constructor(algorithm: string) {
    this._algorithm = algorithm;
  }

  /**
   * Updates the hash content with the given data.
   */
  update(data: string, inputEncoding?: string): Hash;
  update(data: Uint8Array): Hash;
  update(_data: any, _inputEncoding?: any): any {
    throw new Error("Unreachable overload stub");
  }

  update_string(data: string, inputEncoding?: string): Hash {
    this.pushChunk(decodeInputBytes(data, inputEncoding ?? "utf8"));
    return this;
  }

  update_bytes(data: Uint8Array): Hash {
    this.pushChunk(data);
    return this;
  }

  /**
   * Calculates the digest of all data passed to be hashed.
   */
  digest(encoding: string): string;
  digest(): Uint8Array;
  digest(outputLength: number): Uint8Array;
  digest(_encodingOrLength?: any): any {
    throw new Error("Unreachable overload stub");
  }

  digest_encoding(encoding: string): string {
    return encodeOutputString(this.finalizeDigest(), encoding);
  }

  digest_bytes(): Uint8Array {
    return this.finalizeDigest();
  }

  digest_length(outputLength: number): Uint8Array {
    return this.finalizeDigest(outputLength);
  }

  /**
   * Creates a copy of the Hash object in its current state.
   */
  copy(): Hash {
    if (this._finalized) {
      throw new Error("Cannot copy finalized hash");
    }

    const copy = new Hash(this._algorithm);
    for (let index = 0; index < this._chunks.length; index += 1) {
      copy._chunks.push(this._chunks[index]!);
    }
    return copy;
  }

  pushChunk(chunk: Uint8Array): void {
    if (this._finalized) {
      throw new Error("Digest already called");
    }

    this._chunks.push(chunk);
  }

  finalizeDigest(outputLength?: number): Uint8Array {
    if (this._finalized) {
      throw new Error("Digest already called");
    }

    this._finalized = true;
    return computeHashBytes(
      this._algorithm,
      concatBytes(...this._chunks),
      outputLength,
    );
  }
}

O<Hash>().method(x => x.update_string).family(x => x.update);
O<Hash>().method(x => x.update_bytes).family(x => x.update);
O<Hash>().method(x => x.digest_encoding).family(x => x.digest);
O<Hash>().method(x => x.digest_bytes).family(x => x.digest);
O<Hash>().method(x => x.digest_length).family(x => x.digest);
