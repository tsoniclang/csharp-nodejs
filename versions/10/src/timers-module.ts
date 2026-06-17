
import type {} from "./type-bootstrap.ts";

import type { int, long, out } from "@tsonic/core/types.js";
import { Environment } from "@tsonic/dotnet/System.js";
import { ConcurrentQueue } from "@tsonic/dotnet/System.Collections.Concurrent.js";
import { Queue } from "@tsonic/dotnet/System.Collections.Generic.js";
import {
  CancellationTokenSource,
  ManualResetEventSlim,
  Thread,
} from "@tsonic/dotnet/System.Threading.js";
import { Task } from "@tsonic/dotnet/System.Threading.Tasks.js";
import type { RuntimeValue } from "./runtime-value.ts";

const normalizeDelay = (value?: int): int => {
  if (value === undefined || value < (0 as int)) {
    return 0 as int;
  }
  return value;
};

const immediateDispatchDelay = 50 as int;

export class Timeout {
  callback: () => void;
  delay: int;
  period: int | undefined;
  cancellation: CancellationTokenSource =
    new CancellationTokenSource();
  generation = 0;
  disposed = false;
  referenced = true;

  constructor(callback: () => void, delay: int, period?: int) {
    this.callback = callback;
    this.delay = normalizeDelay(delay);
    this.period = period;
    this.schedule(this.delay);
  }

  schedule(initialDelay: int): void {
    this.generation += 1;
    const currentGeneration = this.generation;

    Task.Run(async () => {
      try {
        await Task.Delay(initialDelay, this.cancellation.Token);

        while (!this.disposed && currentGeneration === this.generation) {
          this.callback();

          if (this.period === undefined) {
            this.dispose();
            return;
          }

          await Task.Delay(this.period, this.cancellation.Token);
        }
      } catch {
        return;
      }
    });
  }

  ref(): Timeout {
    this.referenced = true;
    return this;
  }

  unref(): Timeout {
    this.referenced = false;
    return this;
  }

  hasRef(): boolean {
    return this.referenced;
  }

  refresh(): Timeout {
    if (!this.disposed) {
      this.schedule(this.delay);
    }
    return this;
  }

  close(): void {
    this.dispose();
  }

  dispose(): void {
    if (this.disposed) {
      return;
    }

    this.disposed = true;
    this.cancellation.Cancel();
    this.cancellation.Dispose();
  }
}

export class Immediate {
  static pendingHandles: ConcurrentQueue<Immediate> =
    new ConcurrentQueue<Immediate>();
  static dispatchThread: Thread =
    Immediate.startDispatchThread();

  static startDispatchThread(): Thread {
    const thread = new Thread(() => {
      let handle!: Immediate;
      while (true) {
        let hadWork = false;

        while (Immediate.pendingHandles.TryDequeue(handle as out<Immediate>)) {
          hadWork = true;
          handle.tryExecute();
        }

        if (!hadWork) {
          Thread.Sleep(1 as int);
        }
      }
    });
    thread.IsBackground = true;
    thread.Name = "nodejs.Immediate.dispatch";
    thread.Start();
    return thread;
  }

  callback: () => void;
  cancelSignal: ManualResetEventSlim =
    new ManualResetEventSlim(false);
  readyAfterTick: long =
    Environment.TickCount64 + immediateDispatchDelay;
  disposed = false;
  referenced = true;

  constructor(callback: () => void) {
    this.callback = callback;
    Immediate.pendingHandles.Enqueue(this);
  }

  tryExecute(): void {
    if (this.cancelSignal.IsSet || this.disposed) {
      return;
    }

    if (Environment.TickCount64 < this.readyAfterTick) {
      Immediate.pendingHandles.Enqueue(this);
      return;
    }

    try {
      this.callback();
    } finally {
      this.dispose();
    }
  }

  ref(): Immediate {
    this.referenced = true;
    return this;
  }

  unref(): Immediate {
    this.referenced = false;
    return this;
  }

  hasRef(): boolean {
    return this.referenced;
  }

