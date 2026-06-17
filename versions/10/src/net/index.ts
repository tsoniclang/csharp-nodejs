/**
 * Node.js net module — asynchronous network API for creating stream-based
 * TCP or IPC servers and clients.
 *
 */

import type {} from "../type-bootstrap.ts";

import { overloads as O } from "@tsonic/core/lang.js";
import type { int } from "@tsonic/core/types.js";

export { BlockList } from "./block-list.ts";
export { SocketAddress } from "./block-list.ts";
export { Socket } from "./socket.ts";
export { Server } from "./server.ts";
export {
  AddressInfo,
  IpcSocketConnectOpts,
  ListenOptions,
  ServerOpts,
  SocketAddressInitOptions,
  SocketConstructorOpts,
  TcpSocketConnectOpts,
} from "./options.ts";

import { Server } from "./server.ts";
import { Socket } from "./socket.ts";
import type { ServerOpts, TcpSocketConnectOpts } from "./options.ts";
import { toUnaryEventListener } from "../events-module.ts";

// ==================== Module-level state ====================

let defaultAutoSelectFamily: boolean = false;
let defaultAutoSelectFamilyAttemptTimeout: int = 250;

// ==================== createServer ====================

/**
 * Creates a new TCP or IPC server.
 */
export function createServer(
  connectionListener?: (socket: Socket) => void
): Server;
export function createServer(
  options: ServerOpts,
  connectionListener?: (socket: Socket) => void
): Server;
export function createServer(
  _optionsOrListener?: any,
  _connectionListener?: any
): any {
  throw new Error("Unreachable overload stub");
}

function createServer_listener(
  connectionListener?: (socket: Socket) => void
): Server {
  const server = new Server();
  if (connectionListener !== undefined) {
    server.on("connection", toUnaryEventListener(connectionListener)!);
  }
  return server;
}

function createServer_options(
  options: ServerOpts,
  connectionListener?: (socket: Socket) => void
): Server {
  return new Server(options, connectionListener);
}

// ==================== connect / createConnection ====================

/**
 * Creates a new socket connection.
 */
export function connect(
  port: int,
  host?: string,
  connectionListener?: () => void
): Socket;
export function connect(
  options: TcpSocketConnectOpts,
  connectionListener?: () => void
): Socket;
export function connect(
  path: string,
  connectionListener?: () => void
): Socket;
export function connect(
  _portOrOptionsOrPath: any,
  _hostOrListener?: any,
  _connectionListener?: any
): any {
  throw new Error("Unreachable overload stub");
}

function connect_port(
  port: int,
  host?: string,
  connectionListener?: () => void
): Socket {
  const socket = new Socket();
  socket.connect(port, host, connectionListener);
  return socket;
}

function connect_options(
  options: TcpSocketConnectOpts,
  connectionListener?: () => void
): Socket {
  const socket = new Socket();
  socket.connect(options, connectionListener);
  return socket;
}

function connect_path(
  path: string,
  connectionListener?: () => void
): Socket {
  const socket = new Socket();
  socket.connect(path, connectionListener);
  return socket;
}

/**
 * Alias for connect().
 */
export function createConnection(
  port: int,
  host?: string,
  connectionListener?: () => void
): Socket;
export function createConnection(
  options: TcpSocketConnectOpts,
  connectionListener?: () => void
): Socket;
export function createConnection(
  path: string,
  connectionListener?: () => void
): Socket;
export function createConnection(
  _portOrOptionsOrPath: any,
  _hostOrListener?: any,
  _connectionListener?: any
): any {
  throw new Error("Unreachable overload stub");
}

function createConnection_port(
  port: int,
  host?: string,
  connectionListener?: () => void
): Socket {
  return connect_port(port, host, connectionListener);
}

function createConnection_options(
  options: TcpSocketConnectOpts,
  connectionListener?: () => void
): Socket {
  return connect_options(options, connectionListener);
}

