/**
 * os.endianness — returns 'BE' or 'LE'.
 *
 */
import { BitConverter } from "@tsonic/dotnet/System.js";

export const endianness = (): string =>
  BitConverter.IsLittleEndian ? "LE" : "BE";
