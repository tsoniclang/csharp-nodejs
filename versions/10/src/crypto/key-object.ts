/**
 * Node.js crypto KeyObject types.
 *
 */
import type { int, out } from "@tsonic/core/types.js";
import { overloads as O } from "@tsonic/core/lang.js";
import {
  DSA,
  ECDsa,
  RSA,
} from "@tsonic/dotnet/System.Security.Cryptography.js";
import {
  createDsaAlgorithm,
  createRsaAlgorithm,
  curveFromName,
  toReadOnlyByteSpan,
} from "./crypto-helpers.ts";
import { base64ToBytes } from "../buffer/buffer-encoding.ts";

export type NativeAsymmetricKey = RSA | DSA | ECDsa | null;
export type KeyExportValue = string | Uint8Array | NativeAsymmetricKey;
export type KeyExportOptions = object | null | undefined;

export const isRsaKey = (value: NativeAsymmetricKey): value is RSA =>
  value instanceof RSA;

export const isDsaKey = (value: NativeAsymmetricKey): value is DSA =>
  value instanceof DSA;

export const isEcDsaKey = (value: NativeAsymmetricKey): value is ECDsa =>
  value instanceof ECDsa;

/**
 * Represents a cryptographic key.
 */
export class KeyObject {
  constructor() {}

  /**
   * The type of the key: 'secret', 'public', or 'private'.
   */
  get type(): string {
    throw new Error("KeyObject.type must be implemented by a subclass");
  }

  /**
   * For asymmetric keys, this property returns the type of the key (e.g., 'rsa', 'ec', 'ed25519').
   * For secret keys, this property is undefined.
   */
  get asymmetricKeyType(): string | null {
    throw new Error(
      "KeyObject.asymmetricKeyType must be implemented by a subclass",
    );
  }

  /**
   * For secret keys, this property returns the size of the key in bytes.
   * For asymmetric keys, this property is undefined.
   */
  get symmetricKeySize(): int | null {
    throw new Error(
      "KeyObject.symmetricKeySize must be implemented by a subclass",
    );
  }

  ["export"](options?: KeyExportOptions): KeyExportValue {
    return this.exportCore(options);
  }

  exportCore(_options?: KeyExportOptions): KeyExportValue {
    throw new Error("KeyObject.exportCore must be implemented by a subclass");
  }
}

/**
 * Represents a secret (symmetric) key.
 */
export class SecretKeyObject extends KeyObject {
  _keyData: Uint8Array;

  constructor(keyData: Uint8Array) {
    super();
    this._keyData = keyData;
  }

  override get type(): string {
    return "secret";
  }

  override get asymmetricKeyType(): string | null {
    return null;
  }

  override get symmetricKeySize(): int | null {
    return this._keyData.length;
  }

  override ["export"](_options?: KeyExportOptions): Uint8Array {
    return this._keyData;
  }

  override exportCore(_options?: KeyExportOptions): Uint8Array {
    return this._keyData;
  }
}

/**
 * Represents a public key.
 */
export class PublicKeyObject extends KeyObject {
  _keyType: string;
  _keyData: NativeAsymmetricKey;
  _pem: string | null;

  constructor(
    keyData: NativeAsymmetricKey,
    keyType: string,
    pem: string | null = null
  ) {
    super();
    this._keyData = keyData;
    this._keyType = keyType;
    this._pem = pem;
  }

  override get type(): string {
    return "public";
  }

  override get asymmetricKeyType(): string {
    return this._keyType;
  }

  override get symmetricKeySize(): int | null {
    return null;
  }

  get nativeKeyData(): NativeAsymmetricKey {
    return this._keyData;
  }

  get pem(): string | null {
    return this._pem;
  }

  override exportCore(_options?: KeyExportOptions): KeyExportValue {
    if (this._pem !== null) {
      return this._pem;
    }

    return this._keyData;
  }

  exportFormatted(format: string, _type?: string): string {
    if (this._pem !== null && format.toLowerCase() === "pem") {
      return this._pem;
    }

    throw new Error("PublicKeyObject.exportFormatted is only supported for PEM keys");
  }
}

/**
 * Represents a private key.
 */
export class PrivateKeyObject extends KeyObject {
  _keyType: string;
  _keyData: NativeAsymmetricKey;
  _pem: string | null;
  _publicKeyData: NativeAsymmetricKey;
  _publicPem: string | null;

  constructor(
    keyData: NativeAsymmetricKey,
    keyType: string,
    pem: string | null = null,
    publicKeyData: NativeAsymmetricKey = null,
    publicPem: string | null = null,
  ) {
    super();
    this._keyData = keyData;
    this._keyType = keyType;
    this._pem = pem;
    this._publicKeyData = publicKeyData;
    this._publicPem = publicPem;
  }

  override get type(): string {
    return "private";
  }

  override get asymmetricKeyType(): string {
    return this._keyType;
  }

