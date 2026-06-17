/**
 * performance singleton — provides access to performance-related information.
 * High-resolution timing and performance measurement.
 *
 */

import type { RuntimeValue } from "../runtime-value.ts";
import { Stopwatch } from "@tsonic/dotnet/System.Diagnostics.js";

import { PerformanceEntry, PerformanceMark, PerformanceMeasure } from "./performance-entry.ts";
import { PerformanceObserver } from "./performance-observer.ts";

/**
 * Options for creating a performance mark.
 */
export interface MarkOptions {
  detail?: RuntimeValue;
  startTime?: number;
}

/**
 * Options for creating a performance measure.
 */
export interface MeasureOptions {
  detail?: RuntimeValue;
  startMark?: string;
  endMark?: string;
  start?: number;
  end?: number;
}

const stopwatch = new Stopwatch();
stopwatch.Start();

const entries: PerformanceEntry[] = [];

const getMarkTime = (markName: string | null | undefined): number | null => {
  if (markName === null || markName === undefined || markName.length === 0) {
    return null;
  }

  for (let i = entries.length - 1; i >= 0; i -= 1) {
    const entry = entries[i]!;
    if (entry.entryType === "mark" && entry.name === markName) {
      return entry.startTime;
    }
  }

  return null;
};

/**
 * Returns the current high-resolution timestamp in milliseconds.
 * The timestamp is relative to an arbitrary point in time and is monotonically increasing.
 */
export const now = (): number => {
  return stopwatch.Elapsed.TotalMilliseconds;
};

/**
 * Creates a new PerformanceMark entry with the given name.
 * The mark represents a single point in time.
 */
export const mark = (
  name: string,
  options?: MarkOptions | null,
): PerformanceMark => {
  if (name === null || name === undefined || name.length === 0) {
    throw new Error("Name cannot be null or empty");
  }

  const requestedStartTime =
    options !== null && options !== undefined ? options.startTime : undefined;
  const startTime =
    requestedStartTime !== null && requestedStartTime !== undefined
      ? requestedStartTime
      : now();

  const detail =
    options !== null && options !== undefined ? (options.detail ?? null) : null;

  const entry = new PerformanceMark(name, startTime, detail);
  entries.push(entry);

  PerformanceObserver.notifyObservers(entry);

  return entry;
};

/**
 * Creates a new PerformanceMeasure entry representing the duration between two marks or timestamps.
 */
export const measure = (
  name: string,
  startOrOptions?: string | MeasureOptions | null,
  endMark?: string | null,
): PerformanceMeasure => {
  if (name === null || name === undefined || name.length === 0) {
    throw new Error("Name cannot be null or empty");
  }

  let options: MeasureOptions | null = null;
  let startMark: string | null = null;
  if (typeof startOrOptions === "string") {
    startMark = startOrOptions;
  } else {
    options = startOrOptions ?? null;
  }

  let startTime: number;
  let endTime: number;
  let detail: RuntimeValue = null;

  if (
    options !== null &&
    options !== undefined &&
    options.start !== null &&
    options.start !== undefined
  ) {
    startTime = options.start;
  } else {
    const startMarkName =
      options !== null && options !== undefined ? options.startMark : startMark;
    startTime = getMarkTime(startMarkName) ?? 0;
  }

  if (
    options !== null &&
    options !== undefined &&
    options.end !== null &&
    options.end !== undefined
  ) {
    endTime = options.end;
  } else {
    const endMarkName =
      options !== null && options !== undefined
        ? (options.endMark ?? endMark ?? null)
        : (endMark ?? null);
    endTime = getMarkTime(endMarkName) ?? now();
  }

  detail =
    options !== null && options !== undefined ? (options.detail ?? null) : null;

  const duration = endTime - startTime;
  const entry = new PerformanceMeasure(name, startTime, duration, detail);
  entries.push(entry);

  PerformanceObserver.notifyObservers(entry);

  return entry;
};

/**
 * Returns all performance entries in chronological order.
 */
export const getEntries = (): PerformanceEntry[] => {
  return [...entries];
};

/**
 * Returns all performance entries with the given name, optionally filtered by type.
 */
export const getEntriesByName = (
  name: string,
  type?: string | null,
): PerformanceEntry[] => {
  if (name === null || name === undefined || name.length === 0) {
    throw new Error("Name cannot be null or empty");
  }

  return entries.filter((entry) => {
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
};

/**
 * Returns all performance entries of the given type.
 */
export const getEntriesByType = (type: string): PerformanceEntry[] => {
  if (type === null || type === undefined || type.length === 0) {
    throw new Error("Type cannot be null or empty");
  }

  return entries.filter((entry) => entry.entryType === type);
};

/**
 * Removes all marks from the performance timeline, or a specific mark if name is provided.
 */
export const clearMarks = (name?: string | null): void => {
  if (name === null || name === undefined || name.length === 0) {
    for (let i = entries.length - 1; i >= 0; i -= 1) {
      if (entries[i]!.entryType === "mark") {
        entries.splice(i, 1);
      }
    }
  } else {
    for (let i = entries.length - 1; i >= 0; i -= 1) {
      if (entries[i]!.entryType === "mark" && entries[i]!.name === name) {
        entries.splice(i, 1);
      }
    }
  }
};

/**
 * Removes all measures from the performance timeline, or a specific measure if name is provided.
 */
export const clearMeasures = (name?: string | null): void => {
  if (name === null || name === undefined || name.length === 0) {
    for (let i = entries.length - 1; i >= 0; i -= 1) {
      if (entries[i]!.entryType === "measure") {
        entries.splice(i, 1);
      }
    }
  } else {
    for (let i = entries.length - 1; i >= 0; i -= 1) {
      if (entries[i]!.entryType === "measure" && entries[i]!.name === name) {
        entries.splice(i, 1);
      }
    }
  }
};
