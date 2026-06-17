
import type {} from "./type-bootstrap.ts";

import type { int } from "@tsonic/core/types.js";
import { Convert, Environment } from "@tsonic/dotnet/System.js";
import { Process } from "@tsonic/dotnet/System.Diagnostics.js";
import { Directory, File, Path } from "@tsonic/dotnet/System.IO.js";
import {
  OSPlatform,
  RuntimeInformation,
} from "@tsonic/dotnet/System.Runtime.InteropServices.js";

const archToNodeName = (value: string): string => {
  const normalized = value.toLowerCase();
  if (
    normalized === "x64" ||
    normalized === "arm64" ||
    normalized === "arm" ||
    normalized === "wasm" ||
    normalized === "s390x"
  ) {
    return normalized;
  }
  if (normalized === "x86") {
    return "ia32";
  }
  return normalized;
};

const platformToNodeName = (): string => {
  if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) {
    return "win32";
  }
  if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX)) {
    return "darwin";
  }
  if (RuntimeInformation.IsOSPlatform(OSPlatform.FreeBSD)) {
    return "freebsd";
  }
  return "linux";
};

const toInt32 = (value: number): int | undefined => {
  if (
    Number.isInteger(value) &&
    value >= -2147483648 &&
    value <= 2147483647
  ) {
    return value as int;
  }

  return undefined;
};

const isWindows = (): boolean =>
  RuntimeInformation.IsOSPlatform(OSPlatform.Windows);

const stringsEqual = (left: string, right: string): boolean =>
  isWindows()
    ? left.toLowerCase() === right.toLowerCase()
    : left === right;

const unsetEnvironmentVariable = (key: string): void => {
  Environment.SetEnvironmentVariable(key, null);
};

let currentExitCode: int | undefined = undefined;

let currentArgv = Environment.GetCommandLineArgs();
let currentArgv0: string = currentArgv[0] ?? "";

export const pid = Environment.ProcessId;
export const execPath =
  Environment.ProcessPath ??
  (currentArgv.length > 0 ? Path.GetFullPath(currentArgv[0]!) : "");
export const arch = archToNodeName(
  Convert.ToString(RuntimeInformation.ProcessArchitecture) ?? ""
);
export const platform = platformToNodeName();
export const version = "v24.0.0-tsonic";

export class ProcessVersions {
  node: string = "24.0.0";
  v8: string = "0.0.0";
  dotnet: string = Environment.Version.ToString();
  tsonic: string = "0.0.74";
}

const currentVersions = new ProcessVersions();

export class ProcessEnv {
  values: Map<string, string> = new Map<string, string>();

  constructor() {
    const variables = Environment.GetEnvironmentVariables();
    const iterator = variables.GetEnumerator();
    while (iterator.MoveNext()) {
      const entry = iterator.Entry;
      const rawKey = entry.Key;
      if (rawKey === undefined || rawKey === null) {
        continue;
      }

      const key = Convert.ToString(rawKey) ?? "";
      const rawValue = entry.Value;
      if (rawValue !== undefined && rawValue !== null) {
        this.values.set(key, Convert.ToString(rawValue) ?? "");
      }
    }
  }

  containsKey(key: string): boolean {
    return this.resolveKey(key) !== undefined;
  }

  get(key: string): string | undefined {
    const resolvedKey = this.resolveKey(key);
    return resolvedKey === undefined ? undefined : this.values.get(resolvedKey);
  }

  set(key: string, value: string | undefined): void {
    const resolvedKey = this.resolveKey(key) ?? key;
    if (value === undefined) {
      this.values.delete(resolvedKey);
      unsetEnvironmentVariable(resolvedKey);
      return;
    }

    this.values.set(resolvedKey, value);
    Environment.SetEnvironmentVariable(resolvedKey, value);
  }

  remove(key: string): boolean {
    const resolvedKey = this.resolveKey(key);
    if (resolvedKey === undefined) {
      return false;
    }

    this.values.delete(resolvedKey);
    unsetEnvironmentVariable(resolvedKey);
    return true;
  }