  override get symmetricKeySize(): int | null {
    return null;
  }

  get nativeKeyData(): NativeAsymmetricKey {
    return this._keyData;
  }

  get pem(): string | null {
    return this._pem;
  }

  get publicKeyData(): NativeAsymmetricKey {
    return this._publicKeyData;
  }

  get publicPem(): string | null {
    return this._publicPem;
  }

  override exportCore(_options?: KeyExportOptions): KeyExportValue {
    if (this._pem !== null) {
      return this._pem;
    }

    return this._keyData;
  }

  exportFormatted(
    format: string,
    _type?: string,
    _cipher?: string,
    _passphrase?: string
  ): string {
    if (this._pem !== null && format.toLowerCase() === "pem") {
      return this._pem;
    }

    throw new Error("PrivateKeyObject.exportFormatted is only supported for PEM keys");
  }
}

const stripPemBody = (value: string): string => {
  let body = "";
  let currentLine = "";
  for (let index = 0; index <= value.length; index += 1) {
    const char = index === value.length ? "\n" : value.charAt(index);
    if (char !== "\n" && char !== "\r") {
      currentLine += char;
      continue;
    }

    const line = currentLine.trim();
    currentLine = "";
    if (line.length === 0 || line.startsWith("-----")) {
      continue;
    }
    body += line;
  }
  return body;
};

const pemToBytes = (pem: string): Uint8Array => {
  return base64ToBytes(stripPemBody(pem));
};

const emptyPlaceholderPublicKey = (): PublicKeyObject => {
  return new PublicKeyObject(null, "rsa");
};

const emptyPlaceholderPrivateKey = (): PrivateKeyObject => {
  return new PrivateKeyObject(null, "rsa");
};

const tryImportRsaPublic = (
  bytes: Uint8Array,
  pem: string | null,
): PublicKeyObject | null => {
  try {
    const rsa = createRsaAlgorithm();
    rsa.ImportSubjectPublicKeyInfo(toReadOnlyByteSpan(bytes), 0 as out<int>);
    return new PublicKeyObject(rsa, "rsa", pem);
  } catch {
    return null;
  }
};

const tryImportDsaPublic = (
  bytes: Uint8Array,
  pem: string | null,
): PublicKeyObject | null => {
  try {
    const dsa = createDsaAlgorithm();
    dsa.ImportSubjectPublicKeyInfo(toReadOnlyByteSpan(bytes), 0 as out<int>);
    return new PublicKeyObject(dsa, "dsa", pem);
  } catch {
    return null;
  }
};

const tryImportEcPublic = (
  bytes: Uint8Array,
  pem: string | null,
): PublicKeyObject | null => {
  try {
    const ec = ECDsa.Create(curveFromName("secp256r1"));
    ec.ImportSubjectPublicKeyInfo(toReadOnlyByteSpan(bytes), 0 as out<int>);
    return new PublicKeyObject(ec, "ec", pem);
  } catch {
    return null;
  }
};

const tryImportRsaPrivate = (
  bytes: Uint8Array,
  pem: string | null,
): PrivateKeyObject | null => {
  try {
    const rsa = createRsaAlgorithm();
    rsa.ImportPkcs8PrivateKey(toReadOnlyByteSpan(bytes), 0 as out<int>);
    const publicRsa = createRsaAlgorithm();
    publicRsa.ImportParameters(rsa.ExportParameters(false));
    return new PrivateKeyObject(rsa, "rsa", pem, publicRsa, null);
  } catch {
    return null;
  }
};

const tryImportDsaPrivate = (
  bytes: Uint8Array,
  pem: string | null,
): PrivateKeyObject | null => {
  try {
    const dsa = createDsaAlgorithm();
    dsa.ImportPkcs8PrivateKey(toReadOnlyByteSpan(bytes), 0 as out<int>);
    const publicDsa = createDsaAlgorithm();
    publicDsa.ImportParameters(dsa.ExportParameters(false));
    return new PrivateKeyObject(dsa, "dsa", pem, publicDsa, null);
  } catch {
    return null;
  }
};

const tryImportEcPrivate = (
  bytes: Uint8Array,
  pem: string | null,
): PrivateKeyObject | null => {
  try {
    const ec = ECDsa.Create(curveFromName("secp256r1"));
    ec.ImportPkcs8PrivateKey(toReadOnlyByteSpan(bytes), 0 as out<int>);
    const publicEc = ECDsa.Create(curveFromName("secp256r1"));
    publicEc.ImportSubjectPublicKeyInfo(
      toReadOnlyByteSpan(ec.ExportSubjectPublicKeyInfo()),
      0 as out<int>,
    );
    return new PrivateKeyObject(ec, "ec", pem, publicEc, null);
  } catch {
    return null;
  }
};

export function importPublicKey(key: string): PublicKeyObject;
export function importPublicKey(key: Uint8Array): PublicKeyObject;
export function importPublicKey(_key: any): any {
  throw new Error("Unreachable overload stub");
}

