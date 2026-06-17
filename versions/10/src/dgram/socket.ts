/**
 * DgramSocket — UDP socket for sending and receiving datagrams.
 *
 *
 * NOTE: All OS-level UDP interop is deferred with TODO markers.
 * The class shape faithfully mirrors the Node.js dgram.Socket API.
 */

import { overloads as O } from "@tsonic/core/lang.js";
import { EventEmitter } from "../events-module.ts";
import type { int } from "@tsonic/core/types.js";
import { stringToBytes } from "../buffer/buffer-encoding.ts";
import { errorFromUnknown } from "../error-from-unknown.ts";
import { RemoteInfo } from "./remote-info.ts";
import { SocketOptions, BindOptions } from "./socket-options.ts";
import type { RuntimeValue } from "../runtime-value.ts";

/** Address information returned by socket.address() and socket.remoteAddress(). */
export class AddressInfo {
  address: string = "";
  family: string = "IPv4";
  port: int = 0;
}

export class DgramSocket extends EventEmitter {
  _type: string;
  _options: SocketOptions;
  _isBound: boolean = false;
  _isClosed: boolean = false;
  _isConnected: boolean = false;
  _localAddress: AddressInfo | undefined = undefined;
  _remoteAddress: AddressInfo | undefined = undefined;
  _recvBufferSize: int = 65536 as int;
  _sendBufferSize: int = 65536 as int;
  _sourceSpecificMemberships: string[] = [];
  _boundFileDescriptor: int | undefined = undefined;

  constructor(
    options: SocketOptions,
    callback?: (msg: Uint8Array, rinfo: RemoteInfo) => void,
  ) {
    super();

    this._type = options.type;
    this._options = options;

    if (callback !== undefined) {
      this.on("message", (...args: RuntimeValue[]) => {
        callback(args[0] as Uint8Array, args[1] as RemoteInfo);
      });
    }
  }

  /**
   * Returns an object containing the address information for a socket.
   */
  address(): AddressInfo {
    if (!this._isBound) {
      throw new Error("Socket is not bound");
    }

    // TODO: OS interop — read actual local endpoint from native UDP socket
    return this._localAddress!;
  }

  /**
   * Causes the socket to listen for datagram messages on a named port and optional address.
   */
  bind(): DgramSocket;
  bind(port: int, address?: string, callback?: () => void): DgramSocket;
  bind(port: int, callback: () => void): DgramSocket;
  bind(callback: () => void): DgramSocket;
  bind(options: BindOptions, callback?: () => void): DgramSocket;
  bind(
    _portOrCallbackOrOptions?: any,
    _addressOrCallback?: any,
    _callback?: any,
  ): any {
    throw new Error("Unreachable overload stub");
  }

  bind_empty(): DgramSocket {
    return this.bindSocket(0 as int, undefined, undefined);
  }

  bind_port_address(
    port: int,
    address?: string,
    callback?: () => void,
  ): DgramSocket {
    return this.bindSocket(port, address, callback);
  }

  bind_port_callback(port: int, callback: () => void): DgramSocket {
    return this.bindSocket(port, undefined, callback);
  }

  bind_callback(callback: () => void): DgramSocket {
    return this.bindSocket(0 as int, undefined, callback);
  }

  bind_options(options: BindOptions, callback?: () => void): DgramSocket {
    if (options.fd !== undefined) {
      this._boundFileDescriptor = options.fd;
    }
    return this.bindSocket(options.port ?? (0 as int), options.address, callback);
  }

  /**
   * Close the underlying socket and stop listening for data on it.
   */
  close(callback?: () => void): DgramSocket {
    if (this._isClosed) {
      return this;
    }

    this._isClosed = true;
    this._boundFileDescriptor = undefined;

    // TODO: OS interop — close and dispose native UDP socket

    this.emit("close");

    if (callback !== undefined) {
      callback();
    }

    return this;
  }

  /**
   * Associates the socket to a remote address and port.
   */
  connect(port: int, address?: string, callback?: () => void): void;
  connect(port: int, callback: () => void): void;
  connect(_port: any, _addressOrCallback?: any, _callback?: any): any {
    throw new Error("Unreachable overload stub");
  }

  connect_port_address(
    port: int,
    address?: string,
    callback?: () => void,
  ): void {
    this.connectSocket(port, address, callback);
  }

  connect_port_callback(port: int, callback: () => void): void {
    this.connectSocket(port, undefined, callback);
  }

