/**
 * Performance entry classes for the perf_hooks module.
 *
 */

import type { RuntimeValue } from "../runtime-value.ts";

/**
 * Base class for all performance entries.
 * Represents a single performance metric entry in the Performance Timeline.
 */
export class PerformanceEntry {
  name: string;
  entryType: string;
  startTime: number;
  duration: number;

  constructor(
    name: string,
    entryType: string,
    startTime: number,
    duration: number,
  ) {
    this.name = name;
    this.entryType = entryType;
    this.startTime = startTime;
    this.duration = duration;
  }
}

/**
 * Represents a performance mark — a named timestamp in the performance timeline.
 */
export class PerformanceMark extends PerformanceEntry {
  detail: RuntimeValue;

  constructor(
    name: string,
    startTime: number,
    detail: RuntimeValue = null,
  ) {
    super(name, "mark", startTime, 0);
    this.detail = detail;
  }
}

/**
 * Represents a performance measure — the duration between two marks or timestamps.
 */
export class PerformanceMeasure extends PerformanceEntry {
  detail: RuntimeValue;

  constructor(
    name: string,
    startTime: number,
    duration: number,
    detail: RuntimeValue = null,
  ) {
    super(name, "measure", startTime, duration);
    this.detail = detail;
  }
}
