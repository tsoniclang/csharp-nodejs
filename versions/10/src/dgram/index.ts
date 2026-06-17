/**
 * Node.js dgram module — UDP datagram sockets.
 *
 */

import type {} from "../type-bootstrap.ts";

export { RemoteInfo } from "./remote-info.ts";
export { SocketOptions, BindOptions } from "./socket-options.ts";
export { DgramSocket, AddressInfo } from "./socket.ts";

import { DgramSocket } from "./socket.ts";
import { SocketOptions } from "./socket-options.ts";
import { RemoteInfo } from "./remote-info.ts";

/**
 * Creates a dgram.Socket object. The type argument can be either 'udp4' or 'udp6'.
 */
export const createSocket = (
  typeOrOptions: string | SocketOptions,
  callback?: (msg: Uint8Array, rinfo: RemoteInfo) => void,
): DgramSocket => {
  if (typeOrOptions instanceof SocketOptions) {
    return new DgramSocket(typeOrOptions, callback);
  }
  const options = new SocketOptions();
  options.type = typeOrOptions;
  return new DgramSocket(options, callback);
};
