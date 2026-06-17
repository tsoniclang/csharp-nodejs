/**
 * os.devNull — platform-specific path to the null device.
 *
 */
import {
  OSPlatform,
  RuntimeInformation,
} from "@tsonic/dotnet/System.Runtime.InteropServices.js";

export const devNull: string = RuntimeInformation.IsOSPlatform(
  OSPlatform.Windows,
)
  ? "\\\\.\\nul"
  : "/dev/null";
