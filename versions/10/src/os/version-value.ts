/**
 * os.version — returns a human-readable OS version string.
 *
 */
import {
  RuntimeInformation,
} from "@tsonic/dotnet/System.Runtime.InteropServices.js";

export const version = (): string => RuntimeInformation.OSDescription;
