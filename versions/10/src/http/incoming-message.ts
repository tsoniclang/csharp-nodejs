/**
 * Node.js http.IncomingMessage — represents an incoming HTTP request (server-side)
 * or response (client-side).
 *
 *
 * This class requires OS network substrate for the actual readable-stream body.
 * Class shape and method signatures are ported; body-streaming internals are
 * stubbed with TODO markers.
 */

import type { byte, int } from "@tsonic/core/types.js";
import { Convert } from "@tsonic/dotnet/System.js";
import { MemoryStream } from "@tsonic/dotnet/System.IO.js";
import type { HttpListenerRequest } from "@tsonic/dotnet/System.Net.js";
import { Encoding } from "@tsonic/dotnet/System.Text.js";
import {
  EventEmitter,
  toEventListener,
  toUnaryEventListener,
} from "../events-module.ts";

/**
 * Represents an incoming HTTP request (server-side) or response (client-side).
 * Extends EventEmitter and implements a simplified readable-stream interface.
 */
export class IncomingMessage extends EventEmitter {
  _method: string | null;
  _url: string | null;
  _httpVersion: string;
  _statusCode: int | null;
  _statusMessage: string | null;
  _headers: Record<string, string>;
  _complete: boolean = false;
  _nativeRequest: HttpListenerRequest | null;
  _bodyReadPromise: Promise<string> | null = null;
  _bodyBytesReadPromise: Promise<Uint8Array> | null = null;
  _bodyText: string | null = null;
  _bodyBytes: Uint8Array | null = null;
  _bodyEmitted: boolean = false;

  constructor(request?: HttpListenerRequest | null) {
    super();
    this._nativeRequest = request ?? null;
    this._method = this._nativeRequest?.HttpMethod ?? null;
    this._url = this._nativeRequest?.RawUrl ?? null;
    this._httpVersion = this._nativeRequest?.ProtocolVersion.ToString() ?? "1.1";
    this._statusCode = null;
    this._statusMessage = null;
    this._headers = {};

    if (this._nativeRequest !== null) {
      for (const headerName of this._nativeRequest.Headers.AllKeys) {
        if (headerName === undefined || headerName === null) {
          continue;
        }

        const headerValue = this._nativeRequest.Headers.Get(headerName);
        if (headerValue !== undefined && headerValue !== null) {
          this._headers[(headerName as string).toLowerCase()] = headerValue;
        }
      }
    }
  }

  /**
   * Request method (server-side) or null (client-side).
   */
  get method(): string | null {
    return this._method;
  }

  /**
   * Request URL (server-side) or null (client-side).
   */
  get url(): string | null {
    return this._url;
  }

  /**
   * HTTP version sent by the client.
   */
  get httpVersion(): string {
    return this._httpVersion;
  }

  /**
   * Response status code (client-side) or null (server-side).
   */
  get statusCode(): int | null {
    return this._statusCode;
  }

  /**
   * Response status message (client-side) or null (server-side).
   */
  get statusMessage(): string | null {
    return this._statusMessage;
  }

  /**
   * Request/response headers object.
   */
  get headers(): Record<string, string> {
    return this._headers;
  }

  /**
   * Indicates that the underlying connection was closed.
   */
  get complete(): boolean {
    return this._complete;
  }

  /**
   * Calls destroy() on the socket that received the IncomingMessage.
   */
  destroy(): void {
    this._complete = true;
    this.emit("close");
  }

  /**
   * Sets the timeout value in milliseconds for the incoming message.
   * @param msecs - Timeout in milliseconds.
   * @param callback - Optional callback for timeout event.
   * @returns The IncomingMessage instance.
   */
  setTimeout(msecs: int, callback?: () => void): IncomingMessage {
    if (msecs < 0) {
      throw new Error("Timeout must be non-negative");
    }

    if (callback !== undefined) {
      this.once("timeout", toEventListener(callback)!);
    }

    // TODO: Implement actual timeout mechanism (requires OS timer substrate)
    return this;
  }

  /**
   * Reads the entire body as a string (simplified implementation).
   * In a full implementation, this would be a streaming interface.
   * @returns The body content as a string.
   */
  async readAll(): Promise<string> {
    const body = await this._ensureBodyLoaded();
    const bodyBytes = await this._ensureBodyBytesLoaded();
    this._emitLoadedBodyBytesOnce(bodyBytes);
    return body;
  }

  /**
   * Reads the entire body as bytes.
   * @returns The body content as raw bytes.
   */
  async readAllBytes(): Promise<Uint8Array> {
    const body = await this._ensureBodyBytesLoaded();
    this._emitLoadedBodyBytesOnce(body);
    return body;
  }

  /**
   * Event handler for 'data' event.
   */
  onData(callback: (chunk: string | Uint8Array) => void): void {
    this.on("data", toUnaryEventListener<string | Uint8Array>(callback)!);
  }