function createConnection_path(
  path: string,
  connectionListener?: () => void
): Socket {
  return connect_path(path, connectionListener);
}

// ==================== IP Utilities ====================

const IPV4_REGEX =
  /^(?:(?:25[0-5]|2[0-4]\d|1\d\d|[1-9]?\d)\.){3}(?:25[0-5]|2[0-4]\d|1\d\d|[1-9]?\d)$/;

const IPV6_REGEX =
  /^(([0-9a-fA-F]{1,4}:){7}[0-9a-fA-F]{1,4}|([0-9a-fA-F]{1,4}:){1,7}:|([0-9a-fA-F]{1,4}:){1,6}:[0-9a-fA-F]{1,4}|([0-9a-fA-F]{1,4}:){1,5}(:[0-9a-fA-F]{1,4}){1,2}|([0-9a-fA-F]{1,4}:){1,4}(:[0-9a-fA-F]{1,4}){1,3}|([0-9a-fA-F]{1,4}:){1,3}(:[0-9a-fA-F]{1,4}){1,4}|([0-9a-fA-F]{1,4}:){1,2}(:[0-9a-fA-F]{1,4}){1,5}|[0-9a-fA-F]{1,4}:((:[0-9a-fA-F]{1,4}){1,6})|:((:[0-9a-fA-F]{1,4}){1,7}|:)|fe80:(:[0-9a-fA-F]{0,4}){0,4}%[0-9a-zA-Z]+|::(ffff(:0{1,4})?:)?((25[0-5]|(2[0-4]|1?[0-9])?[0-9])\.){3}(25[0-5]|(2[0-4]|1?[0-9])?[0-9])|([0-9a-fA-F]{1,4}:){1,4}:((25[0-5]|(2[0-4]|1?[0-9])?[0-9])\.){3}(25[0-5]|(2[0-4]|1?[0-9])?[0-9]))$/;

/**
 * Tests if input is an IP address. Returns 0 for invalid strings,
 * returns 4 for IP version 4 addresses, and returns 6 for IP version 6 addresses.
 */
export const isIP = (input: string): int => {
  if (input === undefined || input === null || input.length === 0) {
    return 0;
  }

  if (IPV4_REGEX.test(input)) {
    return 4;
  }

  if (IPV6_REGEX.test(input)) {
    return 6;
  }

  return 0;
};

/**
 * Returns true if input is a version 4 IP address, otherwise returns false.
 */
export const isIPv4 = (input: string): boolean => {
  return isIP(input) === 4;
};

/**
 * Returns true if input is a version 6 IP address, otherwise returns false.
 */
export const isIPv6 = (input: string): boolean => {
  return isIP(input) === 6;
};

// ==================== Auto-Select Family Configuration ====================

/**
 * Gets the default value of autoSelectFamily option of socket.connect(options).
 */
export const getDefaultAutoSelectFamily = (): boolean => {
  return defaultAutoSelectFamily;
};

/**
 * Sets the default value of autoSelectFamily option of socket.connect(options).
 */
export const setDefaultAutoSelectFamily = (value: boolean): void => {
  defaultAutoSelectFamily = value;
};

/**
 * Gets the default value of autoSelectFamilyAttemptTimeout option of socket.connect(options).
 */
export const getDefaultAutoSelectFamilyAttemptTimeout = (): int => {
  return defaultAutoSelectFamilyAttemptTimeout;
};

/**
 * Sets the default value of autoSelectFamilyAttemptTimeout option of socket.connect(options).
 */
export const setDefaultAutoSelectFamilyAttemptTimeout = (
  value: int
): void => {
  defaultAutoSelectFamilyAttemptTimeout = value;
};

O(createServer_listener).family(createServer);
O(createServer_options).family(createServer);
O(connect_port).family(connect);
O(connect_options).family(connect);
O(connect_path).family(connect);
O(createConnection_port).family(createConnection);
O(createConnection_options).family(createConnection);
O(createConnection_path).family(createConnection);
