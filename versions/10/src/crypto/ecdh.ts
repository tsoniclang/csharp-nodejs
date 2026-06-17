import type { int, out } from "@tsonic/core/types.js";
import { overloads as O } from "@tsonic/core/lang.js";
import { ECDiffieHellman } from "@tsonic/dotnet/System.Security.Cryptography.js";
import {
  curveFromName,
  decodeInputBytes,
  encodeOutputBytes,
  encodeOutputString,
  fromByteArray,
  toReadOnlyByteSpan,
} from "./crypto-helpers.ts";

function toEcdhPublicKeyBytes(
  otherPublicKey: string,
  inputEncoding?: string,
): Uint8Array {
  return decodeInputBytes(otherPublicKey, inputEncoding ?? "base64");
}

const encodeEcdhSecret = (
  secret: Uint8Array,
  outputEncoding?: string,
): string => {
  return encodeOutputString(secret, outputEncoding ?? "base64");
};

/**
 * Node.js crypto ECDH class.
 *
 */

/**
 * The ECDH class is a utility for creating Elliptic Curve Diffie-Hellman (ECDH) key exchanges.
 */
export class ECDH {
  _curveName: string;
  _ecdh: ECDiffieHellman;
  _publicKeyOverride: Uint8Array | null = null;

  constructor(curveName: string) {
    this._curveName = curveName;
    this._ecdh = ECDiffieHellman.Create(curveFromName(curveName));
  }

  /**
   * Generates private and public EC Diffie-Hellman key values.
   */
  generateKeys(encoding?: undefined, _format?: string): Uint8Array;
  generateKeys(encoding: string, _format?: string): string;
  generateKeys(encoding?: any, format?: any): any {
    throw new Error("Unreachable overload stub");
  }

  generateKeys_bytes(
    _encoding?: undefined,
    _format?: string
  ): Uint8Array {
    return this.publicKeyBytes();
  }

  generateKeys_string(
    encoding: string,
    _format?: string
  ): string {
    return encodeOutputBytes(this.publicKeyBytes(), encoding) as string;
  }

  /**
   * Computes the shared secret using the other party's public key.
   */
  computeSecret(
    otherPublicKey: string,
    inputEncoding?: string,
    outputEncoding?: string
  ): string;
  computeSecret(otherPublicKey: Uint8Array, outputEncoding?: undefined): Uint8Array;
  computeSecret(otherPublicKey: Uint8Array, outputEncoding: string): string;
  computeSecret(
    otherPublicKey: string | Uint8Array,
    inputOrOutputEncoding?: string,
    outputEncoding?: string
  ): any {
    throw new Error("Unreachable overload stub");
  }

  computeSecret_string(
    otherPublicKey: string,
    inputEncoding?: string,
    outputEncoding?: string
  ): string {
    return encodeEcdhSecret(
      this.computeSecretBytes(
        toEcdhPublicKeyBytes(otherPublicKey, inputEncoding),
      ),
      outputEncoding,
    );
  }

  computeSecret_bytes(
    otherPublicKey: Uint8Array,
    _outputEncoding?: undefined
  ): Uint8Array {
    return this.computeSecretBytes(otherPublicKey);
  }

  computeSecret_bytes_string(
    otherPublicKey: Uint8Array,
    outputEncoding: string
  ): string {
    return encodeOutputString(this.computeSecretBytes(otherPublicKey), outputEncoding);
  }

  /**
   * Returns the EC Diffie-Hellman public key.
   */
  getPublicKey(encoding?: undefined, _format?: string): Uint8Array;
  getPublicKey(encoding: string, _format?: string): string;
  getPublicKey(encoding?: any, format?: any): any {
    throw new Error("Unreachable overload stub");
  }

  getPublicKey_bytes(
    _encoding?: undefined,
    _format?: string
  ): Uint8Array {
    return this.publicKeyBytes();
  }

  getPublicKey_string(
    encoding: string,
    _format?: string
  ): string {
    return encodeOutputBytes(this.publicKeyBytes(), encoding) as string;
  }

  /**
   * Returns the EC Diffie-Hellman private key.
   */
  getPrivateKey(encoding?: undefined): Uint8Array;
  getPrivateKey(encoding: string): string;
  getPrivateKey(encoding?: any): any {
    throw new Error("Unreachable overload stub");
  }