  /**
   * Disassociates a connected socket from its remote address.
   */
  disconnect(): void {
    if (!this._isConnected) {
      throw new Error("Socket is not connected");
    }

    this._remoteAddress = undefined;
    this._isConnected = false;
  }

  /**
   * Broadcasts a datagram on the socket.
   */
  send(msg: Uint8Array | string, callback: (error: Error | null, bytes: number) => void): void;
  send(msg: Uint8Array | string, port: int, callback: (error: Error | null, bytes: number) => void): void;
  send(
    msg: Uint8Array | string,
    port: int,
    address: string,
    callback?: (error: Error | null, bytes: number) => void,
  ): void;
  send(msg: Uint8Array, offset: int, length: int, callback?: (error: Error | null, bytes: number) => void): void;
  send(
    msg: Uint8Array,
    offset: int,
    length: int,
    port: int,
    callback?: (error: Error | null, bytes: number) => void,
  ): void;
  send(
    msg: Uint8Array,
    offset: int,
    length: int,
    port: int,
    address: string,
    callback?: (error: Error | null, bytes: number) => void,
  ): void;
  send(
    msg: any,
    arg1?: any,
    arg2?: any,
    arg3?: any,
    arg4?: any,
    arg5?: any,
  ): any {
    throw new Error("Unreachable overload stub");
  }

  send_message_callback(
    msg: Uint8Array | string,
    callback: (error: Error | null, bytes: number) => void,
  ): void {
    this.sendPrepared(toBytes(msg), undefined, undefined, callback);
  }

  send_message_port_callback(
    msg: Uint8Array | string,
    port: int,
    callback: (error: Error | null, bytes: number) => void,
  ): void {
    this.sendPrepared(toBytes(msg), port, undefined, callback);
  }

  send_message_port_address(
    msg: Uint8Array | string,
    port: int,
    address: string,
    callback?: (error: Error | null, bytes: number) => void,
  ): void {
    this.sendPrepared(toBytes(msg), port, address, callback);
  }

  send_buffer_offset_length(
    msg: Uint8Array,
    offset: int,
    length: int,
    callback?: (error: Error | null, bytes: number) => void,
  ): void {
    const data = copyRange(msg, offset, toInt32(offset + length));
    this.sendPrepared(data, undefined, undefined, callback);
  }

  send_buffer_offset_length_port(
    msg: Uint8Array,
    offset: int,
    length: int,
    port: int,
    callback?: (error: Error | null, bytes: number) => void,
  ): void {
    const data = copyRange(msg, offset, toInt32(offset + length));
    this.sendPrepared(data, port, undefined, callback);
  }

  send_buffer_offset_length_port_address(
    msg: Uint8Array,
    offset: int,
    length: int,
    port: int,
    address: string,
    callback?: (error: Error | null, bytes: number) => void,
  ): void {
    const data = copyRange(msg, offset, toInt32(offset + length));
    this.sendPrepared(data, port, address, callback);
  }

  bindSocket(
    port: int,
    address: string | undefined,
    callback?: () => void,
  ): DgramSocket {
    if (this._isBound) {
      throw new Error("Socket is already bound");
    }

    try {
      // TODO: OS interop — create native UDP socket and bind to port/address
      // Apply socket options: _options.reuseAddr, _options.recvBufferSize, _options.sendBufferSize
      // Determine address family from _type ("udp4" → IPv4, "udp6" → IPv6)

      const bindAddress =
        address !== undefined && address !== null
          ? address
          : this._type === "udp6"
            ? "::"
            : "0.0.0.0";

      this._localAddress = new AddressInfo();
      this._localAddress.address = bindAddress;
      this._localAddress.family = this._type === "udp6" ? "IPv6" : "IPv4";
      this._localAddress.port = port;
      this._isBound = true;

      // TODO: OS interop — start receiving messages on background thread

      this.emit("listening");

      if (callback !== undefined) {
        callback();
      }
    } catch (ex) {
      const error: Error = errorFromUnknown(ex, "Failed to bind UDP socket");
      this.emit("error", error);
    }

    return this;
  }

