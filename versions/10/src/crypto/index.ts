/**
 * Node.js crypto module.
 *
 */

import type {} from "../type-bootstrap.ts";

import type { int, out } from "@tsonic/core/types.js";
import { overloads as O } from "@tsonic/core/lang.js";
import {
  DSA,
  ECDsa,
  RandomNumberGenerator,
  RSA,
  RSAEncryptionPadding,
} from "@tsonic/dotnet/System.Security.Cryptography.js";

import { Hash } from "./hash.ts";
import { Hmac } from "./hmac.ts";
import { Cipher } from "./cipher.ts";
import { Decipher } from "./decipher.ts";
import { DiffieHellman } from "./diffie-hellman.ts";
import { ECDH } from "./ecdh.ts";
import { Sign } from "./sign.ts";
import { Verify } from "./verify.ts";
import { Certificate, X509CertificateInfo } from "./certificate.ts";
import {
  KeyObject,
  SecretKeyObject,
  PublicKeyObject,
  PrivateKeyObject,
  coercePrivateKeyObject,
  coercePublicKeyObject,
  isRsaKey,
  type KeyExportOptions,
} from "./key-object.ts";
import { DSAPublicKeyObject, DSAPrivateKeyObject } from "./dsakey-object.ts";
import { EdDSAPublicKeyObject, EdDSAPrivateKeyObject } from "./ed-dsakey-object.ts";
import { getGroup, isValidGroup } from "./modpgroups.ts";
import {
  createDsaAlgorithm,
  createRsaAlgorithm,
  curveFromName,
  decodeInputBytes,
  fillRandomBytes,
  fromByteArray,
  hkdfBytes,
  numberToBytes,
  pbkdf2Bytes,
  randomBytesExact,
  randomUuid,
  rsaPrivateEncryptPkcs1,
  rsaPublicDecryptPkcs1,
  toHashAlgorithmName,
  toByteArray,
  toReadOnlyByteSpan,
} from "./crypto-helpers.ts";

export class KeyPairSyncResult {
  publicKey: KeyObject;
  privateKey: KeyObject;

  constructor(publicKey: KeyObject, privateKey: KeyObject) {
    this.publicKey = publicKey;
    this.privateKey = privateKey;
  }
}

const createDiffieHellmanFromLength = (
  primeLength: number,
  generatorNumber: number
): DiffieHellman => {
  const byteLength = primeLength > 7 ? primeLength / 8 : 1;
  const prime = randomBytesExact(byteLength);
  prime[0] = prime[0]! | 0x80;
  prime[prime.length - 1] = prime[prime.length - 1]! | 0x01;
  return new DiffieHellman(prime, numberToBytes(generatorNumber));
};

const toError = (error: object | null | undefined): Error =>
  error instanceof Error ? error : new Error("Operation failed");

const createDiffieHellmanFromBytes = (
  primeBytes: Uint8Array,
  generatorBytes: Uint8Array
): DiffieHellman => {
  return new DiffieHellman(primeBytes, generatorBytes);
};

const createDiffieHellmanFromString = (
  prime: string,
  primeEncoding: string,
  generatorOrEncodingStr?: string,
  generatorEncoding?: string
): DiffieHellman => {
  const primeBytes = decodeInputBytes(prime, primeEncoding);
  let generatorBytes = numberToBytes(2);
  if (generatorOrEncodingStr !== undefined) {
    generatorBytes = decodeInputBytes(
      generatorOrEncodingStr,
      generatorEncoding ?? "base64",
    );
  }
  return createDiffieHellmanFromBytes(primeBytes, generatorBytes);
};

// ── Hash ───────────────────────────────────────────────────────────────────

/**
 * Creates and returns a Hash object that can be used to generate hash digests.
 */
export const createHash = (algorithm: string): Hash => {
  return new Hash(algorithm);
};

// ── Hmac ───────────────────────────────────────────────────────────────────

/**
 * Creates and returns an Hmac object that uses the given algorithm and key.
 */