  /**
   * Event handler for 'end' event.
   */
  onEnd(callback: () => void): void {
    this.on("end", toEventListener(callback)!);
  }

  /**
   * Event handler for 'close' event.
   */
  onClose(callback: () => void): void {
    this.on("close", toEventListener(callback)!);
  }

  // -- Internal setters for server/client construction --

  /** @internal */
  _setMethod(method: string | null): void {
    this._method = method;
  }

  /** @internal */
  _setUrl(url: string | null): void {
    this._url = url;
  }

  /** @internal */
  _setHttpVersion(version: string): void {
    this._httpVersion = version;
  }

  /** @internal */
  _setStatusCode(code: int | null): void {
    this._statusCode = code;
  }

  /** @internal */
  _setStatusMessage(message: string | null): void {
    this._statusMessage = message;
  }

  /** @internal */
  _setHeaders(headers: Record<string, string>): void {
    this._headers = headers;
  }

  /** @internal */
  _markComplete(): void {
    this._complete = true;
  }

  /**
   * Emits buffered client body data/end/close events.
   * @internal
   */
  _emitBufferedClientBody(body: string): void {
    this._bodyText = body;
    this._bodyBytes = toUint8Array(Encoding.UTF8.GetBytes(body));
    this._emitLoadedBodyBytesOnce(this._bodyBytes);
  }

  /**
   * Starts background body emission for server-side request listeners that use
   * the stream/event API rather than `readAll()`.
   * @internal
   */
  _beginStreamingBody(): void {
    void this._streamLoadedBody();
  }

  async _streamLoadedBody(): Promise<void> {
    const bodyBytes = await this._ensureBodyBytesLoaded();
    this._emitLoadedBodyBytesOnce(bodyBytes);
  }

  _ensureBodyLoaded(): Promise<string> {
    if (this._bodyReadPromise !== null) {
      return this._bodyReadPromise;
    }

    this._bodyReadPromise = this._loadBody();

    return this._bodyReadPromise;
  }

  _ensureBodyBytesLoaded(): Promise<Uint8Array> {
    if (this._bodyBytesReadPromise !== null) {
      return this._bodyBytesReadPromise;
    }

    this._bodyBytesReadPromise = this._loadBodyBytes();

    return this._bodyBytesReadPromise;
  }

  async _loadBody(): Promise<string> {
    if (this._bodyText !== null) {
      return this._bodyText;
    }

    const bodyBytes = await this._ensureBodyBytesLoaded();
    this._bodyText = this._decodeBodyBytes(bodyBytes);
    return this._bodyText;
  }

  async _loadBodyBytes(): Promise<Uint8Array> {
    if (this._bodyBytes !== null) {
      return this._bodyBytes;
    }

    if (
      this._nativeRequest === null ||
      !this._nativeRequest.HasEntityBody
    ) {
      this._bodyBytes = new Uint8Array(0);
      return this._bodyBytes;
    }

    const output = new MemoryStream();
    try {
      this._nativeRequest.InputStream.CopyTo(output);
      this._bodyBytes = toUint8Array(output.ToArray());
    } finally {
      output.Dispose();
    }

    return this._bodyBytes;
  }

  _decodeBodyBytes(bodyBytes: Uint8Array): string {
    if (bodyBytes.length === 0) {
      return "";
    }

    if (
      this._nativeRequest === null ||
      !this._nativeRequest.HasEntityBody
    ) {
      return "";
    }

    const encoding = this._nativeRequest.ContentEncoding ?? Encoding.UTF8;
    return encoding.GetString(toByteArray(bodyBytes));
  }

  _markBodyEmitted(): boolean {
    if (this._bodyEmitted) {
      return false;
    }

    this._bodyEmitted = true;
    this._complete = true;
    return true;
  }

  _emitLoadedBodyOnce(body: string): void {
    if (!this._markBodyEmitted()) {
      return;
    }

    if (body.length > 0) {
      this.emit("data", toUint8Array(Encoding.UTF8.GetBytes(body)));
    }

    this.emit("end");
    this.emit("close");
  }

  _emitLoadedBodyBytesOnce(bodyBytes: Uint8Array): void {
    if (!this._markBodyEmitted()) {
      return;
    }

    this._bodyBytes = bodyBytes;
    this._bodyText = this._bodyText ?? this._decodeBodyBytes(bodyBytes);
    if (bodyBytes.length > 0) {
      this.emit("data", bodyBytes);
    }
    this.emit("end");
    this.emit("close");
  }
}

function toByteArray(bytes: Uint8Array): byte[] {
  return Array.from(bytes, (value) => Convert.ToByte(value));
}

function toUint8Array(bytes: byte[]): Uint8Array {
  const result = new Uint8Array(bytes.length);
  for (let index = 0; index < bytes.length; index += 1) {
    result[index] = bytes[index]!;
  }
  return result;
}
