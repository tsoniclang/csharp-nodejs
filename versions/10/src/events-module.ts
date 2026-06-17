
import type {} from "./type-bootstrap.ts";

import type { int } from "@tsonic/core/types.js";
import { Console as DotnetConsole } from "@tsonic/dotnet/System.js";
import type { RuntimeValue } from "./runtime-value.ts";

export type EventListener = (...args: RuntimeValue[]) => void;

export const toEventListener = (
  listener: (() => void) | null | undefined
): EventListener | undefined => {
  if (listener === undefined || listener === null) {
    return undefined;
  }

  return (..._args: RuntimeValue[]): void => {
    listener();
  };
};

export const toUnaryEventListener = <T>(
  listener: ((value: T) => void) | null | undefined
): EventListener | undefined => {
  if (listener === undefined || listener === null) {
    return undefined;
  }

  return (...args: RuntimeValue[]): void => {
    listener(args[0] as T);
  };
};

export const toBinaryEventListener = <T1, T2>(
  listener: ((first: T1, second: T2) => void) | null | undefined
): EventListener | undefined => {
  if (listener === undefined || listener === null) {
    return undefined;
  }

  return (...args: RuntimeValue[]): void => {
    listener(args[0] as T1, args[1] as T2);
  };
};

type ListenerRegistration = {
  original: EventListener;
  invoke: EventListener;
  once: boolean;
};

const ERROR_EVENT = "error";
const NEW_LISTENER_EVENT = "newListener";
const REMOVE_LISTENER_EVENT = "removeListener";

const throwUnhandledError = (value?: RuntimeValue): never => {
  if (value instanceof Error) {
    throw value;
  }

  throw new Error(`Uncaught, unspecified 'error' event. (${String(value)})`);
};

export class EventEmitter {
  static _defaultMaxListeners: int = 10 as int;

  static once(
    emitter: EventEmitter,
    eventName: string
  ): Promise<RuntimeValue[]> {
    if (emitter === undefined || emitter === null) {
      throw new Error("EventEmitter.once requires an emitter");
    }

    if (eventName === undefined || eventName === null || eventName.length === 0) {
      throw new Error("EventEmitter.once requires a non-empty event name");
    }

    return once(emitter, eventName);
  }

  listenersByEvent: Map<string, ListenerRegistration[]> =
    new Map<string, ListenerRegistration[]>();
  knownEventNames: string[] = [];
  _maxListeners: int = EventEmitter._defaultMaxListeners;

  static get defaultMaxListeners(): int {
    return EventEmitter._defaultMaxListeners;
  }

  static set defaultMaxListeners(value: int) {
    if (value < 0) {
      throw new Error("Max listeners must be non-negative");
    }

    EventEmitter._defaultMaxListeners = value;
  }

  addListener(eventName: string, listener: EventListener): EventEmitter {
    return this.on(eventName, listener);
  }

  on(eventName: string, listener: EventListener): EventEmitter {
    return this.insertListener(eventName, listener, false, false);
  }

  once(eventName: string, listener: EventListener): EventEmitter {
    return this.insertListener(eventName, listener, true, false);
  }

  prependListener(
    eventName: string,
    listener: EventListener
  ): EventEmitter {
    return this.insertListener(eventName, listener, false, true);
  }

  prependOnceListener(
    eventName: string,
    listener: EventListener
  ): EventEmitter {
    return this.insertListener(eventName, listener, true, true);
  }

  off(eventName: string, listener: EventListener): EventEmitter {
    return this.removeListener(eventName, listener);
  }

  removeListener(eventName: string, listener: EventListener): EventEmitter {
    const registrations = this.listenersByEvent.get(eventName);
    if (registrations === undefined || registrations.length === 0) {
      return this;
    }

    let removed: ListenerRegistration | undefined;
    const remaining: ListenerRegistration[] = [];

    for (const registration of registrations) {
      if (
        removed === undefined &&
        (registration.original === listener || registration.invoke === listener)
      ) {
        removed = registration;
        continue;
      }

      remaining.push(registration);
    }

    if (remaining.length === 0) {
      this.listenersByEvent.delete(eventName);
      this.removeKnownEventName(eventName);
    } else {
      this.listenersByEvent.set(eventName, remaining);
    }

    if (removed !== undefined && eventName !== REMOVE_LISTENER_EVENT) {
      this.emit(REMOVE_LISTENER_EVENT, eventName, removed.original);
    }

    return this;
  }

  removeAllListeners(eventName?: string): EventEmitter {
    if (eventName === undefined) {
      const names = this.eventNames();
      for (const name of names) {
        this.removeAllListeners(name);
      }
      this.listenersByEvent.clear();
      this.knownEventNames.splice(0, this.knownEventNames.length);
      return this;
    }

    const listeners = this.listeners(eventName);
    this.listenersByEvent.delete(eventName);
    this.removeKnownEventName(eventName);

    if (eventName !== REMOVE_LISTENER_EVENT) {
      for (const listener of listeners) {
        this.emit(REMOVE_LISTENER_EVENT, eventName, listener);
      }
    }

    return this;
  }

  emit(eventName: string, ...args: RuntimeValue[]): boolean {
    const registrations = this.listenersByEvent.get(eventName);
    if (registrations === undefined || registrations.length === 0) {
      if (eventName === ERROR_EVENT) {
        throwUnhandledError(args.length > 0 ? args[0] : undefined);
      }
      return false;
    }

    const snapshot = registrations.slice();
    for (const registration of snapshot) {
      try {
        registration.invoke(...args);
      } catch (error) {
        if (eventName !== ERROR_EVENT) {
          const eventError =
            error instanceof Error
              ? error
              : new Error("Unknown event handler error");
          this.emit(ERROR_EVENT, eventError);
        } else {
          throw error;
        }
      }
    }

    return true;
  }

