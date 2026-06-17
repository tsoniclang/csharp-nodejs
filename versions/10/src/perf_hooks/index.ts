/**
 * Node.js perf_hooks module — performance measurement APIs.
 *
 */

import type {} from "../type-bootstrap.ts";

export {
  PerformanceEntry,
  PerformanceMark,
  PerformanceMeasure,
} from "./performance-entry.ts";

export {
  PerformanceObserver,
  PerformanceObserverEntryList,
} from "./performance-observer.ts";

export type {
  PerformanceObserverOptions,
  PerformanceObserverCallback,
} from "./performance-observer.ts";

export {
  now,
  mark,
  measure,
  getEntries,
  getEntriesByName,
  getEntriesByType,
  clearMarks,
  clearMeasures,
} from "./performance.ts";

export type { MarkOptions, MeasureOptions } from "./performance.ts";

import * as performanceModule from "./performance.ts";
import type { MarkOptions, MeasureOptions } from "./performance.ts";
import type {
  PerformanceEntry,
  PerformanceMark,
  PerformanceMeasure,
} from "./performance-entry.ts";

class PerformanceApi {
  now(): number {
    return performanceModule.now();
  }

  mark(
    name: string,
    options?: MarkOptions | null,
  ): PerformanceMark {
    return performanceModule.mark(name, options);
  }

  measure(
    name: string,
    startOrOptions?: string | MeasureOptions | null,
    endMark?: string | null,
  ): PerformanceMeasure {
    return performanceModule.measure(name, startOrOptions, endMark);
  }

  getEntries(): PerformanceEntry[] {
    return performanceModule.getEntries();
  }

  getEntriesByName(
    name: string,
    type?: string | null,
  ): PerformanceEntry[] {
    return performanceModule.getEntriesByName(name, type);
  }

  getEntriesByType(type: string): PerformanceEntry[] {
    return performanceModule.getEntriesByType(type);
  }

  clearMarks(name?: string | null): void {
    performanceModule.clearMarks(name);
  }

  clearMeasures(name?: string | null): void {
    performanceModule.clearMeasures(name);
  }
}

/**
 * The performance object — a namespace-like re-export of the performance functions
 * matching Node.js `perf_hooks.performance`.
 */
export const performance: PerformanceApi = new PerformanceApi();