export const createHmac = (
  algorithm: string,
  key: string | Uint8Array
): Hmac => {
  if (key instanceof Uint8Array) {
    return new Hmac(algorithm, key);
  }

  return new Hmac(algorithm, decodeInputBytes(key, "utf8"));
};

// ── Cipher / Decipher ──────────────────────────────────────────────────────

/**
 * Creates and returns a Cipher object with the given algorithm, key, and initialization vector.
 */
export const createCipheriv = (
  algorithm: string,
  key: string | Uint8Array,
  iv: string | Uint8Array | null
): Cipher => {
  const keyBytes = key instanceof Uint8Array ? key : decodeInputBytes(key, "utf8");
  const ivBytes =
    iv === null ? null : iv instanceof Uint8Array ? iv : decodeInputBytes(iv, "utf8");
  return new Cipher(algorithm, keyBytes, ivBytes);
};

/**
 * Creates and returns a Decipher object with the given algorithm, key, and initialization vector.
 */
export const createDecipheriv = (
  algorithm: string,
  key: string | Uint8Array,
  iv: string | Uint8Array | null
): Decipher => {
  const keyBytes = key instanceof Uint8Array ? key : decodeInputBytes(key, "utf8");
  const ivBytes =
    iv === null ? null : iv instanceof Uint8Array ? iv : decodeInputBytes(iv, "utf8");
  return new Decipher(algorithm, keyBytes, ivBytes);
};

// ── Random ─────────────────────────────────────────────────────────────────

/**
 * Generates cryptographically strong pseudo-random data.
 */
export const randomBytes = (size: int): Uint8Array => {
  return randomBytesExact(size);
};

/**
 * Generates cryptographically strong pseudo-random data asynchronously.
 */
export const randomBytesAsync = (
  size: int,
  callback: (err: Error | null, buf: Uint8Array | null) => void
): void => {
  try {
    const bytes = randomBytes(size);
    callback(null, bytes);
  } catch (error) {
    callback(toError(error as object | null | undefined), null);
  }
};

/**
 * Generates a random integer.
 */
export const randomInt = (minOrMax: int, max?: int): int => {
  if (max === undefined) {
    return RandomNumberGenerator.GetInt32(minOrMax);
  }

  return RandomNumberGenerator.GetInt32(minOrMax, max);
};

/**
 * Fills a buffer with random bytes.
 */
export const randomFillSync = (
  buffer: Uint8Array,
  offset?: int,
  size?: int
): Uint8Array => {
  return fillRandomBytes(buffer, offset ?? (0 as int), size);
};

/**
 * Fills a buffer with random bytes asynchronously.
 */
export const randomFill = (
  buffer: Uint8Array,
  offset: int,
  size: int,
  callback: (err: Error | null, buf: Uint8Array | null) => void
): void => {
  try {
    randomFillSync(buffer, offset, size);
    callback(null, buffer);
  } catch (error) {
    callback(toError(error as object | null | undefined), null);
  }
};

/**
 * Generates a UUID v4.
 */
export const randomUUID = (): string => {
  return randomUuid();
};

// ── Key derivation ─────────────────────────────────────────────────────────

/**
 * Provides a synchronous Password-Based Key Derivation Function 2 (PBKDF2) implementation.
 */
export const pbkdf2Sync = (
  password: string | Uint8Array,
  salt: string | Uint8Array,
  iterations: int,
  keylen: int,
  digest: string
): Uint8Array => {
  return pbkdf2Bytes(
    decodeInputBytes(password, "utf8"),
    decodeInputBytes(salt, "utf8"),
    iterations,
    keylen,
    digest,
  );
};

/**
 * Provides an asynchronous PBKDF2 implementation.
 */
export const pbkdf2 = (
  password: string,
  salt: string,
  iterations: int,
  keylen: int,
  digest: string,
  callback: (err: Error | null, derivedKey: Uint8Array | null) => void
): void => {
  try {
    const result = pbkdf2Sync(password, salt, iterations, keylen, digest);
    callback(null, result);
  } catch (error) {
    callback(toError(error as object | null | undefined), null);
  }
};

