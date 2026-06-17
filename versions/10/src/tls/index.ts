/**
 * Node.js tls module — Transport Layer Security (TLS) and Secure Socket
 * Layer (SSL) protocol implementation.
 *
 */

import type {} from "../type-bootstrap.ts";
import type { RuntimeValue } from "../runtime-value.ts";

export {
  CipherNameAndProtocol,
  CommonConnectionOptions,
  ConnectionOptions,
  DetailedPeerCertificate,
  EphemeralKeyInfo,
  PeerCertificate,
  SecureContextOptions,
  TLSCertificateInfo,
  TLSSocketOptions,
  TlsOptions,
} from "./options.ts";

export { SecureContext } from "./secure-context.ts";
export { TLSSocket } from "./tlssocket.ts";
export { TLSServer } from "./server.ts";

import { SecureContext } from "./secure-context.ts";
import { TLSServer } from "./server.ts";
import { TLSSocket } from "./tlssocket.ts";
import {
  ConnectionOptions,
  PeerCertificate,
  SecureContextOptions,
  TLSSocketOptions,
  TlsOptions,
} from "./options.ts";
import { EventEmitter } from "../events-module.ts";

// ---------------------------------------------------------------------------
// Module-level constants
// ---------------------------------------------------------------------------

/**
 * Client renegotiation limit.
 */
export const CLIENT_RENEG_LIMIT: number = 3;

/**
 * Client renegotiation window in seconds.
 */
export const CLIENT_RENEG_WINDOW: number = 600;

/**
 * The default curve name to use for ECDH key agreement.
 */
export const DEFAULT_ECDH_CURVE: string = "auto";

/**
 * The default value of the maxVersion option.
 */
export const DEFAULT_MAX_VERSION: string = "TLSv1.3";

/**
 * The default value of the minVersion option.
 */
export const DEFAULT_MIN_VERSION: string = "TLSv1.2";

/**
 * The default value of the ciphers option.
 */
export const DEFAULT_CIPHERS: string =
  "TLS_AES_256_GCM_SHA384:TLS_CHACHA20_POLY1305_SHA256:TLS_AES_128_GCM_SHA256";

/**
 * Root certificates from the bundled Mozilla CA store.
 */
export const rootCertificates: readonly string[] = [];

// ---------------------------------------------------------------------------
// Factory functions
// ---------------------------------------------------------------------------

/**
 * Creates a new TLS server.
 */
export const createServer = (
  optionsOrListener?: TlsOptions | ((socket: TLSSocket) => void) | null,
  secureConnectionListener?: ((socket: TLSSocket) => void) | null
): TLSServer => {
  return new TLSServer(
    optionsOrListener ?? null,
    secureConnectionListener ?? null
  );
};

/**
 * Creates a TLS connection to a server.
 *
 * TODO: Implement — requires substrate TCP connect + TLS handshake.
 */
export const connect = (
  options: ConnectionOptions,
  secureConnectListener?: (() => void) | null,
): TLSSocket => {
  const rejectUnauthorized = options.rejectUnauthorized ?? true;

  const ctxOpts = new SecureContextOptions();
  ctxOpts.ca = options.ca;
  ctxOpts.cert = options.cert;
  ctxOpts.key = options.key;
  ctxOpts.passphrase = options.passphrase;
  ctxOpts.minVersion = rejectUnauthorized ? "TLSv1.2" : null;

  const secureContext = createSecureContext(ctxOpts);

  const socketOpts = new TLSSocketOptions();
  socketOpts.isServer = false;
  socketOpts.servername = options.servername ?? options.host;
  socketOpts.ca = options.ca;
  socketOpts.cert = options.cert;
  socketOpts.key = options.key;
  socketOpts.passphrase = options.passphrase;
  socketOpts.rejectUnauthorized = rejectUnauthorized;
  socketOpts.secureContext = secureContext;

  // TODO: create a real transport socket from the substrate net module
  const transportSocket = new EventEmitter();

  const tlsSocket = new TLSSocket(transportSocket, socketOpts);

  if (secureConnectListener !== null && secureConnectListener !== undefined) {
    tlsSocket.once("secureConnect", (..._args: RuntimeValue[]) => {
      secureConnectListener();
    });
  }

  // TODO: initiate TCP connection then TLS handshake via substrate
  // const port = options.port ?? 443;
  // const host = options.host ?? "localhost";
  // transportSocket.connect(port, host);

  return tlsSocket;
};

/**
 * Creates a secure context.
 */
export const createSecureContext = (
  options?: SecureContextOptions | null
): SecureContext => {
  const context = new SecureContext();

  if (options !== null && options !== undefined) {
    context.loadCertificate(options.cert, options.key, options.passphrase);
    context.loadCACertificates(options.ca);
    context.setProtocols(options.minVersion, options.maxVersion);
  }

  return context;
};

/**
 * Verifies the certificate is issued to hostname.
 */
export const checkServerIdentity = (
  hostname: string,
  cert: PeerCertificate
): Error | null => {
  if (cert === null || cert === undefined) {
    return new Error("Certificate is required");
  }

  // Check CN matches hostname
  if (cert.subject.CN === hostname) {
    return null;
  }

  // Check subject alternative names
  if (cert.subjectaltname !== null) {
    const sans = cert.subjectaltname.split(",");
    const count = sans.length;
    for (let i = 0; i < count; i = i + 1) {
      const san = sans[i]!.trim();
      if (san.startsWith("DNS:")) {
        const dnsName = san.substring(4);
        if (dnsName === hostname || matchesWildcard(dnsName, hostname)) {
          return null;
        }
      } else if (san.startsWith("IP Address:")) {
        const ipAddress = san.substring(11);
        if (ipAddress === hostname) {
          return null;
        }
      }
    }
  }

  return new Error(
    `Hostname/IP does not match certificate's altnames: Host: ${hostname}. is not in the cert's altnames`
  );
};

/**
 * Returns an array with the names of the supported TLS ciphers.
 */
export const getCiphers = (): string[] => {
  return [
    "TLS_AES_256_GCM_SHA384",
    "TLS_CHACHA20_POLY1305_SHA256",
    "TLS_AES_128_GCM_SHA256",
    "TLS_ECDHE_RSA_WITH_AES_256_GCM_SHA384",
    "TLS_ECDHE_RSA_WITH_AES_128_GCM_SHA256",
    "TLS_DHE_RSA_WITH_AES_256_GCM_SHA384",
    "TLS_DHE_RSA_WITH_AES_128_GCM_SHA256",
  ];
};

/**
 * Returns an array containing the CA certificates.
 *
 * TODO: Read from the substrate's system certificate store.
 */
export const getCACertificates = (_type: string = "default"): string[] => {
  // TODO: substrate-dependent — access system trust store
  return [];
};

/**
 * Sets the default CA certificates.
 *
 * TODO: Substrate-dependent.
 */
export const setDefaultCACertificates = (_certs: string[]): void => {
  // TODO: substrate-dependent
};

// ---------------------------------------------------------------------------
// Internal helpers
// ---------------------------------------------------------------------------

const matchesWildcard = (pattern: string, hostname: string): boolean => {
  if (!pattern.startsWith("*.")) {
    return false;
  }

  const suffix = pattern.substring(2);
  return (
    hostname.endsWith(suffix) &&
    hostname.indexOf(".") === hostname.lastIndexOf(".")
  );
};
