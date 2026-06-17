/**
 * Promise-based stream helpers.
 *
 */
import { Stream } from "./stream.ts";
import {
  finished as finishedCb,
  pipelineStreams,
} from "./utilities.ts";

/**
 * Promise-based pipeline. Pipes streams together and returns a Promise that
 * resolves when the pipeline completes.
 */
export const pipeline = (...streams: Stream[]): Promise<void> => {
  if (streams.length < 2) {
    throw new Error("pipeline requires at least two streams");
  }

  return new Promise<void>((resolve, reject) => {
    pipelineStreams(streams, (error: Error | undefined) => {
      if (error !== undefined) {
        reject(error);
      } else {
        resolve();
      }
    });
  });
};

/**
 * Promise-based finished. Returns a Promise that resolves when the stream is
 * no longer readable, writable or has experienced an error.
 */
export const finished = (stream: Stream): Promise<void> => {
  return new Promise<void>((resolve, reject) => {
    finishedCb(stream, (error) => {
      if (error !== undefined) {
        reject(error);
      } else {
        resolve();
      }
    });
  });
};
