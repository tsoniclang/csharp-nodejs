/**
 * A writable stream is an abstraction for a destination to which data is
 * written.
 *
 */
import { Stream } from "./stream.ts";
import type { RuntimeValue } from "../runtime-value.ts";

type WriteRequest = {
  chunk: RuntimeValue;
  encoding: string | undefined;
  callback: (() => void) | undefined;
};

export class Writable extends Stream {
  _buffer: WriteRequest[] = [];
  _ended = false;
  _writing = false;
  _corked = false;
  _destroyed = false;

  /** Is true if it is safe to call write(). */
  get writable(): boolean {
    return !this._ended && !this._destroyed;
  }

  /** Is true after writable.end() has been called. */
  get writableEnded(): boolean {
    return this._ended;
  }

  /** Is true after destroy() has been called. */
  get destroyed(): boolean {
    return this._destroyed;
  }

  /** Number of bytes (or objects) in the write queue ready to be written. */
  get writableLength(): number {
    return this._buffer.length;
  }

  /** Is true if the stream's buffer has been corked. */
  get writableCorked(): boolean {
    return this._corked;
  }

  /**
   * Writes data to the stream.
   *
   * @param chunk - The data to write.
   * @param encoding - The encoding if chunk is a string.
   * @param callback - Callback for when this chunk of data is flushed.
   * @returns False if the stream wishes for the calling code to wait for the
   *   'drain' event to be emitted before continuing to write.
   */
  override write(
    chunk: RuntimeValue,
    encoding?: string,
    callback?: () => void,
  ): boolean {
    if (this._ended) {
      throw new Error("write after end");
    }

    const request: WriteRequest = {
      chunk,
      encoding,
      callback,
    };

    this._buffer.push(request);

    if (!this._corked) {
      this.processWrites();
    }

    // Simplified: always return true (no backpressure handling in basic
    // implementation)
    return true;
  }

  /**
   * Signals that no more data will be written to the Writable.
   *
   * @param chunk - Optional data to write before ending.
   * @param encoding - The encoding if chunk is a string.
   * @param callback - Optional callback for when the stream has finished.
   */
  override end(
    chunk?: RuntimeValue,
    encoding?: string,
    callback?: () => void,
  ): Writable {
    if (chunk !== undefined && chunk !== null) {
      this.write(chunk, encoding);
    }

    if (callback !== undefined) {
      this.once("finish", (..._args: RuntimeValue[]) => {
        callback();
      });
    }

    this._ended = true;

    if (!this._corked) {
      this.processWrites();
    }

    if (this._buffer.length === 0) {
      this.emit("finish");
    }

    return this;
  }

  /**
   * Forces all written data to be buffered in memory. The buffered data will
   * be flushed when uncork() is called.
   */
  cork(): void {
    this._corked = true;
  }

  /**
   * Flushes all data buffered since cork() was called.
   */
  uncork(): void {
    this._corked = false;
    this.processWrites();
  }

  /**
   * Destroys the stream.
   *
   * @param error - Optional error to emit.
   */
  override destroy(error?: Error): void {
    if (this._destroyed) {
      return;
    }

    this._destroyed = true;
    while (this._buffer.length > 0) {
      this._buffer.pop();
    }

    super.destroy(error);
  }

  processWrites(): void {
    if (this._writing || this._buffer.length === 0) {
      return;
    }

    this._writing = true;

    while (this._buffer.length > 0) {
      const request = this._buffer.shift()!;
      this._write(request.chunk, request.encoding, () => {
        if (request.callback !== undefined) {
          request.callback();
        }
      });
    }

    this._writing = false;

    if (this._ended && this._buffer.length === 0) {
      this.emit("finish");
    }
  }

  /**
   * Internal method to be implemented by subclasses to write data.
   *
   * @param _chunk - Chunk of data to write.
   * @param _encoding - Encoding if chunk is a string.
   * @param callback - Callback for when write is complete.
   */
  _write(
    _chunk: RuntimeValue,
    _encoding: string | undefined,
    callback: () => void,
  ): void {
    // To be implemented by subclasses
    callback();
  }

  /**
   * Internal method called right before the stream closes.
   *
   * @param callback - Callback for when finalize is complete.
   */
  _final(callback: () => void): void {
    // To be implemented by subclasses
    callback();
  }
}
