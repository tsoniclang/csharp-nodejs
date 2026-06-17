/**
 * os.hostname — returns the host name of the operating system.
 *
 */
import { Dns } from "@tsonic/dotnet/System.Net.js";

// TODO: Verify Dns.GetHostName() availability in NativeAOT target.
export const hostname = (): string => Dns.GetHostName();
