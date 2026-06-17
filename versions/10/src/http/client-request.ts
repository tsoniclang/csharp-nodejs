/**
 * Node.js http.ClientRequest — wraps an outgoing HTTP request.
 *
 *
 * This class requires OS network substrate for the actual HTTP client.
 * Class shape and method signatures are ported; network I/O internals are
 * stubbed with TODO markers.
 */

import type { int } from "@tsonic/core/types.js";
import type { RequestOptions as RequestOptionsType } from "./request-options.ts";
import { bytesToBase64, stringToBytes } from "../buffer/buffer-encoding.ts";
import {
  EventEmitter,
  toEventListener,
  toUnaryEventListener,
} from "../events-module.ts";
import { IncomingMessage } from "./incoming-message.ts";

/**
 * Implements Node.js http.ClientRequest.
 * Provides Node.js-compatible API for making HTTP requests.
 * Extends EventEmitter to support events like 'response', 'error', 'timeout'.
 */
export class ClientRequest extends EventEmitter {
  _options: RequestOptionsType;
  _requestHeaders: Map<string, string> =
    new Map<string, string>();
  _requestBody: string = "";
  _aborted: boolean = false;
  _ended: boolean = false;

  constructor(
    options: RequestOptionsType,
    callback?: ((res: IncomingMessage) => void) | null
  ) {
    super();
    this._options = options;

    // Copy headers from options
    if (options.headers !== null) {
      options.headers.forEach((value, key, _map) => {
        this._requestHeaders.set(key, value);
      });
    }

    // Add basic auth if provided
    if (options.auth !== null) {
      this._requestHeaders.set(
        "Authorization",
        `Basic ${bytesToBase64(stringToBytes(options.auth, "utf8"))}`
      );
    }

    // Register response callback if provided
    if (callback !== undefined && callback !== null) {
      this.on("response", toUnaryEventListener(callback)!);
    }
  }

  /**
   * Gets the request path.
   */
  get path(): string {
    return this._options.path ?? "/";
  }

  /**
   * Gets the request method.
   */
  get method(): string {
    return this._options.method;
  }

  /**
   * Gets the request host.
   */
  get host(): string {
    return this._options.hostname ?? "localhost";
  }

  /**
   * Gets the request protocol.
   */
  get protocol(): string {
    return this._options.protocol;
  }

  /**
   * Boolean indicating if the request has been aborted.
   */
  get aborted(): boolean {
    return this._aborted;
  }

  /**
   * Sets a single header value for the request.
   * @param name - Header name.
   * @param value - Header value.
   */
  setHeader(name: string, value: string): void {
    if (this._ended) {
      throw new Error("Cannot set headers after request has been sent");
    }

    this._requestHeaders.set(name, value);
  }

  /**
   * Gets the value of a header.
   * @param name - Header name.
   * @returns Header value or null if not set.
   */
  getHeader(name: string): string | null {
    const value = this._requestHeaders.get(name);
    return value !== undefined ? value : null;
  }

  /**
   * Returns an array containing the unique names of the current outgoing headers.
   * @returns Array of header names.
   */
  getHeaderNames(): string[] {
    const names: string[] = [];
    this._requestHeaders.forEach((_value, key, _map) => {
      names.push(key);
    });
    return names;
  }

  /**
   * Removes a header that's already been added to the request.
   * @param name - Header name.
   */
  removeHeader(name: string): void {
    if (this._ended) {
      throw new Error("Cannot remove headers after request has been sent");
    }

    this._requestHeaders.delete(name);
  }

  /**
   * Writes a chunk of data to the request body.
   * @param chunk - The data to write.
   * @param encoding - Optional encoding (ignored, always UTF-8).
   * @param callback - Optional callback when chunk is flushed.
   * @returns True if entire data was flushed successfully.
   */
  write(
    chunk: string,
    encoding?: string | null,
    callback?: (() => void) | null
  ): boolean {
    if (this._ended) {
      throw new Error("Cannot write after request has been sent");
    }

    this._requestBody = this._requestBody + chunk;
    if (callback !== undefined && callback !== null) {
      callback();
    }
    return true;
  }

  /**
   * Finishes sending the request.
   * If any part of the body is unsent, it will flush them to the stream.
   * @param chunk - Optional final chunk to send.
   * @param encoding - Optional encoding (ignored, always UTF-8).
   * @param callback - Optional callback when request is sent.
   */
  end(
    chunk?: string | null,
    encoding?: string | null,
    callback?: (() => void) | null
  ): Promise<void> {
    if (this._ended) {
      return Promise.resolve();
    }

    if (chunk !== undefined && chunk !== null) {
      this.write(chunk, encoding);
    }

    this._ended = true;

    // TODO: Send the actual HTTP request using OS network substrate
    // The CLR version uses HttpClient.SendAsync here.
    // For now, emit error to indicate network substrate is not yet wired.

    if (callback !== undefined && callback !== null) {
      callback();
    }

    return Promise.resolve();
  }

  /**
   * Aborts the ongoing request.
   */
  abort(): void {
    if (this._aborted) {
      return;
    }

    this._aborted = true;
    this.emit("abort");
  }

  /**
   * Sets the timeout value in milliseconds for the request.
   * @param msecs - Timeout in milliseconds.
   * @param callback - Optional callback for timeout event.
   * @returns The ClientRequest instance.
   */
  setTimeout(
    msecs: int,
    callback?: (() => void) | null
  ): ClientRequest {
    this._options.timeout = msecs;

    if (callback !== undefined && callback !== null) {
      this.once("timeout", toEventListener(callback)!);
    }

    return this;
  }
}
