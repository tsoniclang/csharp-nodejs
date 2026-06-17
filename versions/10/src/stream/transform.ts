/**
 * Transform streams are Duplex streams where the output is computed from the
 * input.
 *
 */
import { Duplex } from "./duplex.ts";
import type { RuntimeValue } from "../runtime-value.ts";

export class Transform extends Duplex {
  /**
   * Internal method to be implemented by subclasses to transform data.
   *
   * @param chunk - Chunk of data to transform.
   * @param encoding - Encoding if chunk is a string.
   * @param callback - Callback for when transform is complete. Call with
   *   (error, data).
   */
  _transform(
    chunk: RuntimeValue,
    encoding: string | undefined,
    callback: (error: Error | null, data: RuntimeValue | null | undefined) => void,
  ): void {
    // Default implementation: pass through
    callback(null, chunk);
  }

  /**
   * Internal method called right before the stream closes, to process any
   * remaining data.
   *
   * @param callback - Callback for when flush is complete.
   */
  _flush(callback: (error: Error | null) => void): void {
    // To be implemented by subclasses
    callback(null);
  }

  /**
   * Internal method to write data to the transform stream.
   *
   * @param chunk - Chunk of data to write.
   * @param encoding - Encoding if chunk is a string.
   * @param callback - Callback for when write is complete.
   */
  override _write(
    chunk: RuntimeValue,
    encoding: string | undefined,
    callback: () => void,
  ): void {
    this._transform(chunk, encoding, (error, data) => {
      if (error !== null) {
        this.emit("error", error);
        callback();
        return;
      }

      if (data !== null && data !== undefined) {
        this.push(data);
      }

      callback();
    });
  }
}