  getPrivateKey_bytes(_encoding?: undefined): Uint8Array {
    return this.privateKeyBytes();
  }

  getPrivateKey_string(encoding: string): string {
    return encodeOutputBytes(this.privateKeyBytes(), encoding) as string;
  }

  /**
   * Sets the EC Diffie-Hellman public key (deprecated, throws).
   */
  setPublicKey(_publicKey: string, _encoding?: string): void;
  setPublicKey(_publicKey: Uint8Array): void;
  setPublicKey(publicKey: any, encoding?: any): any {
    throw new Error("Unreachable overload stub");
  }

  setPublicKey_string(publicKey: string, encoding?: string): void {
    this.setPublicKeyBytes(
      decodeInputBytes(publicKey, encoding ?? "base64")
    );
  }

  setPublicKey_bytes(publicKey: Uint8Array): void {
    this.setPublicKeyBytes(publicKey);
  }

  /**
   * Sets the EC Diffie-Hellman private key.
   */
  setPrivateKey(privateKey: string, encoding?: string): void;
  setPrivateKey(privateKey: Uint8Array): void;
  setPrivateKey(privateKey: any, encoding?: any): any {
    throw new Error("Unreachable overload stub");
  }

  setPrivateKey_string(privateKey: string, encoding?: string): void {
    this.importPrivateKeyBytes(decodeInputBytes(privateKey, encoding ?? "base64"));
  }

  setPrivateKey_bytes(privateKey: Uint8Array): void {
    this.importPrivateKeyBytes(privateKey);
  }

  publicKeyBytes(): Uint8Array {
    if (this._publicKeyOverride !== null) {
      return this._publicKeyOverride;
    }

    return fromByteArray(this._ecdh.PublicKey.ExportSubjectPublicKeyInfo());
  }

  privateKeyBytes(): Uint8Array {
    return fromByteArray(this._ecdh.ExportECPrivateKey());
  }

  computeSecretBytes(otherPublicKey: Uint8Array): Uint8Array {
    const other = ECDiffieHellman.Create(curveFromName(this._curveName));
    other.ImportSubjectPublicKeyInfo(
      toReadOnlyByteSpan(otherPublicKey),
      0 as out<int>,
    );
    const secret = fromByteArray(this._ecdh.DeriveKeyMaterial(other.PublicKey));
    other.Dispose();
    return secret;
  }

  importPrivateKeyBytes(privateKey: Uint8Array): void {
    this._ecdh.Dispose();
    this._ecdh = ECDiffieHellman.Create(curveFromName(this._curveName));
    this._ecdh.ImportECPrivateKey(toReadOnlyByteSpan(privateKey), 0 as out<int>);
    this._publicKeyOverride = null;
  }

  setPublicKeyBytes(publicKey: Uint8Array): void {
    this._publicKeyOverride = publicKey;
  }
}

O<ECDH>().method(x => x.generateKeys_bytes).family(x => x.generateKeys);
O<ECDH>().method(x => x.generateKeys_string).family(x => x.generateKeys);
O<ECDH>().method(x => x.computeSecret_string).family(x => x.computeSecret);
O<ECDH>().method(x => x.computeSecret_bytes).family(x => x.computeSecret);
O<ECDH>().method(x => x.computeSecret_bytes_string).family(x => x.computeSecret);
O<ECDH>().method(x => x.getPublicKey_bytes).family(x => x.getPublicKey);
O<ECDH>().method(x => x.getPublicKey_string).family(x => x.getPublicKey);
O<ECDH>().method(x => x.getPrivateKey_bytes).family(x => x.getPrivateKey);
O<ECDH>().method(x => x.getPrivateKey_string).family(x => x.getPrivateKey);
O<ECDH>().method(x => x.setPublicKey_string).family(x => x.setPublicKey);
O<ECDH>().method(x => x.setPublicKey_bytes).family(x => x.setPublicKey);
O<ECDH>().method(x => x.setPrivateKey_string).family(x => x.setPrivateKey);
O<ECDH>().method(x => x.setPrivateKey_bytes).family(x => x.setPrivateKey);
