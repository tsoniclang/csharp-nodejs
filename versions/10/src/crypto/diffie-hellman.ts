import { overloads as O } from "@tsonic/core/lang.js";
import {
  decodeInputBytes,
  encodeOutputBytes,
  encodeOutputString,
  modPowBytes,
  numberToBytes,
  randomBytesExact,
  randomUnsignedLessThan,
} from "./crypto-helpers.ts";

function toDiffieHellmanPublicKeyBytes(
  otherPublicKey: string,
  inputEncoding?: string,
): Uint8Array {
  return decodeInputBytes(otherPublicKey, inputEncoding ?? "base64");
}

const encodeDiffieHellmanSecret = (
  secret: Uint8Array,
  outputEncoding?: string,
): string => {
  return encodeOutputString(secret, outputEncoding ?? "base64");
};

const resolveDiffieHellmanPrimeByteLength = (primeLength: number): number => {
  return primeLength > 7 ? primeLength / 8 : 1;
};

const resolveDiffieHellmanGeneratorBytes = (
  generatorOrValue?: Uint8Array,
): Uint8Array => {
  if (generatorOrValue === undefined) {
    return numberToBytes(2);
  }

  return generatorOrValue;
};

/**
 * Node.js crypto DiffieHellman class.
 *
 */

/**
 * The DiffieHellman class is a utility for creating Diffie-Hellman key exchanges.
 */
export class DiffieHellman {
  _prime: Uint8Array;
  _generator: Uint8Array;
  _privateKey: Uint8Array | null = null;
  _publicKey: Uint8Array | null = null;

  constructor(
    prime: Uint8Array,
    generator?: Uint8Array
  ) {
    this._prime = prime;
    this._generator = resolveDiffieHellmanGeneratorBytes(generator);
  }

  /**
   * Generates private and public Diffie-Hellman key values.
   */
  generateKeys(encoding?: undefined): Uint8Array;
  generateKeys(encoding: string): string;
  generateKeys(encoding?: any): any {
    throw new Error("Unreachable overload stub");
  }

  generateKeys_bytes(_encoding?: undefined): Uint8Array {
    return this.generateKeysBytes();
  }

