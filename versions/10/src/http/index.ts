/**
 * Node.js http module — HTTP server and client utilities.
 *
 */

import type {} from "../type-bootstrap.ts";

import type { int } from "@tsonic/core/types.js";
import type { ClientRequest as ClientRequestType } from "./client-request.ts";
import type { IncomingMessage as IncomingMessageType } from "./incoming-message.ts";
import type { RequestOptions as RequestOptionsType } from "./request-options.ts";
import type { Server as ServerType } from "./server.ts";
import type { ServerResponse as ServerResponseType } from "./server-response.ts";
import { URL } from "../url/url.ts";

export { ClientRequest } from "./client-request.ts";
export { IncomingMessage } from "./incoming-message.ts";
export { RequestOptions } from "./request-options.ts";
export { Server, AddressInfo } from "./server.ts";
export { ServerResponse } from "./server-response.ts";

import { ClientRequest } from "./client-request.ts";
import { RequestOptions } from "./request-options.ts";
import { Server } from "./server.ts";

const normalizePortNumber = (value: number): int => {
  if (Number.isInteger(value) && value >= -2147483648 && value <= 2147483647) {
    return value as int;
  }

  throw new RangeError(
    `port must be an Int32-compatible integer. Received ${String(value)}`
  );
};

/**
 * Maximum allowed size of HTTP headers in bytes.
 * Default: 16KB (matches Node.js)
 */
export const maxHeaderSize: int = (16 * 1024) as int;

/**
 * Creates a new HTTP server.
 * @param requestListener - Optional request handler function.
 * @returns A new Server instance.
 */
export const createServer = (
  requestListener?: ((req: IncomingMessageType, res: ServerResponseType) => void) | null
): ServerType => {
  return new Server(requestListener);
};

/**
 * Makes an HTTP request using options.
 * @param options - Request options.
 * @param callback - Optional callback for response.
 * @returns A ClientRequest instance.
 */
export const request = (
  options: RequestOptionsType,
  callback?: ((res: IncomingMessageType) => void) | null
): ClientRequestType => {
  return new ClientRequest(options, callback);
};

/**
 * Makes an HTTP request from a URL string.
 * @param url - The URL to request.
 * @param callback - Optional callback for response.
 * @returns A ClientRequest instance.
 */
export const requestFromUrl = (
  url: string,
  callback?: ((res: IncomingMessageType) => void) | null
): ClientRequestType => {
  const parsed = new URL(url);
  const options = new RequestOptions();
  options.protocol = parsed.protocol;
  options.hostname = parsed.hostname;
  options.path = `${parsed.pathname}${parsed.search}`;
  options.auth =
    parsed.username.length > 0
      ? `${parsed.username}:${parsed.password}`
      : null;
  if (parsed.port.length > 0) {
    options.port = normalizePortNumber(parseInt(parsed.port, 10));
  }
  options.method = "GET";
  return new ClientRequest(options, callback);
};

/**
 * Makes a GET request from a URL string.
 * Convenience method that calls request() and automatically calls req.end().
 * @param url - The URL to request.
 * @param callback - Optional callback for response.
 * @returns A ClientRequest instance.
 */
export const get = (
  url: string,
  callback?: ((res: IncomingMessageType) => void) | null
): ClientRequestType => {
  const req = requestFromUrl(url, callback);
  req.end();
  return req;
};

/**
 * Makes a GET request with options.
 * Convenience method that calls request() and automatically calls req.end().
 * @param options - Request options.
 * @param callback - Optional callback for response.
 * @returns A ClientRequest instance.
 */
export const getWithOptions = (
  options: RequestOptionsType,
  callback?: ((res: IncomingMessageType) => void) | null
): ClientRequestType => {
  const req = request(options, callback);
  req.end();
  return req;
};

/**
 * Validates that the given name is a valid HTTP header name.
 * Throws Error if invalid.
 * @param name - Header name to validate.
 */
export const validateHeaderName = (name: string): void => {
  if (name.length === 0) {
    throw new Error("Header name must be a valid string");
  }

  for (let index = 0; index < name.length; index += 1) {
    const c = name.charAt(index);
    if (!isValidHeaderNameChar(c)) {
      throw new Error(`Invalid character in header name: '${c}'`);
    }
  }
};

/**
 * Validates that the given value is a valid HTTP header value.
 * Throws Error if invalid.
 * @param name - Header name (for error messages).
 * @param value - Header value to validate.
 */
export const validateHeaderValue = (name: string, value: string): void => {
  for (let index = 0; index < value.length; index += 1) {
    const c = value.charAt(index);
    const code = c.charCodeAt(0);
    if ((code < 0x20 && code !== 0x09) || code === 0x7f) {
      throw new Error(`Invalid character in header '${name}' value`);
    }
  }
};

const isValidHeaderNameChar = (c: string): boolean => {
  const code = c.charCodeAt(0);

  // a-z
  if (code >= 0x61 && code <= 0x7a) return true;
  // A-Z
  if (code >= 0x41 && code <= 0x5a) return true;
  // 0-9
  if (code >= 0x30 && code <= 0x39) return true;

  // Special characters: ! # $ % & ' * + - . ^ _ ` | ~
  return (
    code === 0x21 || // !
    code === 0x23 || // #
    code === 0x24 || // $
    code === 0x25 || // %
    code === 0x26 || // &
    code === 0x27 || // '
    code === 0x2a || // *
    code === 0x2b || // +
    code === 0x2d || // -
    code === 0x2e || // .
    code === 0x5e || // ^
    code === 0x5f || // _
    code === 0x60 || // `
    code === 0x7c || // |
    code === 0x7e    // ~
  );
};