  dispose(): void {
    if (this.disposed) {
      return;
    }

    this.disposed = true;
    this.cancelSignal.Set();
  }
}

export class TimersScheduler {
  async wait(delay: int = 1 as int): Promise<void> {
    await Task.Delay(normalizeDelay(delay));
  }

  async yield(): Promise<void> {
    await Task.Delay(0 as int);
  }
}

export class IntervalIterationResult<T> {
  done: boolean;
  value: T | undefined;

  constructor(done: boolean, value: T | undefined) {
    this.done = done;
    this.value = value;
  }
}

export class IntervalAsyncIterator<T> {
  queue: Queue<T | undefined> = new Queue<T | undefined>();
  waiters: Queue<
    (result: IntervalIterationResult<T>) => void
  > = new Queue<(result: IntervalIterationResult<T>) => void>();
  handle: Timeout;
  closed = false;

  constructor(delay: int, value?: T) {
    const actualDelay = normalizeDelay(delay);
    this.handle = new Timeout(() => {
      this.enqueue(value);
    }, actualDelay, actualDelay);
  }

  enqueue(value?: T): void {
    if (this.closed) {
      return;
    }

    if (this.waiters.Count > 0) {
      const waiter = this.waiters.Dequeue();
      waiter(new IntervalIterationResult(false, value));
      return;
    }

    this.queue.Enqueue(value);
  }

  close(): void {
    if (this.closed) {
      return;
    }

    this.closed = true;
    this.handle.dispose();

    while (this.waiters.Count > 0) {
      const waiter = this.waiters.Dequeue();
      waiter(new IntervalIterationResult<T>(true, undefined));
    }
  }

  next(): Promise<IntervalIterationResult<T>> {
    if (this.queue.Count > 0) {
      const value = this.queue.Dequeue();
      return Promise.resolve(new IntervalIterationResult(false, value));
    }

    if (this.closed) {
      return Promise.resolve(new IntervalIterationResult<T>(true, undefined));
    }

    return new Promise<IntervalIterationResult<T>>((resolve) => {
      this.waiters.Enqueue(resolve);
    });
  }

  return(
    value: T | undefined = undefined
  ): Promise<IntervalIterationResult<T>> {
    this.close();
    return Promise.resolve(new IntervalIterationResult(true, value));
  }

  async throw(
    error?: Error
  ): Promise<IntervalIterationResult<T>> {
    this.close();
    throw error ?? new Error("Promise rejected");
  }

  [Symbol.asyncIterator](): IntervalAsyncIterator<T> {
    return this;
  }
}

export class TimersPromises {
  scheduler: TimersScheduler = new TimersScheduler();

  async setTimeout<T extends RuntimeValue>(
    delay: int = 1 as int,
    value?: T,
  ): Promise<T | undefined> {
    await Task.Delay(normalizeDelay(delay));
    return value;
  }

  async setImmediate<T extends RuntimeValue>(
    value?: T
  ): Promise<T | undefined> {
    await Task.Delay(immediateDispatchDelay);
    return value;
  }

  setInterval(
    delay: int = 1 as int,
    value?: RuntimeValue
  ): IntervalAsyncIterator<RuntimeValue> {
    return new IntervalAsyncIterator<RuntimeValue>(delay, value);
  }
}

export const promises = new TimersPromises();

export const setTimeout = (
  callback: () => void,
  delay: int = 0 as int
): Timeout => {
  return new Timeout(callback, normalizeDelay(delay));
};

export const clearTimeout = (timeout?: Timeout): void => {
  timeout?.dispose();
};

export const setInterval = (
  callback: () => void,
  delay: int = 0 as int
): Timeout => {
  const actualDelay = normalizeDelay(delay);
  return new Timeout(callback, actualDelay, actualDelay);
};

export const clearInterval = (timeout?: Timeout): void => {
  timeout?.dispose();
};

export const setImmediate = (callback: () => void): Immediate => {
  return new Immediate(callback);
};

export const clearImmediate = (immediate?: Immediate): void => {
  immediate?.dispose();
};

export const queueMicrotask = (callback: () => void): void => {
  Task.Run(() => {
    callback();
  });
};
