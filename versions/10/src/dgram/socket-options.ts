/**
 * Options for creating and binding dgram sockets.
 *
 */
import type { int } from "@tsonic/core/types.js";

/** Options for creating a dgram socket. */
export class SocketOptions {
  /** The type of socket - 'udp4' or 'udp6'. */
  type: string = "udp4";

  /** If true, the socket will reuse the address. */
  reuseAddr: boolean = false;

  /** If true, the socket will reuse the port (SO_REUSEPORT). */
  reusePort: boolean = false;

  /** For IPv6 sockets, if true the socket will only receive IPv6 traffic. */
  ipv6Only: boolean = false;

  /** Sets the SO_RCVBUF socket receive buffer size in bytes. */
  recvBufferSize: int | undefined = undefined;

  /** Sets the SO_SNDBUF socket send buffer size in bytes. */
  sendBufferSize: int | undefined = undefined;
}

/** Options for binding a dgram socket. */
export class BindOptions {
  /** The port to bind to. */
  port: int | undefined = undefined;

  /** The address to bind to. */
  address: string | undefined = undefined;

  /** If true, the socket will be bound exclusively. */
  exclusive: boolean = false;

  /** File descriptor to bind the socket to. */
  fd: int | undefined = undefined;
}