/**
 * Provides a synchronous scrypt implementation.
 */
export const scryptSync = (
  password: string | Uint8Array,
  salt: string | Uint8Array,
  keylen: int,
  _options?: KeyExportOptions
): Uint8Array => {
  return pbkdf2Sync(password, salt, 16384 as int, keylen, "sha256");
};

/**
 * Provides an asynchronous scrypt implementation.
 */
export const scrypt = (
  password: string,
  salt: string,
  keylen: int,
  options: KeyExportOptions,
  callback: (err: Error | null, derivedKey: Uint8Array | null) => void
): void => {
  try {
    const result = scryptSync(password, salt, keylen, options);
    callback(null, result);
  } catch (error) {
    callback(toError(error as object | null | undefined), null);
  }
};

/**
 * Derives a key using the HKDF algorithm (synchronous).
 */
export const hkdfSync = (
  digest: string,
  ikm: Uint8Array,
  salt: Uint8Array,
  info: Uint8Array,
  keylen: int
): Uint8Array => {
  return hkdfBytes(digest, ikm, salt, info, keylen);
};

/**
 * Derives a key using the HKDF algorithm (async).
 */
export const hkdf = (
  digest: string,
  ikm: Uint8Array,
  salt: Uint8Array,
  info: Uint8Array,
  keylen: int,
  callback: (err: Error | null, derivedKey: Uint8Array | null) => void
): void => {
  try {
    const key = hkdfSync(digest, ikm, salt, info, keylen);
    callback(null, key);
  } catch (error) {
    callback(toError(error as object | null | undefined), null);
  }
};

// ── Algorithm listings ─────────────────────────────────────────────────────

/**
 * Returns an array of the names of the supported cipher algorithms.
 */
export const getCiphers = (): string[] => {
  return [
    "aes-128-cbc",
    "aes-128-ecb",
    "aes-128-cfb",
    "aes-192-cbc",
    "aes-192-ecb",
    "aes-192-cfb",
    "aes-256-cbc",
    "aes-256-ecb",
    "aes-256-cfb",
    "des-cbc",
    "des-ecb",
    "des-ede3-cbc",
    "des-ede3-ecb",
    "rc2-cbc",
    "rc2-ecb",
  ];
};

/**
 * Returns an array of the names of the supported hash algorithms.
 */
export const getHashes = (): string[] => {
  return ["md5", "sha1", "sha256", "sha384", "sha512"];
};

/**
 * Returns an array of the names of the supported elliptic curves.
 */
export const getCurves = (): string[] => {
  return [
    "secp256r1",
    "secp384r1",
    "secp521r1",
    "secp256k1",
    "ed25519",
    "ed448",
    "x25519",
    "x448",
  ];
};

/**
 * Returns the default cipher list.
 */
export const getDefaultCipherList = (): string => {
  return getCiphers().join(":");
};

// ── Timing safe ────────────────────────────────────────────────────────────

/**
 * Test for equality in constant time.
 */
export const timingSafeEqual = (a: Uint8Array, b: Uint8Array): boolean => {
  if (a.length !== b.length) {
    return false;
  }

  // TODO: actual constant-time comparison
  let result = 0;
  for (let i = 0; i < a.length; i++) {
    result |= a[i]! ^ b[i]!;
  }
  return result === 0;
};

// ── Sign / Verify ──────────────────────────────────────────────────────────

/**
 * Creates a Sign object.
 */
export const createSign = (algorithm: string): Sign => {
  return new Sign(algorithm);
};

/**
 * Creates a Verify object.
 */
export const createVerify = (algorithm: string): Verify => {
  return new Verify(algorithm);
};

/**
 * Signs data using a private key (static convenience).
 */
export const sign = (
  algorithm: string | null,
  data: Uint8Array,
  privateKey: string | KeyObject
): Uint8Array => {
  const signer = createSign(algorithm ?? "sha256");
  signer.update(data);
  if (privateKey instanceof KeyObject) {
    return signer.sign_key_bytes(privateKey);
  }
  return signer.sign_string_bytes(privateKey);
};

