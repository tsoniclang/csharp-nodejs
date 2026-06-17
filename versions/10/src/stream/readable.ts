/**
 * A readable stream is an abstraction for a source from which data is read.
 *
 */
import type { int } from "@tsonic/core/types.js";

import { Stream } from "./stream.ts";
import type { RuntimeValue } from "../runtime-value.ts";

export class Readable extends Stream {
  _buffer: RuntimeValue[] = [];
  _ended = false;
  _flowing = false;
  _encoding: string | undefined;
  _paused = true;
  _destroyed = false;

  /** Is true if it is safe to call read(). */
  get readable(): boolean {
    return !this._ended && !this._destroyed;
  }

  /** Becomes true when 'end' event is emitted. */
  get readableEnded(): boolean {
    return this._ended;
  }

  /** Current operating state of the Readable stream. */
  get readableFlowing(): boolean | null {
    if (this._flowing) {
      return true;
    }
    if (this._paused) {
      return false;
    }
    return null;
  }

  /**
   * Number of bytes (or objects) in the queue ready to be read.
   */
  get readableLength(): int {
    return this._buffer.length;
  }

  /** Is true after destroy() has been called. */
  get destroyed(): boolean {
    return this._destroyed;
  }

  /**
   * Reads data out of the internal buffer and returns it.
   *
   * @param _size - Optional argument to specify how much data to read.
   * @returns The data read, or null if no data is available.
   */
  read(_size?: int): RuntimeValue | null {
    if (this._buffer.length === 0) {
      if (this._ended) {
        this.emit("end");
      }
      return null;
    }

    const chunk = this._buffer.shift();

    if (this._buffer.length === 0 && this._ended) {
      this.emit("end");
    }

    return chunk === undefined ? null : chunk;
  }

  /**
   * Sets the character encoding for data read from the Readable stream.
   *
   * @param encoding - The encoding to use.
   * @returns This stream.
   */
  setEncoding(encoding: string): Readable {
    this._encoding = encoding;
    return this;
  }

  /**
   * Causes a stream in flowing mode to stop emitting 'data' events,
   * switching out of flowing mode.
   *
   * @returns This stream.
   */
  pause(): Readable {
    this._paused = true;
    this._flowing = false;
    return this;
  }

  /**
   * Causes an explicitly paused Readable stream to resume emitting 'data'
   * events, switching the stream into flowing mode.
   *
   * @returns This stream.
   */
  override resume(): Readable {
    this._paused = false;
    this._flowing = true;

    // Emit buffered data
    while (this._buffer.length > 0) {
      const chunk = this._buffer.shift();
      if (chunk !== undefined) {
        this.emit("data", chunk);
      }
    }

    if (this._ended) {
      this.emit("end");
    }

    return this;
  }

  /**
   * Returns the current operating state of the Readable.
   *
   * @returns True if the stream is paused.
   */
  isPaused(): boolean {
    return this._paused;
  }

  /**
   * Detaches a Writable stream previously attached using pipe().
   *
   * @param _destination - Optional specific stream to unpipe.
   * @returns This stream.
   */
  unpipe(_destination?: Stream): Readable {
    // TODO: Full implementation needs to track piped destinations
    return this;
  }

  /**
   * Pushes a chunk of data back into the front of the internal buffer.
   *
   * @param chunk - Chunk of data to unshift onto the read queue.
   */
  unshift(chunk: RuntimeValue): void {
    if (chunk !== null && chunk !== undefined) {
      this._buffer.unshift(chunk);
    }
  }

  /**
   * Pushes a chunk of data into the internal buffer. Can be called by
   * subclasses.
   *
   * @param chunk - Chunk of data to push. Pushing null signals end of stream.
   * @param _encoding - Optional encoding for string chunks.
   * @returns True if the internal buffer has not exceeded highWaterMark.
   */
  push(chunk: RuntimeValue | null, _encoding?: string): boolean {
    if (chunk === null) {
      // Pushing null signals end of stream
      this._ended = true;
      if (this._flowing) {
        this.emit("end");
      }
      return false;
    }

    this._buffer.push(chunk);

    if (this._flowing) {
      // In flowing mode, emit data immediately
      while (this._buffer.length > 0) {
        const data = this._buffer.shift();
        if (data !== undefined) {
          this.emit("data", data);
        }
      }

      if (this._ended) {
        this.emit("end");
      }
    } else {
      // In paused mode, emit 'readable' event
      this.emit("readable");
    }

    return true;
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

  /**
   * Internal method to be implemented by subclasses to read data.
   *
   * @param _size - Number of bytes to read.
   */
  _read(_size: int): void {
    // To be implemented by subclasses
  }
}