  connectSocket(
    port: int,
    address: string | undefined,
    callback?: () => void,
  ): void {
    if (this._isConnected) {
      throw new Error("Socket is already connected");
    }

    try {
      if (address === undefined || address === null) {
        address = this._type === "udp6" ? "::1" : "127.0.0.1";
      }

      // Auto-bind if not already bound
      if (!this._isBound) {
        this.bind_empty();
      }

      // TODO: OS interop — connect native UDP socket to remote endpoint

      this._remoteAddress = new AddressInfo();
      this._remoteAddress.address = address;
      this._remoteAddress.family = this._type === "udp6" ? "IPv6" : "IPv4";
      this._remoteAddress.port = port;
      this._isConnected = true;

      this.emit("connect");

      if (callback !== undefined) {
        callback();
      }
    } catch (ex) {
      const error: Error = errorFromUnknown(ex, "Failed to connect UDP socket");
      this.emit("error", error);

      if (callback !== undefined) {
        callback();
      }
    }
  }
  
  sendPrepared(
    data: Uint8Array,
    port: number | undefined,
    address: string | undefined,
    callback: ((error: Error | null, bytes: number) => void) | undefined,
  ): void {
    try {
      if (!this._isBound) {
        this.bind_empty();
      }

      const bytesSent: number = data.length;

      if (this._isConnected) {
      } else {
        if (port === undefined) {
          throw new Error("Port must be specified for unconnected socket");
        }
      }

      if (callback !== undefined) {
        callback(null, bytesSent);
      }
    } catch (ex) {
      const error: Error = errorFromUnknown(ex, "Failed to send UDP datagram");
      this.emit("error", error);

      if (callback !== undefined) {
        callback(error, 0);
      }
    }
  }

  /**
   * Sets or clears the SO_BROADCAST socket option.
   */
  setBroadcast(flag: boolean): void {
    if (!this._isBound) {
      throw new Error("Socket is not bound");
    }

    // TODO: OS interop — set SO_BROADCAST on native socket
    void flag;
  }

  /**
   * Sets the IP_MULTICAST_TTL socket option.
   */
  setMulticastTTL(ttl: int): int {
    if (!this._isBound) {
      throw new Error("Socket is not bound");
    }

    if (ttl < 0 || ttl > 255) {
      throw new Error("TTL must be between 0 and 255");
    }

    // TODO: OS interop — set IP_MULTICAST_TTL on native socket
    return ttl;
  }

  /**
   * Sets or clears the IP_MULTICAST_LOOP socket option.
   */
  setMulticastLoopback(flag: boolean): boolean {
    if (!this._isBound) {
      throw new Error("Socket is not bound");
    }

    // TODO: OS interop — set IP_MULTICAST_LOOP on native socket
    return flag;
  }

  /**
   * Tells the kernel to join a multicast group.
   */
  addMembership(multicastAddress: string, multicastInterface?: string): void {
    if (!this._isBound) {
      this.bind_empty();
    }

    // TODO: OS interop — join multicast group on native socket
    void multicastAddress;
    void multicastInterface;
  }

  /**
   * Instructs the kernel to leave a multicast group.
   */
  dropMembership(multicastAddress: string): void {
    if (!this._isBound) {
      throw new Error("Socket is not bound");
    }

    // TODO: OS interop — leave multicast group on native socket
    void multicastAddress;
  }

  /**
   * Sets the default outgoing multicast interface of the socket.
   */
  setMulticastInterface(multicastInterface: string): void {
    if (!this._isBound) {
      throw new Error("Socket is not bound");
    }

    // TODO: OS interop — set multicast interface on native socket
    void multicastInterface;
  }

  /**
   * Sets the IP_TTL socket option.
   */
  setTTL(ttl: int): int {
    if (!this._isBound) {
      throw new Error("Socket is not bound");
    }

    if (ttl < 1 || ttl > 255) {
      throw new Error("TTL must be between 1 and 255");
    }

    // TODO: OS interop — set IP_TTL on native socket
    return ttl;
  }

  /**
   * Gets the number of bytes queued for sending.
   * Note: Returns 0 as a stub — not available without native socket.
   */
  getSendQueueSize(): int {
    return 0;
  }

  /**
   * Gets the number of send requests currently in the queue.
   * Note: Returns 0 as a stub — not available without native socket.
   */
  getSendQueueCount(): int {
    return 0;
  }

  /**
   * Adds the socket back to reference counting.
   * Note: No-op for API compatibility.
   */
  ref(): DgramSocket {
    return this;
  }

  /**
   * Excludes the socket from reference counting.
   * Note: No-op for API compatibility.
   */
  unref(): DgramSocket {
    return this;
  }

