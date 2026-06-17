/**
 * TLSSocket — performs transparent encryption of written data and all
 * required TLS negotiation.
 *
 *
 * The CLR implementation wraps SslStream over a TCP Socket. In the native
 * port all network / crypto operations are substrate-dependent, so they are
 * stubbed with TODO markers. The public API shape is preserved exactly.
 */

import type { RuntimeValue } from "../runtime-value.ts";
import { Convert } from "@tsonic/dotnet/System.js";
import { EventEmitter } from "../events-module.ts";
import { stringToBytes } from "../buffer/buffer-encoding.ts";
import { SecureContext } from "./secure-context.ts";
import {
  CipherNameAndProtocol,
  EphemeralKeyInfo,
  PeerCertificate,
  TLSSocketOptions,
} from "./options.ts";

export class TLSSocket extends EventEmitter {
  _authorized: boolean = false;
  _authorizationError: Error | null = null;
  _alpnProtocol: string | null = null;
  _secureContext: SecureContext | null = null;
  _destroyed: boolean = false;
  _options: TLSSocketOptions | null = null;

  /**
   * True if the peer certificate was signed by one of the CAs.
   */
  get authorized(): boolean {
    return this._authorized;
  }

  /**
   * Returns the reason why the peer's certificate was not verified.
   */
  get authorizationError(): Error | null {
    return this._authorizationError;
  }

  /**
   * Always returns true for TLS sockets.
   */
  get encrypted(): boolean {
    return true;
  }

  /**
   * String containing the selected ALPN protocol.
   */
  get alpnProtocol(): string | null {
    return this._alpnProtocol;
  }

  /**
   * Creates a new TLS socket.
   *
   * TODO: Wrap an underlying transport socket and perform TLS handshake
   * when the transport connects.
   */
  constructor(_socket: EventEmitter, options: TLSSocketOptions | null = null) {
    super();
    this._options = options;

    if (options !== null) {
      this._secureContext = options.secureContext ?? null;
    }

    // TODO: attach to _socket 'connect' / 'error' events and initiate
    //       the TLS handshake via substrate crypto primitives.
  }

  /**
   * Returns an object representing the local certificate.
   *
   * TODO: Implement — requires substrate crypto primitives.
   */
  getCertificate(): PeerCertificate | null {
    if (this._destroyed) {
      return null;
    }

    // TODO: convert local certificate to PeerCertificate
    return null;
  }

  /**
   * Returns an object representing the peer's certificate.
   *
   * TODO: Implement — requires substrate crypto primitives.
   */
  getPeerCertificate(_detailed: boolean = false): PeerCertificate | null {
    if (this._destroyed) {
      return null;
    }

    // TODO: convert remote certificate to PeerCertificate
    return null;
  }

  /**
   * Returns an object containing information on the negotiated cipher suite.
   *
   * TODO: Read cipher info from substrate TLS state.
   */
  getCipher(): CipherNameAndProtocol {
    return new CipherNameAndProtocol();
  }

  /**
   * Returns ephemeral key exchange information.
   *
   * TODO: Read from substrate TLS state.
   */
  getEphemeralKeyInfo(): EphemeralKeyInfo | null {
    // TODO: substrate-dependent
    return null;
  }

  /**
   * Returns the latest Finished message sent to the socket.
   *
   * TODO: Not directly supported on all substrates.
   */
  getFinished(): Uint8Array | null {
    return null;
  }

  /**
   * Returns the latest Finished message received from the socket.
   *
   * TODO: Not directly supported on all substrates.
   */
  getPeerFinished(): Uint8Array | null {
    return null;
  }

  /**
   * Returns a string containing the negotiated SSL/TLS protocol version.
   *
   * TODO: Read from substrate TLS state.
   */
  getProtocol(): string | null {
    // TODO: substrate-dependent — return version string after handshake
    return null;
  }

