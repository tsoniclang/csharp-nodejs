/**
 * os.homedir — returns the current user's home directory.
 *
 */
import { Environment, Environment_SpecialFolder } from "@tsonic/dotnet/System.js";

export const homedir = (): string =>
  Environment.GetFolderPath(Environment_SpecialFolder.UserProfile);