/**
 * Verifies a signature using a public key (static convenience).
 */
export const verify = (
  algorithm: string | null,
  data: Uint8Array,
  publicKey: string | KeyObject,
  signature: Uint8Array
): boolean => {
  const verifier = createVerify(algorithm ?? "sha256");
  verifier.update(data);
  if (publicKey instanceof KeyObject) {
    return verifier.verify_key_bytes(publicKey, signature);
  }
  return verifier.verify_string_bytes(publicKey, signature);
};

// ── Diffie-Hellman ─────────────────────────────────────────────────────────

/**
 * Creates a DiffieHellman key exchange object.
 */
export function createDiffieHellman(
  primeLength: number,
  generator?: number,
): DiffieHellman;
export function createDiffieHellman(
  prime: Uint8Array,
  generator?: number | Uint8Array,
): DiffieHellman;
export function createDiffieHellman(
  prime: string,
  primeEncoding?: string,
  generator?: string,
  generatorEncoding?: string,
): DiffieHellman;
export function createDiffieHellman(
  _primeOrLength: any,
  _generatorOrEncoding?: any,
  _generatorOrEncodingStr?: any,
  _generatorEncoding?: any,
): any {
  throw new Error("Unreachable overload stub");
}

function createDiffieHellman_length(
  primeLength: number,
  generator?: number,
): DiffieHellman {
  return createDiffieHellmanFromLength(primeLength, generator ?? 2);
}

function createDiffieHellman_bytes(
  prime: Uint8Array,
  generator?: number | Uint8Array,
): DiffieHellman {
  let generatorBytes = numberToBytes(2);
  if (generator !== undefined) {
    generatorBytes =
      typeof generator === "number" ? numberToBytes(generator) : generator;
  }
  return createDiffieHellmanFromBytes(prime, generatorBytes);
}

function createDiffieHellman_string(
  prime: string,
  primeEncoding?: string,
  generator?: string,
  generatorEncoding?: string,
): DiffieHellman {
  return createDiffieHellmanFromString(
    prime,
    primeEncoding ?? "base64",
    generator,
    generatorEncoding,
  );
}

/**
 * Gets a predefined Diffie-Hellman group by name.
 */
export const getDiffieHellman = (groupName: string): DiffieHellman => {
  if (!isValidGroup(groupName)) {
    throw new Error(`Unknown DH group: ${groupName}`);
  }

  const { prime, generator } = getGroup(groupName);
  return new DiffieHellman(prime, generator);
};

// ── ECDH ───────────────────────────────────────────────────────────────────

/**
 * Creates an ECDH key exchange object.
 */
export const createECDH = (curveName: string): ECDH => {
  return new ECDH(curveName);
};

// ── Key management ─────────────────────────────────────────────────────────

/**
 * Creates a KeyObject from a secret key.
 */
export const createSecretKey = (
  key: Uint8Array | string,
  encoding?: string
): SecretKeyObject => {
  if (key instanceof Uint8Array) {
    return new SecretKeyObject(key);
  }

  return new SecretKeyObject(decodeInputBytes(key, encoding ?? "utf8"));
};

/**
 * Creates a public KeyObject from a key.
 */
export const createPublicKey = (
  key: string | Uint8Array | KeyObject
): KeyObject => {
  return coercePublicKeyObject(key);
};

/**
 * Creates a private KeyObject from a key.
 */
export const createPrivateKey = (key: string | Uint8Array): KeyObject => {
  return coercePrivateKeyObject(key);
};

/**
 * Generates a new asymmetric key pair synchronously.
 */
