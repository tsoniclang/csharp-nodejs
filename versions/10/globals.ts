import type { int } from "@tsonic/core/types.js";
import { Buffer as SourceBuffer } from "./src/buffer/index.ts";
import { process as SourceProcess } from "./src/process-module.ts";
import type { RuntimeValue } from "./src/runtime-value.ts";
import type { Immediate, Timeout } from "./src/timers-module.js";

declare global {
  interface Console {
    trace(...data: RuntimeValue[]): void;
    assert(
      condition: boolean,
      message?: string,
      ...optionalParams: RuntimeValue[]
    ): void;
    clear(): void;
    count(label?: string): void;
    countReset(label?: string): void;
    dir(obj?: RuntimeValue, ...options: RuntimeValue[]): void;
    dirxml(...data: RuntimeValue[]): void;
    group(...data: RuntimeValue[]): void;
    groupCollapsed(...data: RuntimeValue[]): void;
    groupEnd(): void;
    table(data?: RuntimeValue, properties?: string[]): void;
    time(label?: string): void;
    timeEnd(label?: string): void;
    timeLog(label?: string, ...data: RuntimeValue[]): void;
  }

  function setTimeout(
    handler: (...args: RuntimeValue[]) => void,
    timeout?: int,
    ...args: RuntimeValue[]
  ): Timeout;

  function clearTimeout(timeout?: Timeout): void;

  function setInterval(
    handler: (...args: RuntimeValue[]) => void,
    timeout?: int,
    ...args: RuntimeValue[]
  ): Timeout;

  function clearInterval(timeout?: Timeout): void;

  function setImmediate(
    callback: (...args: RuntimeValue[]) => void,
    ...args: RuntimeValue[]
  ): Immediate;

  function clearImmediate(immediate?: Immediate): void;

  function queueMicrotask(callback: () => void): void;

  const Buffer: typeof SourceBuffer;

  const process: typeof SourceProcess;
}

export {};