  listeners(eventName: string): EventListener[] {
    const registrations = this.listenersByEvent.get(eventName);
    if (registrations === undefined) {
      return [];
    }

    return registrations.map((registration) => registration.original);
  }

  rawListeners(eventName: string): EventListener[] {
    const registrations = this.listenersByEvent.get(eventName);
    if (registrations === undefined) {
      return [];
    }

    return registrations.map((registration) => registration.invoke);
  }

  listenerCount(eventName: string): int {
    const registrations = this.listenersByEvent.get(eventName);
    return registrations?.length ?? 0;
  }

  eventNames(): string[] {
    return [...this.knownEventNames];
  }

  getMaxListeners(): int {
    return this._maxListeners;
  }

  setMaxListeners(value: int): EventEmitter {
    if (value < 0) {
      throw new Error("Max listeners must be non-negative");
    }

    this._maxListeners = value;
    return this;
  }

  insertListener(
    eventName: string,
    listener: EventListener,
    once: boolean,
    prepend: boolean
  ): EventEmitter {
    const registration = this.createRegistration(eventName, listener, once);
    const existing = this.listenersByEvent.get(eventName) ?? [];
    const next = prepend
      ? [registration, ...existing]
      : [...existing, registration];
    if (existing.length === 0) {
      this.knownEventNames.push(eventName);
    }
    this.listenersByEvent.set(eventName, next);

    if (eventName !== NEW_LISTENER_EVENT) {
      this.emit(NEW_LISTENER_EVENT, eventName, listener);
    }

    if (this._maxListeners > 0 && next.length > this._maxListeners) {
      DotnetConsole.Error.WriteLine(
        `Warning: Possible EventEmitter memory leak detected. ${String(next.length)} ${eventName} listeners added.`
      );
    }

    return this;
  }

  createRegistration(
    eventName: string,
    listener: EventListener,
    once: boolean
  ): ListenerRegistration {
    if (!once) {
      return { original: listener, invoke: listener, once: false };
    }

    let invoked = false;
    const invoke: EventListener = (...args: RuntimeValue[]): void => {
      if (invoked) {
        return;
      }
      invoked = true;
      const forwardedArgs = args;
      listener(...forwardedArgs);
      this.removeListener(eventName, invoke);
    };

    return { original: listener, invoke, once: true };
  }

  removeKnownEventName(eventName: string): void {
    const index = this.knownEventNames.indexOf(eventName);
    if (index >= 0) {
      this.knownEventNames.splice(index, 1);
    }
  }
}

export const captureRejectionSymbol = "nodejs.captureRejection";
export const errorMonitor = "errorMonitor";

let captureRejections: boolean = false;

export const addAbortListener = (
  _signal: RuntimeValue,
  listener: () => void
): (() => void) => listener;

export const getEventListeners = (
  emitter: EventEmitter,
  eventName: string
): EventListener[] => emitter.listeners(eventName);

export const getMaxListeners = (emitter: EventEmitter): int =>
  emitter.getMaxListeners();

export const listenerCount = (emitter: EventEmitter, eventName: string): int =>
  emitter.listenerCount(eventName);

export const once = (
  emitter: EventEmitter,
  eventName: string
): Promise<RuntimeValue[]> => {
  if (emitter === undefined || emitter === null) {
    throw new Error("EventEmitter.once requires an emitter");
  }

  if (eventName === undefined || eventName === null || eventName.length === 0) {
    throw new Error("EventEmitter.once requires a non-empty event name");
  }

  return new Promise<RuntimeValue[]>((resolve) => {
    emitter.once(eventName, (...args: RuntimeValue[]) => {
      resolve(args);
    });
  });
};

export const setMaxListeners = (
  value: int,
  ...emitters: EventEmitter[]
): void => {
  for (const emitter of emitters) {
    emitter.setMaxListeners(value);
  }
};

export class EventsModule {
  get captureRejections(): boolean {
    return captureRejections;
  }

  set captureRejections(value: boolean) {
    captureRejections = value;
  }

  get defaultMaxListeners(): int {
    return EventEmitter.defaultMaxListeners;
  }

  set defaultMaxListeners(value: int) {
    EventEmitter.defaultMaxListeners = value;
  }

  get captureRejectionSymbol(): string {
    return captureRejectionSymbol;
  }

  get errorMonitor(): string {
    return errorMonitor;
  }

  addAbortListener(signal: RuntimeValue, listener: () => void): () => void {
    return addAbortListener(signal, listener);
  }

  getEventListeners(
    emitter: EventEmitter,
    eventName: string
  ): EventListener[] {
    return getEventListeners(emitter, eventName);
  }

  getMaxListeners(emitter: EventEmitter): int {
    return getMaxListeners(emitter);
  }

  listenerCount(emitter: EventEmitter, eventName: string): int {
    return listenerCount(emitter, eventName);
  }

  async once(
    emitter: EventEmitter,
    eventName: string
  ): Promise<RuntimeValue[]> {
    return await once(emitter, eventName);
  }

  setMaxListeners(value: int, ...emitters: EventEmitter[]): void {
    setMaxListeners(value, ...emitters);
  }
}

export const events = new EventsModule();
