/**
 * PerformanceObserver — observes performance measurement events and notifies
 * when new performance entries are added to the performance timeline.
 *
 */

import type { int } from "@tsonic/core/types.js";

import { PerformanceEntry } from "./performance-entry.ts";

/**
 * Options for configuring a PerformanceObserver.
 */
export interface PerformanceObserverOptions {
  entryTypes: readonly string[];
  buffered?: boolean;
}

/**
 * A list of performance entries provided to PerformanceObserver callbacks.
 */
export class PerformanceObserverEntryList {
  entries: readonly PerformanceEntry[];

  constructor(entries: readonly PerformanceEntry[]) {
    this.entries = entries;
  }

  getEntries(): PerformanceEntry[] {
    return [...this.entries];
  }

  getEntriesByName(
    name: string,
    type?: string | null,
  ): PerformanceEntry[] {
    if (name === null || name === undefined || name.length === 0) {
      throw new Error("Name cannot be null or empty");
    }

    const filtered = this.entries.filter((entry) => {
      if (entry.name !== name) {
        return false;
      }
      if (
        type !== null &&
        type !== undefined &&
        type.length > 0 &&
        entry.entryType !== type
      ) {
        return false;
      }
      return true;
    });

    return filtered;
  }

  getEntriesByType(type: string): PerformanceEntry[] {
    if (type === null || type === undefined || type.length === 0) {
      throw new Error("Type cannot be null or empty");
    }

    return this.entries.filter((entry) => entry.entryType === type);
  }
}

export type PerformanceObserverCallback = (
  list: PerformanceObserverEntryList,
  observer: PerformanceObserver,
) => void;

type RegisteredObserver = {
  id: int;
  observer: PerformanceObserver;
};

const observers: RegisteredObserver[] = [];
let nextObserverId = 1 as int;

/**
 * PerformanceObserver is used to observe performance measurement events and be notified
 * when new performance entries are added to the performance timeline.
 */
export class PerformanceObserver {
  observerId: int;
  callback: PerformanceObserverCallback;
  entryTypes: string[] = [];
  observing = false;

  constructor(callback: PerformanceObserverCallback) {
    if (callback === null || callback === undefined) {
      throw new Error("callback must not be null");
    }
    this.observerId = nextObserverId;
    nextObserverId = (nextObserverId + 1) as int;
    this.callback = callback;
  }

  observe(options: PerformanceObserverOptions): void {
    if (options === null || options === undefined) {
      throw new Error("options must not be null");
    }

    if (
      options.entryTypes === null ||
      options.entryTypes === undefined ||
      options.entryTypes.length === 0
    ) {
      throw new Error("entryTypes must be provided and non-empty");
    }

    this.entryTypes = [...options.entryTypes];
    this.observing = true;

    if (!observers.some((entry) => entry.id === this.observerId)) {
      observers.push({
        id: this.observerId,
        observer: this,
      });
    }
  }

  disconnect(): void {
    this.observing = false;

    const index = observers.findIndex(
      (entry) => entry.id === this.observerId,
    );
    if (index >= 0) {
      observers.splice(index, 1);
    }
  }

  takeRecords(): PerformanceObserverEntryList {
    return new PerformanceObserverEntryList([]);
  }

  static supportedEntryTypes(): string[] {
    return ["mark", "measure", "function", "gc", "resource"];
  }

  /** @internal */
  static notifyObservers(entry: PerformanceEntry): void {
    const toNotify = observers.filter(
      (registeredObserver) =>
        registeredObserver.observer.observing &&
        registeredObserver.observer.entryTypes.includes(entry.entryType),
    );

    for (const registeredObserver of toNotify) {
      try {
        const entryList = new PerformanceObserverEntryList([entry]);
        const observer = registeredObserver.observer;
        observer.callback(entryList, observer);
      } catch {
        // Observers should not throw, but if they do, we continue notifying others
      }
    }
  }
}
