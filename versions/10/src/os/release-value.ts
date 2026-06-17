/**
 * os.release — returns the OS release version string.
 *
 */
import { Environment } from "@tsonic/dotnet/System.js";

export const release = (): string =>
  Environment.OSVersion.Version.ToString();
