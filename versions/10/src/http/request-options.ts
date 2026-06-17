/**
 * Node.js http.RequestOptions — options for making HTTP requests.
 *
 */

import type { int } from "@tsonic/core/types.js";
import type { RuntimeValue } from "../runtime-value.ts";

/**
 * Options for making HTTP requests.
 * Matches Node.js http.RequestOptions interface.
 */
export class RequestOptions {
  /**
   * Domain name or IP address of the server. Default: 'localhost'
   */
  hostname: string | null = null;

  /**
   * Alias for hostname.
   */
  get host(): string | null {
    return this.hostname;
  }

  set host(value: string | null) {
    this.hostname = value;
  }

  /**
   * Port of remote server. Default: 80
   */
  port: int = 80 as int;

  /**
   * Request path. Should include query string if any. Default: '/'
   */
  path: string | null = "/";

  /**
   * HTTP request method. Default: 'GET'
   */
  method: string = "GET";

  /**
   * Object containing request headers.
   */
  headers: Map<string, string> | null = null;

  /**
   * Protocol to use. Default: 'http:'
   */
  protocol: string = "http:";

  /**
   * Request timeout in milliseconds. Default: no timeout
   */
  timeout: int | null = null;

  /**
   * Controls Agent behavior. Possible values:
   * - null (default): use global agent
   * - Agent instance: explicitly use passed Agent
   * - false: disable connection pooling
   */
  agent: RuntimeValue = null;

  /**
   * Authentication in the form 'user:password' for basic auth.
   */
  auth: string | null = null;
}