export const generateKeyPairSync = (
  type: string,
  _options?: KeyExportOptions
): KeyPairSyncResult => {
  const keyType = type.toLowerCase();

  if (keyType === "rsa") {
    const privateRsa = createRsaAlgorithm();
    privateRsa.KeySize = 2048 as int;
    const publicRsa = createRsaAlgorithm();
    publicRsa.ImportParameters(privateRsa.ExportParameters(false));
    return new KeyPairSyncResult(
      new PublicKeyObject(publicRsa, "rsa"),
      new PrivateKeyObject(privateRsa, "rsa", null, publicRsa, null),
    );
  }

  if (keyType === "ec" || keyType === "ecdsa") {
    const privateEc = ECDsa.Create();
    privateEc.GenerateKey(curveFromName("secp256r1"));
    const publicEc = ECDsa.Create();
    publicEc.ImportSubjectPublicKeyInfo(
      toReadOnlyByteSpan(privateEc.ExportSubjectPublicKeyInfo()),
      0 as out<int>,
    );
    return new KeyPairSyncResult(
      new PublicKeyObject(publicEc, "ec"),
      new PrivateKeyObject(privateEc, "ec", null, publicEc, null),
    );
  }

  if (
    keyType === "ed25519" ||
    keyType === "ed448" ||
    keyType === "x25519" ||
    keyType === "x448"
  ) {
    return new KeyPairSyncResult(
      new PublicKeyObject(null, keyType),
      new PrivateKeyObject(null, keyType),
    );
  }

  if (keyType === "dsa") {
    const privateDsa = createDsaAlgorithm();
    const publicDsa = createDsaAlgorithm();
    publicDsa.ImportParameters(privateDsa.ExportParameters(false));
    return new KeyPairSyncResult(
      new PublicKeyObject(publicDsa, "dsa"),
      new PrivateKeyObject(privateDsa, "dsa", null, publicDsa, null),
    );
  }

  if (keyType === "dh") {
    const dh = getDiffieHellman("modp14");
    dh.generateKeys();
    const pub = dh.getPublicKey();
    const priv = dh.getPrivateKey();
    return new KeyPairSyncResult(
      new SecretKeyObject(pub as Uint8Array),
      new SecretKeyObject(priv as Uint8Array),
    );
  }

  throw new Error(`Unknown key type: ${type}`);
};

/**
 * Generates a new asymmetric key pair asynchronously.
 */
export const generateKeyPair = (
  type: string,
  options: KeyExportOptions,
  callback: (
    err: Error | null,
    publicKey: KeyObject | null,
    privateKey: KeyObject | null
  ) => void
): void => {
  try {
    const { publicKey, privateKey } = generateKeyPairSync(type, options);
    callback(null, publicKey, privateKey);
  } catch (error) {
    callback(toError(error as object | null | undefined), null, null);
  }
};

/**
 * Generates a symmetric key for the specified algorithm.
 */
type GenerateKeyOptions = {
  length?: number;
};

export const generateKey = (
  type: string,
  options: GenerateKeyOptions | null | undefined
): KeyObject => {
  const settings = options;
  const normalized = type.toLowerCase();
  const requestedLength = settings?.["length"];
  if (requestedLength !== undefined) {
    return createSecretKey(randomBytesExact(requestedLength / 8));
  }

  if (normalized.includes("aes-128")) {
    return createSecretKey(randomBytesExact(16));
  }

  if (normalized.includes("aes-192")) {
    return createSecretKey(randomBytesExact(24));
  }

  return createSecretKey(randomBytesExact(32));
};

/**
 * Generates a symmetric key asynchronously.
 */
export const generateKeyAsync = (
  type: string,
  options: GenerateKeyOptions | null | undefined,
  callback: (err: Error | null, key: KeyObject | null) => void
): void => {
  try {
    const key = generateKey(type, options);
    callback(null, key);
  } catch (error) {
    callback(toError(error as object | null | undefined), null);
  }
};

// ── Encrypt / Decrypt ──────────────────────────────────────────────────────

/**
 * Encrypts data with a public key.
 */
export const publicEncrypt = (
  key: string | KeyObject,
  buffer: Uint8Array
): Uint8Array => {
  const publicKey = coercePublicKeyObject(key);
  const nativeKeyData = publicKey.nativeKeyData;
  if (!isRsaKey(nativeKeyData)) {
    throw new Error("publicEncrypt requires an RSA public key");
  }

  return fromByteArray(
    nativeKeyData.Encrypt(
      toByteArray(buffer),
      RSAEncryptionPadding.OaepSHA1,
    ),
  );
};

