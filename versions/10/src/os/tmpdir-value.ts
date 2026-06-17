/**
 * os.tmpdir — returns the OS default temporary directory.
 *
 */
import { Convert } from "@tsonic/dotnet/System.js";
import { Path } from "@tsonic/dotnet/System.IO.js";

export const tmpdir = (): string => {
  const raw = Path.GetTempPath();
  // Node.js trims trailing separators from tmpdir
  const sep = Convert.ToString(Path.DirectorySeparatorChar) ?? "";
  const altSep = Convert.ToString(Path.AltDirectorySeparatorChar) ?? "";
  let end = raw.length;
  while (
    end > 1 &&
    (raw[end - 1] === sep || raw[end - 1] === altSep)
  ) {
    end -= 1;
  }
  return raw.substring(0, end);
};
