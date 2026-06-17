/**
 * os.userInfo — returns information about the current user.
 *
 */
import { Environment, Environment_SpecialFolder } from "@tsonic/dotnet/System.js";
import {
  OSPlatform,
  RuntimeInformation,
} from "@tsonic/dotnet/System.Runtime.InteropServices.js";

export interface UserInfo {
  username: string;
  uid: number;
  gid: number;
  shell: string | null;
  homedir: string;
}

export const userInfo = (): UserInfo => {
  const isWindows = RuntimeInformation.IsOSPlatform(OSPlatform.Windows);
  return {
    username: Environment.UserName,
    uid: isWindows ? -1 : 1000,
    gid: isWindows ? -1 : 1000,
    shell: isWindows
      ? null
      : (Environment.GetEnvironmentVariable("SHELL") ?? "/bin/bash"),
    homedir: Environment.GetFolderPath(Environment_SpecialFolder.UserProfile),
  };
};
