/**
 * net.Socket — TCP socket abstraction.
 *
 *
 * This is substrate-heavy: actual TCP I/O requires OS interop via .NET sockets.
 * Method signatures are correct; implementations that require OS interop use
 * TODO placeholders.
 */
import { overloads as O } from "@tsonic/core/lang.js";
import type { int } from "@tsonic/core/types.js";
import { EventEmitter, toEventListener } from "../events-module.ts";
import { AddressInfo } from "./options.ts";
import type { SocketConstructorOpts, TcpSocketConnectOpts } from "./options.ts";

/**
 * This class is an abstraction of a TCP socket or a streaming IPC endpoint.
 * It is also an EventEmitter.
 */
export class Socket extends EventEmitter {
  _connecting: boolean = false;
  _connected: boolean = false;
  _destroyed: boolean = false;
  _bytesRead: number = 0;
  _bytesWritten: number = 0;
  _localAddress: string | undefined = undefined;
  _localPort: int | undefined = undefined;
  _localFamily: string | undefined = undefined;
  _remoteAddress: string | undefined = undefined;
  _remotePort: int | undefined = undefined;
  _remoteFamily: string | undefined = undefined;
  _allowHalfOpen: boolean;

  /**
   * The amount of received bytes.
   */
  get bytesRead(): number {
    return this._bytesRead;
  }

  /**
   * The amount of bytes sent.
   */
  get bytesWritten(): number {
    return this._bytesWritten;
  }

  /**
   * Whether the connection is active.
   */
  get connecting(): boolean {
    return this._connecting;
  }

  /**
   * Whether the socket has been destroyed.
   */
  get destroyed(): boolean {
    return this._destroyed;
  }

  /**
   * The string representation of the local IP address.
   */
  get localAddress(): string | undefined {
    return this._localAddress;
  }

  /**
   * The numeric representation of the local port.
   */
  get localPort(): number | undefined {
    return this._localPort;
  }

  /**
   * The string representation of the local IP family.
   */
  get localFamily(): string | undefined {
    return this._localFamily;
  }

  /**
   * The string representation of the remote IP address.
   */
  get remoteAddress(): string | undefined {
    return this._remoteAddress;
  }

  /**
   * The numeric representation of the remote port.
   */
  get remotePort(): number | undefined {
    return this._remotePort;
  }

  /**
   * The string representation of the remote IP family.
   */
  get remoteFamily(): string | undefined {
    return this._remoteFamily;
  }

  /**
   * This property represents the state of the connection as a string.
   */
  get readyState(): string {
    if (this._destroyed) {
      return "closed";
    }
    if (this._connecting) {
      return "opening";
    }
    return this._connected ? "open" : "closed";
  }

  constructor(options?: SocketConstructorOpts) {
    super();
    this._allowHalfOpen = options?.allowHalfOpen ?? false;
  }

  /**
   * Initiate a connection on a given socket.
   */
  connect(
    port: number,
    host?: string,
    connectionListener?: () => void
  ): Socket;
  connect(
    options: TcpSocketConnectOpts,
    connectionListener?: () => void
  ): Socket;
  connect(path: string, connectionListener?: () => void): Socket;
  connect(
    _portOrOptionsOrPath: any,
    _hostOrListener?: any,
    _connectionListener?: any
  ): any {
    throw new Error("Unreachable overload stub");
  }

  connect_port(
    port: number,
    host?: string,
    connectionListener?: () => void
  ): Socket {
    return this.connectPort(port, host, connectionListener);
  }

  connect_options(
    options: TcpSocketConnectOpts,
    connectionListener?: () => void
  ): Socket {
    return this.connectPort(options.port, options.host, connectionListener);
  }

  connect_path(
    path: string,
    connectionListener?: () => void
  ): Socket {
    return this.connectPath(path, connectionListener);
  }

  connectPort(
    port: number,
    host: string | undefined,
    connectionListener: (() => void) | undefined
  ): Socket {
    if (connectionListener !== undefined) {
      this.once("connect", toEventListener(connectionListener)!);
    }

    this._connecting = true;

    // TODO: Initiate async TCP connection via OS interop
    // In the CLR version this uses TcpClient.ConnectAsync.
    // On connect success:
    //   this._connecting = false;
    //   update address info from the underlying socket
    //   this.emit("connect");
    //   this.emit("ready");
    //   start the read loop

    return this;
  }

  connectPath(
    path: string,
    connectionListener: (() => void) | undefined
  ): Socket {
    if (connectionListener !== undefined) {
      this.once("connect", toEventListener(connectionListener)!);
    }

    this._connecting = false;
    this._connected = true;
    this._destroyed = false;
    this._remoteAddress = path;
    this._remoteFamily = "IPC";
    this._remotePort = undefined;
    this.emit("connect");
    this.emit("ready");
    return this;
  }

