/**
 * net module — option types and data classes.
 *
 */
import type { int } from "@tsonic/core/types.js";

/**
 * Address information returned by server.address() and socket.address().
 */
export class AddressInfo {
  address: string = "";
  family: string = "";
  port: int = 0;
}

/**
 * Options for Socket constructor.
 */
export class SocketConstructorOpts {
  fd: int | undefined = undefined;
  allowHalfOpen: boolean | undefined = undefined;
  readable: boolean | undefined = undefined;
  writable: boolean | undefined = undefined;
}

/**
 * TCP socket connection options.
 */
export class TcpSocketConnectOpts {
  port: int = 0;
  host: string | undefined = undefined;
  localAddress: string | undefined = undefined;
  localPort: int | undefined = undefined;
  hints: int | undefined = undefined;
  family: int | undefined = undefined;
  noDelay: boolean | undefined = undefined;
  keepAlive: boolean | undefined = undefined;
  keepAliveInitialDelay: int | undefined = undefined;
}

/**
 * IPC socket connection options.
 */
export class IpcSocketConnectOpts {
  path: string = "";
}

/**
 * Options for server.listen().
 */
export class ListenOptions {
  port: int | undefined = undefined;
  host: string | undefined = undefined;
  path: string | undefined = undefined;
  backlog: int | undefined = undefined;
  ipv6Only: boolean | undefined = undefined;
}

/**
 * Options for Server constructor.
 */
export class ServerOpts {
  allowHalfOpen: boolean | undefined = undefined;
  pauseOnConnect: boolean | undefined = undefined;
}

/**
 * Options for SocketAddress constructor.
 */
export class SocketAddressInitOptions {
  address: string | undefined = undefined;
  family: string | undefined = undefined;
  flowlabel: int | undefined = undefined;
  port: int | undefined = undefined;
}