  generateKeys_string(encoding: string): string {
    return encodeOutputBytes(this.generateKeysBytes(), encoding) as string;
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
    return encodeDiffieHellmanSecret(
      this.computeSecretBytes(
        toDiffieHellmanPublicKeyBytes(otherPublicKey, inputEncoding),
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
    return encodeOutputString(
      this.computeSecretBytes(otherPublicKey),
      outputEncoding,
    );
  }

  /**
   * Returns the Diffie-Hellman prime.
   */
  getPrime(encoding?: undefined): Uint8Array;
  getPrime(encoding: string): string;
  getPrime(encoding?: any): any {
    throw new Error("Unreachable overload stub");
  }

  getPrime_bytes(_encoding?: undefined): Uint8Array {
    return this._prime;
  }

  getPrime_string(encoding: string): string {
    return encodeOutputBytes(this._prime, encoding) as string;
  }

  /**
   * Returns the Diffie-Hellman generator.
   */
  getGenerator(encoding?: undefined): Uint8Array;
  getGenerator(encoding: string): string;
  getGenerator(encoding?: any): any {
    throw new Error("Unreachable overload stub");
  }

  getGenerator_bytes(_encoding?: undefined): Uint8Array {
    return this._generator;
  }

  getGenerator_string(encoding: string): string {
    return encodeOutputBytes(this._generator, encoding) as string;
  }

  /**
   * Returns the Diffie-Hellman public key.
   */
  getPublicKey(encoding?: undefined): Uint8Array;
  getPublicKey(encoding: string): string;
  getPublicKey(encoding?: any): any {
    throw new Error("Unreachable overload stub");
  }

  getPublicKey_bytes(_encoding?: undefined): Uint8Array {
    return this.requirePublicKey();
  }

  getPublicKey_string(encoding: string): string {
    return encodeOutputBytes(this.requirePublicKey(), encoding) as string;
  }

  /**
   * Returns the Diffie-Hellman private key.
   */
  getPrivateKey(encoding?: undefined): Uint8Array;
  getPrivateKey(encoding: string): string;
  getPrivateKey(encoding?: any): any {
    throw new Error("Unreachable overload stub");
  }

  getPrivateKey_bytes(_encoding?: undefined): Uint8Array {
    return this.requirePrivateKey();
  }

  getPrivateKey_string(encoding: string): string {
    return encodeOutputBytes(this.requirePrivateKey(), encoding) as string;
  }

  /**
   * Sets the Diffie-Hellman public key.
   */
  setPublicKey(publicKey: string, encoding?: string): void;
  setPublicKey(publicKey: Uint8Array): void;
  setPublicKey(publicKey: any, encoding?: any): any {
    throw new Error("Unreachable overload stub");
  }

  setPublicKey_string(publicKey: string, encoding?: string): void {
    this._publicKey = decodeInputBytes(publicKey, encoding ?? "base64");
  }

  setPublicKey_bytes(publicKey: Uint8Array): void {
    this._publicKey = publicKey;
  }

  /**
   * Sets the Diffie-Hellman private key.
   */
  setPrivateKey(privateKey: string, encoding?: string): void;
  setPrivateKey(privateKey: Uint8Array): void;
  setPrivateKey(privateKey: any, encoding?: any): any {
    throw new Error("Unreachable overload stub");
  }

  setPrivateKey_string(privateKey: string, encoding?: string): void {
    this.setPrivateKeyBytes(decodeInputBytes(privateKey, encoding ?? "base64"));
  }

  setPrivateKey_bytes(privateKey: Uint8Array): void {
    this.setPrivateKeyBytes(privateKey);
  }

  generateKeysBytes(): Uint8Array {
    this._privateKey = randomUnsignedLessThan(this._prime);
    this._publicKey = modPowBytes(
      this._generator,
      this._privateKey,
      this._prime,
      this._prime.length,
    );
    return this._publicKey;
  }

  computeSecretBytes(otherPublicKey: Uint8Array): Uint8Array {
    if (this._privateKey === null) {
      throw new Error("Must call generateKeys() first");
    }

    return modPowBytes(
      otherPublicKey,
      this._privateKey,
      this._prime,
      this._prime.length,
    );
  }

  requirePublicKey(): Uint8Array {
    if (this._publicKey === null) {
      throw new Error("Must call generateKeys() first");
    }

    return this._publicKey;
  }

  requirePrivateKey(): Uint8Array {
    if (this._privateKey === null) {
      throw new Error("Must call generateKeys() first");
    }

    return this._privateKey;
  }

  setPrivateKeyBytes(privateKey: Uint8Array): void {
    this._privateKey = privateKey;
    this._publicKey = modPowBytes(
      this._generator,
      this._privateKey,
      this._prime,
      this._prime.length,
    );
  }

  /**
   * Returns the DH validation error code.
   */
  getVerifyError(): number {
    return 0;
  }
}

O<DiffieHellman>().method(x => x.generateKeys_bytes).family(x => x.generateKeys);
O<DiffieHellman>().method(x => x.generateKeys_string).family(x => x.generateKeys);
O<DiffieHellman>().method(x => x.computeSecret_string).family(x => x.computeSecret);
O<DiffieHellman>().method(x => x.computeSecret_bytes).family(x => x.computeSecret);
O<DiffieHellman>().method(x => x.computeSecret_bytes_string).family(x => x.computeSecret);
O<DiffieHellman>().method(x => x.getPrime_bytes).family(x => x.getPrime);
O<DiffieHellman>().method(x => x.getPrime_string).family(x => x.getPrime);
O<DiffieHellman>().method(x => x.getGenerator_bytes).family(x => x.getGenerator);
O<DiffieHellman>().method(x => x.getGenerator_string).family(x => x.getGenerator);
O<DiffieHellman>().method(x => x.getPublicKey_bytes).family(x => x.getPublicKey);
O<DiffieHellman>().method(x => x.getPublicKey_string).family(x => x.getPublicKey);
O<DiffieHellman>().method(x => x.getPrivateKey_bytes).family(x => x.getPrivateKey);
O<DiffieHellman>().method(x => x.getPrivateKey_string).family(x => x.getPrivateKey);
O<DiffieHellman>().method(x => x.setPublicKey_string).family(x => x.setPublicKey);
O<DiffieHellman>().method(x => x.setPublicKey_bytes).family(x => x.setPublicKey);
O<DiffieHellman>().method(x => x.setPrivateKey_string).family(x => x.setPrivateKey);
O<DiffieHellman>().method(x => x.setPrivateKey_bytes).family(x => x.setPrivateKey);