  /**
   * Tells the kernel to join a source-specific multicast channel.
   */
  addSourceSpecificMembership(
    sourceAddress: string,
    groupAddress: string,
    multicastInterface?: string,
  ): void {
    if (!this._isBound) {
      this.bind_empty();
    }

    const key = buildSourceMembershipKey(
      sourceAddress,
      groupAddress,
      multicastInterface,
    );

    if (this._sourceSpecificMemberships.indexOf(key) < 0) {
      this._sourceSpecificMemberships.push(key);
    }
  }

  /**
   * Instructs the kernel to leave a source-specific multicast channel.
   */
  dropSourceSpecificMembership(
    sourceAddress: string,
    groupAddress: string,
    multicastInterface?: string,
  ): void {
    if (!this._isBound) {
      throw new Error("Socket is not bound");
    }

    const key = buildSourceMembershipKey(
      sourceAddress,
      groupAddress,
      multicastInterface,
    );
    const index = this._sourceSpecificMemberships.indexOf(key);
    if (index >= 0) {
      this._sourceSpecificMemberships.splice(index, 1);
    }
  }

  /**
   * Sets the SO_RCVBUF socket receive buffer size.
   */
  setRecvBufferSize(size: int): void {
    if (!this._isBound) {
      throw new Error("Socket is not bound");
    }

    this._recvBufferSize = size;
  }

  /**
   * Sets the SO_SNDBUF socket send buffer size.
   */
  setSendBufferSize(size: int): void {
    if (!this._isBound) {
      throw new Error("Socket is not bound");
    }

    this._sendBufferSize = size;
  }

  /**
   * Gets the SO_RCVBUF socket receive buffer size.
   */
  getRecvBufferSize(): int {
    if (!this._isBound) {
      throw new Error("Socket is not bound");
    }

    return this._recvBufferSize;
  }

  /**
   * Gets the SO_SNDBUF socket send buffer size.
   */
  getSendBufferSize(): int {
    if (!this._isBound) {
      throw new Error("Socket is not bound");
    }

    return this._sendBufferSize;
  }

  /**
   * Returns the remote endpoint information.
   */
  remoteAddress(): AddressInfo {
    if (!this._isConnected || this._remoteAddress === undefined) {
      throw new Error("Socket is not connected");
    }

    return this._remoteAddress;
  }
}

O<DgramSocket>().method(x => x.bind_empty).family(x => x.bind);
O<DgramSocket>().method(x => x.bind_port_address).family(x => x.bind);
O<DgramSocket>().method(x => x.bind_port_callback).family(x => x.bind);
O<DgramSocket>().method(x => x.bind_callback).family(x => x.bind);
O<DgramSocket>().method(x => x.bind_options).family(x => x.bind);
O<DgramSocket>().method(x => x.connect_port_address).family(x => x.connect);
O<DgramSocket>().method(x => x.connect_port_callback).family(x => x.connect);
O<DgramSocket>().method(x => x.send_message_callback).family(x => x.send);
O<DgramSocket>().method(x => x.send_message_port_callback).family(x => x.send);
O<DgramSocket>().method(x => x.send_message_port_address).family(x => x.send);
O<DgramSocket>().method(x => x.send_buffer_offset_length).family(x => x.send);
O<DgramSocket>().method(x => x.send_buffer_offset_length_port).family(x => x.send);
O<DgramSocket>().method(x => x.send_buffer_offset_length_port_address).family(x => x.send);

const toInt32 = (value: number): int => {
  if (
    Number.isInteger(value) &&
    value >= -2147483648 &&
    value <= 2147483647
  ) {
    return value as int;
  }

  throw new RangeError("Expected Int32-compatible numeric value");
};

const copyRange = (data: Uint8Array, start: int, end: int): Uint8Array => {
  const result = new Uint8Array(end - start);
  for (let index = 0; index < result.length; index += 1) {
    result[index] = data[start + index]!;
  }
  return result;
};

const toBytes = (msg: Uint8Array | string): Uint8Array => {
  if (msg instanceof Uint8Array) {
    return msg;
  }

  return stringToBytes(msg as string, "utf8");
};

const buildSourceMembershipKey = (
  sourceAddress: string,
  groupAddress: string,
  multicastInterface?: string,
): string =>
  `${sourceAddress}|${groupAddress}|${multicastInterface ?? ""}`;
