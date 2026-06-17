/**
 * os.cpus — returns per-core CPU information.
 *
 */
import { Environment } from "@tsonic/dotnet/System.js";

export interface CpuTimes {
  user: number;
  nice: number;
  sys: number;
  idle: number;
  irq: number;
}

export interface CpuInfo {
  model: string;
  speed: number;
  times: CpuTimes;
}

// TODO: Implement real CPU info via native interop.
// Placeholder returns one entry per logical core with zeroed fields.
export const cpus = (): CpuInfo[] => {
  const count = Environment.ProcessorCount;
  const result: CpuInfo[] = [];
  for (let i = 0; i < count; i += 1) {
    result.push({
      model: "Unknown CPU",
      speed: 0,
      times: { user: 0, nice: 0, sys: 0, idle: 0, irq: 0 },
    });
  }
  return result;
};