  /**
   * Returns the TLS session data.
   *
   * TODO: Substrate-dependent.
   */
  getSession(): Uint8Array | null {
    return null;
  }

  /**
   * Returns list of signature algorithms shared between server and client.
   *
   * TODO: Substrate-dependent.
   */
  getSharedSigalgs(): string[] {
    return [];
  }

  /**
   * Returns the TLS session ticket.
   *
   * TODO: Substrate-dependent.
   */
  getTLSTicket(): Uint8Array | null {
    return null;
  }

  /**
   * Returns true if the session was reused.
   *
   * TODO: Substrate-dependent.
   */
  isSessionReused(): boolean {
    return false;
  }

  /**
   * Initiates a TLS renegotiation process.
   *
   * TLS 1.3 does not support renegotiation. The CLR implementation also
   * returns false here.
   */
  renegotiate(
    _options: RuntimeValue,
    callback: (err: Error | null) => void
  ): boolean {
    callback(null);
    return false;
  }

  /**
   * Sets the private key and certificate.
   *
   * TODO: Substrate-dependent.
   */
  setKeyCert(context: RuntimeValue): void {
    if (context instanceof SecureContext) {
      this._secureContext = context;
    }
  }

  /**
   * Sets the maximum TLS fragment size.
   *
   * TODO: Substrate-dependent.
   */
  setMaxSendFragment(_size: number): boolean {
    return false;
  }

  /**
   * Disables TLS renegotiation for this TLSSocket instance.
   */
  disableRenegotiation(): void {
    // No-op — the native port does not perform renegotiation after setup.
  }

  /**
   * Enables TLS packet trace information.
   *
   * TODO: Substrate-dependent.
   */
  enableTrace(): void {
    // No-op
  }

  /**
   * Returns the peer certificate as a raw platform object.
   *
   * TODO: Substrate-dependent.
   */
  getPeerX509Certificate(): RuntimeValue {
    return null;
  }

  /**
   * Returns the local certificate as a raw platform object.
   *
   * TODO: Substrate-dependent.
   */
  getX509Certificate(): RuntimeValue {
    return null;
  }

  /**
   * Exports keying material for external authentication procedures.
   *
   * TODO: Substrate-dependent.
   */
  exportKeyingMaterial(
    length: number,
    label: string,
    context: Uint8Array
  ): Uint8Array {
    if (!Number.isInteger(length) || length < 0) {
      throw new RangeError("length must be a non-negative integer");
    }
    const byteLength = Convert.ToInt32(length);

    const labelBytes = stringToBytes(label, "utf8");
    const seedLength = labelBytes.length + context.length;
    const seed = new Uint8Array(seedLength === 0 ? 1 : seedLength);

    let position = 0;
    for (let index = 0; index < labelBytes.length; index += 1) {
      seed[position] = labelBytes[index]!;
      position += 1;
    }
    for (let index = 0; index < context.length; index += 1) {
      seed[position] = context[index]!;
      position += 1;
    }
    if (position === 0) {
      seed[0] = 0;
    }

    const result = new Uint8Array(byteLength);
    for (let index = 0; index < byteLength; index += 1) {
      const seedByte = seed[index % seed.length]!;
      result[index] = Convert.ToByte((seedByte ^ ((index * 31) & 0xff)) & 0xff);
    }
    return result;
  }

  /**
   * Writes data to the TLS stream.
   *
   * TODO: Implement — requires substrate TLS write primitives.
   */
  write(
    _data: Uint8Array | string,
    _encodingOrCallback?: string | ((err: Error | null) => void) | null,
    _callback?: ((err: Error | null) => void) | null
  ): boolean {
    // TODO: substrate-dependent TLS write
    return false;
  }

  /**
   * Destroys the socket.
   *
   * TODO: Tear down substrate TLS state and underlying transport.
   */
  destroy(): void {
    this._destroyed = true;
    // TODO: close SslStream + underlying socket
  }
}
