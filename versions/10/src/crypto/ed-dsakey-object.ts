/**
 * Node.js crypto EdDSA key objects.
 *
 */
import { KeyObject } from "./key-object.ts";
import type { int } from "@tsonic/core/types.js";
import type { KeyExportOptions, KeyExportValue } from "./key-object.ts";

/**
 * Represents an EdDSA public key.
 */
export class EdDSAPublicKeyObject extends KeyObject {
  _publicKeyData: KeyExportValue;
  _keyType: string;

  constructor(publicKeyData: KeyExportValue, keyType: string) {
    super();
    this._publicKeyData = publicKeyData;
    this._keyType = keyType;
  }

  override get type(): string {
    return "public";
  }

  override get asymmetricKeyType(): string | null {
    return this._keyType;
  }

  override get symmetricKeySize(): int | null {
    return null;
  }

  override exportCore(_options?: KeyExportOptions): KeyExportValue {
    return this._publicKeyData;
  }
}

/**
 * Represents an EdDSA private key.
 */
export class EdDSAPrivateKeyObject extends KeyObject {
  _privateKeyData: KeyExportValue;
  _keyType: string;

  constructor(privateKeyData: KeyExportValue, keyType: string) {
    super();
    this._privateKeyData = privateKeyData;
    this._keyType = keyType;
  }

  override get type(): string {
    return "private";
  }

  override get asymmetricKeyType(): string | null {
    return this._keyType;
  }

  override get symmetricKeySize(): int | null {
    return null;
  }

  override exportCore(_options?: KeyExportOptions): KeyExportValue {
    return this._privateKeyData;
  }
}