export function importPublicKey_string(key: string): PublicKeyObject {
  return importPublicKeyBytes(pemToBytes(key), key);
}

export function importPublicKey_bytes(key: Uint8Array): PublicKeyObject {
  return importPublicKeyBytes(key, null);
}

const importPublicKeyBytes = (
  bytes: Uint8Array,
  pem: string | null,
): PublicKeyObject => {
  if (bytes.length === 0) {
    return emptyPlaceholderPublicKey();
  }

  const rsa = tryImportRsaPublic(bytes, pem);
  if (rsa !== null) {
    return rsa;
  }

  const dsa = tryImportDsaPublic(bytes, pem);
  if (dsa !== null) {
    return dsa;
  }

  const ec = tryImportEcPublic(bytes, pem);
  if (ec !== null) {
    return ec;
  }

  throw new Error("Unsupported public key format");
};

export function importPrivateKey(key: string): PrivateKeyObject;
export function importPrivateKey(key: Uint8Array): PrivateKeyObject;
export function importPrivateKey(_key: any): any {
  throw new Error("Unreachable overload stub");
}

export function importPrivateKey_string(key: string): PrivateKeyObject {
  return importPrivateKeyBytes(pemToBytes(key), key);
}

export function importPrivateKey_bytes(key: Uint8Array): PrivateKeyObject {
  return importPrivateKeyBytes(key, null);
}

const importPrivateKeyBytes = (
  bytes: Uint8Array,
  pem: string | null,
): PrivateKeyObject => {
  if (bytes.length === 0) {
    return emptyPlaceholderPrivateKey();
  }

  const rsa = tryImportRsaPrivate(bytes, pem);
  if (rsa !== null) {
    return rsa;
  }

  const dsa = tryImportDsaPrivate(bytes, pem);
  if (dsa !== null) {
    return dsa;
  }

  const ec = tryImportEcPrivate(bytes, pem);
  if (ec !== null) {
    return ec;
  }

  throw new Error("Unsupported private key format");
};

export const extractPublicKey = (
  key: PrivateKeyObject,
): PublicKeyObject => {
  const publicKeyData = key.publicKeyData;
  if (publicKeyData !== null && publicKeyData !== undefined) {
    return new PublicKeyObject(publicKeyData, key.asymmetricKeyType, key.publicPem);
  }

  const nativeKeyData = key.nativeKeyData;
  if (isRsaKey(nativeKeyData)) {
    const publicRsa = createRsaAlgorithm();
    publicRsa.ImportParameters(nativeKeyData.ExportParameters(false));
    return new PublicKeyObject(publicRsa, "rsa", key.publicPem);
  }

  if (isDsaKey(nativeKeyData)) {
    const publicDsa = createDsaAlgorithm();
    publicDsa.ImportParameters(nativeKeyData.ExportParameters(false));
    return new PublicKeyObject(publicDsa, "dsa", key.publicPem);
  }

  if (isEcDsaKey(nativeKeyData)) {
    const publicEc = ECDsa.Create(curveFromName("secp256r1"));
    publicEc.ImportSubjectPublicKeyInfo(
      toReadOnlyByteSpan(nativeKeyData.ExportSubjectPublicKeyInfo()),
      0 as out<int>,
    );
    return new PublicKeyObject(publicEc, "ec", key.publicPem);
  }

  return new PublicKeyObject(null, key.asymmetricKeyType ?? "rsa", key.publicPem);
};

export const coercePublicKeyObject = (
  key: string | Uint8Array | KeyObject,
): PublicKeyObject => {
  if (key instanceof Uint8Array) {
    return importPublicKey_bytes(key);
  }

  if (key instanceof PublicKeyObject) {
    return key;
  }

  if (key instanceof PrivateKeyObject) {
    return extractPublicKey(key);
  }

  if (key instanceof KeyObject) {
    if (key.type === "public") {
      return new PublicKeyObject(null, key.asymmetricKeyType ?? "rsa");
    }

    throw new Error("Key must be a private or public key");
  }

  return importPublicKey_string(key as string);
};

export const coercePrivateKeyObject = (
  key: string | Uint8Array | KeyObject,
): PrivateKeyObject => {
  if (key instanceof Uint8Array) {
    return importPrivateKey_bytes(key);
  }

  if (key instanceof PrivateKeyObject) {
    return key;
  }

  if (key instanceof PublicKeyObject) {
    throw new Error("Key must be a private key");
  }

  if (key instanceof KeyObject) {
    throw new Error("Key must be a private key");
  }

  return importPrivateKey_string(key as string);
};

O(importPublicKey_string).family(importPublicKey);
O(importPublicKey_bytes).family(importPublicKey);
O(importPrivateKey_string).family(importPrivateKey);
O(importPrivateKey_bytes).family(importPrivateKey);
