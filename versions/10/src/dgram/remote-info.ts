/**
 * Information about the remote endpoint that sent a datagram.
 *
 */

export class RemoteInfo {
  /** The IP address of the remote endpoint. */
  address: string = "";

  /** The address family ('IPv4' or 'IPv6'). */
  family: string = "IPv4";

  /** The port number of the remote endpoint. */
  port: number = 0;

  /** The size of the message in bytes. */
  size: number = 0;
}
