/**
 * Node.js stream module.
 *
 */

import type {} from "../type-bootstrap.ts";

import type { Stream } from "./stream.ts";
import {
  finished as finishedPromise,
  pipeline as pipelinePromise,
} from "./promises.ts";

export { Stream } from "./stream.ts";
export { Readable } from "./readable.ts";
export { Writable } from "./writable.ts";
export { Duplex } from "./duplex.ts";
export { Transform } from "./transform.ts";
export { PassThrough } from "./pass-through.ts";
export { pipeline, finished } from "./utilities.ts";
class StreamPromisesNamespace {
  finished(stream: Stream): Promise<void> {
    return finishedPromise(stream);
  }

  pipeline(...streams: Stream[]): Promise<void> {
    return pipelinePromise(...streams);
  }
}

const promises = new StreamPromisesNamespace();

export { promises };