  resolveKey(key: string): string | undefined {
    if (this.hasOwnKey(key)) {
      return key;
    }

    for (const existingKey of this.values.keys()) {
      if (stringsEqual(existingKey, key)) {
        return existingKey;
      }
    }

    return undefined;
  }

  hasOwnKey(key: string): boolean {
    for (const existingKey of this.values.keys()) {
      if (existingKey === key) {
        return true;
      }
    }

    return false;
  }
}

const currentEnv = new ProcessEnv();

export const cwd = (): string => Directory.GetCurrentDirectory();

export const chdir = (directory: string): void => {
  if (directory.length === 0) {
    throw new Error("Directory path cannot be null or empty.");
  }

  if (!Directory.Exists(directory)) {
    throw new Error(`Directory not found: ${directory}`);
  }

  Directory.SetCurrentDirectory(directory);
};

export const exit = (code?: int): void => {
  const resolved = code ?? currentExitCode ?? (0 as int);
  Environment.Exit(resolved);
};

const normalizeSignal = (signal?: int | string): string => {
  if (signal === undefined) {
    return "SIGTERM";
  }

  if (typeof signal === "number") {
    return signal === 0 ? "0" : signal.toString();
  }

  return signal.toUpperCase();
};

export const kill = (targetPid: int, signal?: int | string): boolean => {
  try {
    const target = Process.GetProcessById(targetPid);
    const normalizedSignal = normalizeSignal(signal);

    switch (normalizedSignal) {
      case "0":
      case "SIGNULL":
        return !target.HasExited;
      case "SIGINT":
      case "SIGHUP":
        if (target.CloseMainWindow()) {
          return true;
        }
        target.Kill();
        return true;
      case "SIGKILL":
      case "SIGTERM":
      default:
        target.Kill();
        return true;
    }
  } catch (error) {
    if (error instanceof Error && error.name === "ArgumentException") {
      throw new Error(`kill ESRCH: No such process ${String(targetPid)}`);
    }

    if (error instanceof Error) {
      throw new Error(`kill failed: ${error.message}`);
    }

    throw new Error("kill failed");
  }
};

const getParentProcessId = (): int => {
  if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) {
    return 0 as int;
  }

  const statPath = `/proc/${String(pid)}/stat`;
  if (!File.Exists(statPath)) {
    return 0 as int;
  }

  const stat = File.ReadAllText(statPath);
  const parts = stat.split(" ");
  if (parts.length <= 3) {
    return 0 as int;
  }

  const parsed = Number.parseInt(parts[3] ?? "", 10);
  if (parsed === undefined || Number.isNaN(parsed)) {
    return 0 as int;
  }

  return toInt32(parsed) ?? (0 as int);
};

export class ProcessModule {
  get env(): ProcessEnv {
    return currentEnv;
  }

  get argv(): string[] {
    return currentArgv;
  }

  set argv(value: string[] | undefined) {
    currentArgv = value ?? [];
  }

  get argv0(): string {
    return currentArgv0;
  }

  set argv0(value: string | undefined) {
    currentArgv0 = value ?? "";
  }

  get pid(): int {
    return pid;
  }

  get execPath(): string {
    return execPath;
  }

  get arch(): string {
    return arch;
  }

  get platform(): string {
    return platform;
  }

  get ppid(): int {
    return getParentProcessId();
  }

  get version(): string {
    return version;
  }

  get versions(): ProcessVersions {
    return currentVersions;
  }

  get exitCode(): int | undefined {
    return currentExitCode;
  }

  set exitCode(value: int | undefined) {
    currentExitCode = value;
  }

  cwd(): string {
    return cwd();
  }

  chdir(directory: string): void {
    chdir(directory);
  }

  exit(code?: int): void {
    exit(code);
  }

  kill(pid: int, signal?: int | string): boolean {
    return kill(pid, signal);
  }
}

export const process = new ProcessModule();