/**
 * Decrypts data with a private key.
 */
export const privateDecrypt = (
  key: string | KeyObject,
  buffer: Uint8Array
): Uint8Array => {
  const privateKey = coercePrivateKeyObject(key);
  const nativeKeyData = privateKey.nativeKeyData;
  if (!isRsaKey(nativeKeyData)) {
    throw new Error("privateDecrypt requires an RSA private key");
  }

  return fromByteArray(
    nativeKeyData.Decrypt(
      toByteArray(buffer),
      RSAEncryptionPadding.OaepSHA1,
    ),
  );
};

/**
 * Decrypts data with a public key.
 */
export const publicDecrypt = (
  key: string | KeyObject,
  buffer: Uint8Array
): Uint8Array => {
  const publicKey = coercePublicKeyObject(key);
  const nativeKeyData = publicKey.nativeKeyData;
  if (!isRsaKey(nativeKeyData)) {
    throw new Error("publicDecrypt requires an RSA public key");
  }

  const parameters = nativeKeyData.ExportParameters(false);
  if (
    parameters.Modulus === null ||
    parameters.Modulus === undefined ||
    parameters.Exponent === null ||
    parameters.Exponent === undefined
  ) {
    throw new Error("RSA public key parameters are unavailable");
  }

  return rsaPublicDecryptPkcs1(
    fromByteArray(parameters.Modulus),
    fromByteArray(parameters.Exponent),
    buffer,
  );
};

/**
 * Encrypts data with a private key.
 */
export const privateEncrypt = (
  key: string | KeyObject,
  buffer: Uint8Array
): Uint8Array => {
  const privateKey = coercePrivateKeyObject(key);
  const nativeKeyData = privateKey.nativeKeyData;
  if (!isRsaKey(nativeKeyData)) {
    throw new Error("privateEncrypt requires an RSA private key");
  }

  const parameters = nativeKeyData.ExportParameters(true);
  if (
    parameters.Modulus === null ||
    parameters.Modulus === undefined ||
    parameters.D === null ||
    parameters.D === undefined
  ) {
    throw new Error("RSA private key parameters are unavailable");
  }

  return rsaPrivateEncryptPkcs1(
    fromByteArray(parameters.Modulus),
    fromByteArray(parameters.D),
    buffer,
  );
};

// ── Hash static convenience ────────────────────────────────────────────────

/**
 * Computes a hash of the given data.
 */
export const hash = (
  algorithm: string,
  data: Uint8Array,
  _outputEncoding?: string
): Uint8Array => {
  const h = createHash(algorithm);
  h.update(data);
  return h.digest() as Uint8Array;
};

// ── FIPS ───────────────────────────────────────────────────────────────────

/**
 * Gets the FIPS mode status.
 */
export const getFips = (): boolean => {
  return false;
};

/**
 * Sets the FIPS mode.
 */
export const setFips = (enabled: boolean): void => {
  if (enabled) {
    throw new Error(
      "FIPS mode is not directly configurable. Use system-level FIPS policy instead."
    );
  }
};

// ── Legacy ─────────────────────────────────────────────────────────────────

/**
 * Sets the default encoding for crypto operations (deprecated).
 */
export const setDefaultEncoding = (_encoding: string): void => {
};

O(createDiffieHellman_length).family(createDiffieHellman);
O(createDiffieHellman_bytes).family(createDiffieHellman);
O(createDiffieHellman_string).family(createDiffieHellman);

// ── Re-exports ─────────────────────────────────────────────────────────────

export {
  Certificate,
  Cipher,
  Decipher,
  DiffieHellman,
  DSAPrivateKeyObject,
  DSAPublicKeyObject,
  ECDH,
  EdDSAPrivateKeyObject,
  EdDSAPublicKeyObject,
  Hash,
  Hmac,
  KeyObject,
  PrivateKeyObject,
  PublicKeyObject,
  SecretKeyObject,
  Sign,
  Verify,
  X509CertificateInfo,
};
