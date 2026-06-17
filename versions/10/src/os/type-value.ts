/**
 * os.type — returns the OS name (e.g. 'Linux', 'Darwin', 'Windows_NT').
 *
 */
import {
  OSPlatform,
  RuntimeInformation,
} from "@tsonic/dotnet/System.Runtime.InteropServices.js";

export const type = (): string => {
  if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) {
    return "Windows_NT";
  }
  if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux)) {
    return "Linux";
  }
  if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX)) {
    return "Darwin";
  }
  if (RuntimeInformation.IsOSPlatform(OSPlatform.FreeBSD)) {
    return "FreeBSD";
  }
  return RuntimeInformation.OSDescription;
};