  /**
   * Sends data on the socket (byte array form).
   */
  write(
    data: Uint8Array,
    callback?: (err?: Error) => void
  ): boolean;
  /**
   * Sends string data on the socket.
   */
  write(
    data: string,
    encoding?: string,
    callback?: (err?: Error) => void
  ): boolean;
  write(
    _data: any,
    _encodingOrCallback?: any,
    _callback?: any
  ): any {
    throw new Error("Unreachable overload stub");
  }

  write_bytes(
    _data: Uint8Array,
    callback?: (err?: Error) => void
  ): boolean {
    if (this._destroyed) {
      if (callback !== undefined) {
        callback(new Error("Socket not connected"));
      }
      return false;
    }

    // TODO: Queue write data and flush via OS interop (NetworkStream.WriteAsync)
    // In the CLR version this uses a BlockingCollection write queue with FIFO ordering.
    // On write success: update this._bytesWritten, invoke callback, emit("drain")

    return true;
  }

  write_string(
    _data: string,
    _encoding?: string,
    callback?: (err?: Error) => void
  ): boolean {
    if (this._destroyed) {
      if (callback !== undefined) {
        callback(new Error("Socket not connected"));
      }
      return false;
    }

    // TODO: Queue write data and flush via OS interop (NetworkStream.WriteAsync)
    // In the CLR version this uses a BlockingCollection write queue with FIFO ordering.
    // On write success: update this._bytesWritten, invoke callback, emit("drain")

    return true;
  }

  /**
   * Half-closes the socket.
   */
  end(callback?: () => void): Socket {
    this.emit("end");
    if (callback !== undefined) {
      callback();
    }

    return this;
  }

  /**
   * Ensures that no more I/O activity happens on this socket.
   */
  destroy(error?: Error): Socket {
    if (this._destroyed) {
      return this;
    }

    this._destroyed = true;
    this._connected = false;
    this._connecting = false;

    // TODO: Close the underlying TCP client and NetworkStream via OS interop

    this.emit("close", error !== undefined);
    if (error !== undefined) {
      this.emit("error", error);
    }

    return this;
  }

  /**
   * Destroys the socket after all data is written.
   */
  destroySoon(): void {
    // TODO: Wait for write queue to drain, then call destroy()
    this.destroy();
  }

  /**
   * Close the TCP connection by sending an RST packet.
   */
  resetAndDestroy(): Socket {
    // TODO: Close the underlying socket with linger=0 for RST via OS interop
    return this.destroy();
  }

  /**
   * Set the encoding for the socket as a Readable Stream.
   */
  setEncoding(_encoding?: string): Socket {
    // TODO: Encoding handling with full stream support
    return this;
  }

  /**
   * Pauses the reading of data.
   */
  pause(): Socket {
    // TODO: Pause the async read loop
    return this;
  }

  /**
   * Resumes reading after a call to socket.pause().
   */
  resume(): Socket {
    // TODO: Resume the async read loop
    return this;
  }

  /**
   * Sets the socket to timeout after timeout milliseconds of inactivity.
   */
  setTimeout(timeout: number, callback?: () => void): Socket {
    if (callback !== undefined) {
      this.once("timeout", toEventListener(callback)!);
    }

    // TODO: Configure read/write timeouts on the underlying NetworkStream via OS interop

    return this;
  }

  /**
   * Enable/disable the use of Nagle's algorithm.
   */
  setNoDelay(_noDelay: boolean = true): Socket {
    // TODO: Set TcpClient.NoDelay via OS interop
    return this;
  }

  /**
   * Enable/disable keep-alive functionality.
   */
  setKeepAlive(
    _enable: boolean = false,
    _initialDelay: number = 0
  ): Socket {
    // TODO: Set socket KeepAlive option via OS interop
    return this;
  }

  /**
   * Returns the bound address, the address family name and port of the socket.
   */
  address(): AddressInfo | null {
    if (
      this._localAddress !== undefined &&
      this._localPort !== undefined &&
      this._localFamily !== undefined
    ) {
      const info = new AddressInfo();
      info.address = this._localAddress;
      info.family = this._localFamily;
      info.port = this._localPort;
      return info;
    }
    return null;
  }

  /**
   * Calling unref() on a socket will allow the program to exit if this is the only active socket.
   */
  unref(): Socket {
    // Not applicable in managed context
    return this;
  }

  /**
   * Opposite of unref().
   */
  ref(): Socket {
    // Not applicable in managed context
    return this;
  }
}

O<Socket>().method(x => x.connect_port).family(x => x.connect);
O<Socket>().method(x => x.connect_options).family(x => x.connect);
O<Socket>().method(x => x.connect_path).family(x => x.connect);
O<Socket>().method(x => x.write_bytes).family(x => x.write);
O<Socket>().method(x => x.write_string).family(x => x.write);
