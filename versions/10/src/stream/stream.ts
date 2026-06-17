/**
 * Base class for all streams. A stream is an abstract interface for working
 * with streaming data.
 *
 */
import { EventEmitter } from "../events-module.ts";
import type { RuntimeValue } from "../runtime-value.ts";

export interface PipeOptions {
  readonly end?: boolean;
}

export class Stream extends EventEmitter {
  write(
    _chunk: RuntimeValue,
    _encoding?: string,
    _callback?: () => void,
  ): boolean {
    return true;
  }

  end(
    _chunk?: RuntimeValue,
    _encoding?: string,
    _callback?: () => void,
  ): Stream {
    return this;
  }

  /**
   * Pipes the output of this readable stream into a writable stream
   * destination.
   *
   * @param destination - The destination writable stream.
   * @param options - Pipe options.
   * @param options.end - Whether to end the destination when this stream ends.
   *   Default is true.
   * @returns The destination stream.
   */
  pipe(
    destination: Stream,
    options?: PipeOptions,
  ): Stream {
    const end = options?.end !== false;

    this.on("data", (...args: RuntimeValue[]) => {
      destination.write(args[0]!);
    });

    if (end) {
      this.on("end", (..._args: RuntimeValue[]) => {
        destination.end();
      });
    }

    this.on("error", (...args: RuntimeValue[]) => {
      destination.emit("error", args[0]!);
    });

    this.resume();

    return destination;
  }

  resume(): Stream {
    return this;
  }

  /**
   * Destroys the stream and optionally emits an error event.
   *
   * @param error - Optional error to emit.
   */
  destroy(error?: Error): void {
    if (error !== undefined) {
      this.emit("error", error);
    }

    this.emit("close");
  }
}
