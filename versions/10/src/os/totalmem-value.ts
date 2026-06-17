/**
 * os.totalmem — returns total system memory in bytes.
 *
 */
import { GC } from "@tsonic/dotnet/System.js";

// TODO: Implement accurate total memory via native interop.
// Placeholder uses GC memory info as an approximation.
export const totalmem = (): number =>
  GC.GetGCMemoryInfo().TotalAvailableMemoryBytes;
