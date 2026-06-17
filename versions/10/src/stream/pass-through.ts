/**
 * PassThrough streams are a trivial implementation of a Transform stream that
 * simply passes the input bytes across to the output.
 *
 */
import { Transform } from "./transform.ts";
import type { RuntimeValue } from "../runtime-value.ts";

export class PassThrough extends Transform {
  /**
   * Transforms data by passing it through unchanged.
   *
   * @param chunk - Chunk of data to transform.
   * @param _encoding - Encoding if chunk is a string.
   * @param callback - Callback for when transform is complete.
   */
  override _transform(
    chunk: RuntimeValue,
    _encoding: string | undefined,
    callback: (error: Error | null, data: RuntimeValue | null | undefined) => void,
  ): void {
    // Just pass the data through unchanged
    callback(null, chunk);
  }
}
