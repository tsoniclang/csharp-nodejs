/**
 * os.platform — returns the operating system platform identifier.
 *
 */
import {
  OSPlatform,
  RuntimeInformation,
} from "@tsonic/dotnet/System.Runtime.InteropServices.js";

export const platform = (): string => {
  if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) {
    return "win32";
  }
  if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux)) {
    return "linux";
  }
  if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX)) {
    return "darwin";
  }
  if (RuntimeInformation.IsOSPlatform(OSPlatform.FreeBSD)) {
    return "freebsd";
  }
  return RuntimeInformation.OSDescription.toLowerCase();
};
