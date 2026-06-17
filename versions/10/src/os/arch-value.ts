/**
 * os.arch — returns the CPU architecture.
 *
 */
import {
  Convert,
} from "@tsonic/dotnet/System.js";
import { RuntimeInformation } from "@tsonic/dotnet/System.Runtime.InteropServices.js";

export const arch = (): string => {
  const raw = (Convert.ToString(RuntimeInformation.ProcessArchitecture) ?? "")
    .toLowerCase();
  if (raw === "x64") {
    return "x64";
  }
  if (raw === "x86") {
    return "ia32";
  }
  if (raw === "arm") {
    return "arm";
  }
  if (raw === "arm64") {
    return "arm64";
  }
  if (raw === "wasm") {
    return "wasm";
  }
  if (raw === "s390x") {
    return "s390x";
  }
  return raw;
};
