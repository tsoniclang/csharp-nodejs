/**
 * Utility functions for working with streams.
 *
 */
import { Stream } from "./stream.ts";
import type { RuntimeValue } from "../runtime-value.ts";

type PipelineCallback = (error: Error | undefined) => void;

const coerceError = (value: RuntimeValue | undefined): Error => {
  if (value instanceof Error) {
    return value;
  }

  return new Error("Unknown stream error");
};

export const pipelineStreams = (
  streamList: Stream[],
  callback?: PipelineCallback,
): void => {
  if (streamList.length < 2) {
    throw new Error(
      "pipeline requires at least a source and destination stream",
    );
  }

  let completed = false;
  const finish = (error: Error | undefined): void => {
    if (completed) {
      return;
    }
    completed = true;
    callback?.(error);
  };

  try {
    for (let i = 0; i < streamList.length - 1; i += 1) {
      const source = streamList[i]!;
      const dest = streamList[i + 1]!;

      source.on("error", (...errorArgs: RuntimeValue[]) => {
        const error = coerceError(errorArgs[0]);

        for (let j = i; j < streamList.length; j += 1) {
          streamList[j]!.destroy(error);
        }

        finish(error);
      });

      source.pipe(dest, {
        end: i === streamList.length - 2,
      });
    }

    const lastStream = streamList[streamList.length - 1]!;
    lastStream.on("finish", (..._args: RuntimeValue[]) => {
      finish(undefined);
    });
    lastStream.on("end", (..._args: RuntimeValue[]) => {
      finish(undefined);
    });
    lastStream.on("error", (...errorArgs: RuntimeValue[]) => {
      const error = coerceError(errorArgs[0]);
      finish(error);
    });
  } catch {
    const error = new Error("Stream pipeline failed");
    for (const s of streamList) {
      try {
        s.destroy(error);
      } catch {
      }
    }
    finish(error);
  }
};

/**
 * A method to pipe between streams forwarding errors and properly cleaning up.
 *
 */
export const pipeline = (
  streamList: Stream[],
  callback?: PipelineCallback,
): void => {
  pipelineStreams(streamList, callback);
};

/**
 * A function to get notified when a stream is no longer readable, writable or
 * has experienced an error or a premature close event.
 *
 * @param stream - The stream to monitor.
 * @param callback - The callback to invoke when the stream is finished.
 */
export const finished = (
  stream: Stream,
  callback: (error: Error | undefined) => void,
): void => {
  let called = false;

  function onFinished(error: Error | undefined): void {
    if (called) {
      return;
    }
    called = true;

    stream.removeListener("finish", onFinish);
    stream.removeListener("end", onEnd);
    stream.removeListener("error", onError);
    stream.removeListener("close", onClose);

    callback(error);
  }

  function onFinish(..._args: RuntimeValue[]): void {
    onFinished(undefined);
  }

  function onEnd(..._args: RuntimeValue[]): void {
    onFinished(undefined);
  }

  function onError(...args: RuntimeValue[]): void {
    const error = coerceError(args[0]);
    onFinished(error);
  }

  function onClose(...args: RuntimeValue[]): void {
    onFinished(undefined);
  }

  stream.on("finish", onFinish);
  stream.on("end", onEnd);
  stream.on("error", onError);
  stream.on("close", onClose);
};
