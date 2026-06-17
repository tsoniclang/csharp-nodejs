/**
 * SecureContext — holds TLS configuration (certificates, protocol versions).
 *
 *
 * The CLR implementation wraps System.Security.Cryptography.X509Certificates
 * and SslProtocols. In the native port these are substrate-dependent, so all
 * crypto/certificate operations are stubbed with TODO markers.
 */

import type { RuntimeValue } from "../runtime-value.ts";

export class SecureContext {
  /**
   * Internal context reference (for compatibility).
   */
  context: RuntimeValue = null;

  // -- internal state (substrate-dependent) --

  _certificateLoaded: boolean = false;
  _caLoaded: boolean = false;
  _minVersion: string | null = null;
  _maxVersion: string | null = null;

  constructor() {
    this.context = this;
  }

  /**
   * Whether a certificate has been loaded into this context.
   */
  get hasCertificate(): boolean {
    return this._certificateLoaded;
  }

  /**
   * Whether CA certificates have been loaded.
   */
  get hasCACertificates(): boolean {
    return this._caLoaded;
  }

  /**
   * The configured minimum TLS version, or null for default.
   */
  get minVersion(): string | null {
    return this._minVersion;
  }

  /**
   * The configured maximum TLS version, or null for default.
   */
  get maxVersion(): string | null {
    return this._maxVersion;
  }

  /**
   * Loads a certificate from PEM, PFX, or raw data.
   *
   * TODO: Implement certificate loading — requires substrate crypto primitives.
   */
  loadCertificate(
    cert: RuntimeValue,
    _key: RuntimeValue,
    _passphrase: string | null
  ): void {
    if (cert === null || cert === undefined) {
      return;
    }

    // TODO: parse PEM / PFX / raw bytes and store the certificate
    this._certificateLoaded = true;
  }

  /**
   * Loads CA certificates.
   *
   * TODO: Implement CA certificate loading — requires substrate crypto primitives.
   */
  loadCACertificates(ca: RuntimeValue): void {
    if (ca === null || ca === undefined) {
      return;
    }

    // TODO: parse PEM CA certs and add to trust store
    this._caLoaded = true;
  }

  /**
   * Configures the allowed SSL/TLS protocol version range.
   *
   * TODO: Map version strings to substrate protocol flags.
   */
  setProtocols(
    minVersion: string | null,
    maxVersion: string | null
  ): void {
    this._minVersion = minVersion;
    this._maxVersion = maxVersion;
  }
}
