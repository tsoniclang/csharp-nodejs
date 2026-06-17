/**
 * os.uptime — returns system uptime in seconds.
 *
 */
import { Environment } from "@tsonic/dotnet/System.js";

// TODO: Implement accurate uptime via native interop.
// Placeholder uses Environment.TickCount64 (milliseconds since boot).
export const uptime = (): number =>
  Math.floor(Environment.TickCount64 / 1000);
