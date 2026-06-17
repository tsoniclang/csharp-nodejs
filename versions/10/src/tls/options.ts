import type { RuntimeValue } from "../runtime-value.ts";

/**
 * Certificate distinguished-name fields.
 */
export class TLSCertificateInfo {
  C: string = "";
  ST: string = "";
  L: string = "";
  O: string = "";
  OU: string = "";
  CN: string = "";
}

/**
 * Peer certificate information returned by getPeerCertificate().
 */
export class PeerCertificate {
  ca: boolean = false;
  raw: Uint8Array | null = null;
  subject: TLSCertificateInfo = new TLSCertificateInfo();
  issuer: TLSCertificateInfo = new TLSCertificateInfo();
  valid_from: string = "";
  valid_to: string = "";
  serialNumber: string = "";
  fingerprint: string = "";
  fingerprint256: string = "";
  fingerprint512: string = "";
  ext_key_usage: string[] | null = null;
  subjectaltname: string | null = null;
}

/**
 * Detailed peer certificate with issuer chain.
 */
export class DetailedPeerCertificate extends PeerCertificate {
  issuerCertificate: DetailedPeerCertificate | null = null;
}

/**
 * Cipher name and protocol information.
 */
export class CipherNameAndProtocol {
  name: string = "";
  version: string = "";
  standardName: string = "";
}

/**
 * Ephemeral key exchange information.
 */
export class EphemeralKeyInfo {
  type: string = "";
  name: string | null = null;
  size: number = 0;
}

/**
 * Secure context options for TLS configuration.
 */
export class SecureContextOptions {
  ca: RuntimeValue = null;
  cert: RuntimeValue = null;
  ciphers: string | null = null;
  key: RuntimeValue = null;
  passphrase: string | null = null;
  pfx: RuntimeValue = null;
  maxVersion: string | null = null;
  minVersion: string | null = null;
}

/**
 * Common connection options for TLS.
 */
export class CommonConnectionOptions {
  secureContext: SecureContext | null = null;
  enableTrace: boolean | null = null;
  requestCert: boolean | null = null;
  rejectUnauthorized: boolean | null = null;
  ALPNProtocols: string[] | null = null;
}

// Forward-declare SecureContext to avoid circular import at the type level.
// The actual class is in secure-context.ts.
import type { SecureContext } from "./secure-context.ts";

/**
 * TLS socket options.
 */
export class TLSSocketOptions extends CommonConnectionOptions {
  isServer: boolean | null = null;
  server: RuntimeValue = null;
  servername: string | null = null;
  ca: RuntimeValue = null;
  cert: RuntimeValue = null;
  key: RuntimeValue = null;
  passphrase: string | null = null;
}

/**
 * Connection options for TLS client.
 */
export class ConnectionOptions extends CommonConnectionOptions {
  host: string | null = null;
  port: number | null = null;
  servername: string | null = null;
  ca: RuntimeValue = null;
  cert: RuntimeValue = null;
  key: RuntimeValue = null;
  passphrase: string | null = null;
  timeout: number | null = null;
}

/**
 * TLS server options.
 */
export class TlsOptions extends CommonConnectionOptions {
  handshakeTimeout: number | null = null;
  sessionTimeout: number | null = null;
  ca: RuntimeValue = null;
  cert: RuntimeValue = null;
  key: RuntimeValue = null;
  passphrase: string | null = null;
  allowHalfOpen: boolean | null = null;
  pauseOnConnect: boolean | null = null;
}
