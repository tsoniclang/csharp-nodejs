/**
 * os.freemem — returns free system memory in bytes.
 *
 */
import { GC } from "@tsonic/dotnet/System.js";

// TODO: Implement accurate free memory via native interop.
// Placeholder uses GC memory info as an approximation.
export const freemem = (): number => {
  const info = GC.GetGCMemoryInfo();
  return info.TotalAvailableMemoryBytes - info.MemoryLoadBytes;
};
