/**
 * Node.js child_process module.
 *
 */

import type {} from "../type-bootstrap.ts";

import { overloads as O } from "@tsonic/core/lang.js";
import { Process, ProcessStartInfo } from "@tsonic/dotnet/System.Diagnostics.js";
import { Convert } from "@tsonic/dotnet/System.js";
import {
  OSPlatform,
  RuntimeInformation,
} from "@tsonic/dotnet/System.Runtime.InteropServices.js";

import { stringToBytes } from "../buffer/buffer-encoding.ts";
import { errorFromUnknown } from "../error-from-unknown.ts";
import { ChildProcess, ExecOptions } from "./child-process.ts";
import { SpawnSyncReturns } from "./spawn-sync-returns.ts";

export { ChildProcess, ExecOptions } from "./child-process.ts";
export { SpawnSyncReturns } from "./spawn-sync-returns.ts";

type ProcessRunResult = {
  pid: number;
  stdout: string;
  stderr: string;
  status: number | null;
  signal: string | null;
  error: Error | null;
};

const isWindows = (): boolean =>
  RuntimeInformation.IsOSPlatform(OSPlatform.Windows);

const normalizeArgs = (args?: string[] | null): string[] => args ?? [];

const normalizeSignal = (options?: ExecOptions | null): string =>
  options?.killSignal ?? "SIGTERM";

const escapeShellArg = (value: string): string =>
  value.includes(" ") || value.includes("\"") || value.includes("'")
    ? `"${value.split("\\").join("\\\\").split("\"").join("\\\"")}"`
    : value;

const buildArgumentsString = (args: readonly string[]): string =>
  args.map(escapeShellArg).join(" ");

const configureStartInfo = (
  startInfo: ProcessStartInfo,
  options?: ExecOptions | null,
  redirectInput: boolean = false,
): void => {
  startInfo.UseShellExecute = false;
  startInfo.CreateNoWindow = options?.windowsHide ?? true;
  startInfo.RedirectStandardOutput = true;
  startInfo.RedirectStandardError = true;
  startInfo.RedirectStandardInput = redirectInput;

  if (options?.cwd !== null && options?.cwd !== undefined) {
    startInfo.WorkingDirectory = options.cwd;
  }

  if (options?.env !== null && options?.env !== undefined) {
    for (const key of options.env.keys()) {
      const value = options.env.get(key);
      if (value !== undefined) {
        startInfo.EnvironmentVariables.Add(key, value);
      }
    }
  }
};

const createShellStartInfo = (
  command: string,
  options?: ExecOptions | null,
): ProcessStartInfo => {
  const shell = options?.shell ?? (isWindows() ? "cmd.exe" : "/bin/sh");
  const startInfo = new ProcessStartInfo(shell);

  if (isWindows()) {
    startInfo.Arguments = `/c ${command}`;
  } else {
    startInfo.Arguments = `-c ${escapeShellArg(command)}`;
  }

  configureStartInfo(startInfo, options, options?.input !== null && options?.input !== undefined);
  return startInfo;
};

const createExecutableStartInfo = (
  file: string,
  args?: string[] | null,
  options?: ExecOptions | null,
): ProcessStartInfo => {
  const startInfo = new ProcessStartInfo(file);
  startInfo.Arguments = buildArgumentsString(normalizeArgs(args));

  configureStartInfo(startInfo, options, options?.input !== null && options?.input !== undefined);
  return startInfo;
};

const startProcess = (startInfo: ProcessStartInfo): Process => {
  const process = Process.Start(startInfo);
  if (process === null) {
    throw new Error(`Failed to start process: ${startInfo.FileName}`);
  }
  return process;
};

const finishSyncProcess = (
  process: Process,
  options?: ExecOptions | null,
): ProcessRunResult => {
  if (options?.input !== null && options?.input !== undefined) {
    process.StandardInput.Write(options.input);
    process.StandardInput.Close();
  }

  let signal: string | null = null;
  let error: Error | null = null;

  const timeout = Convert.ToInt32(options?.timeout ?? 0);
  if (timeout > 0) {
    const exited = process.WaitForExit(timeout);
    if (!exited) {
      signal = normalizeSignal(options);
      process.Kill();
      process.WaitForExit();
      error = new Error(`Process timed out after ${String(timeout)}ms`);
    }
  } else {
    process.WaitForExit();
  }

  const stdout = process.StandardOutput.ReadToEnd();
  const stderr = process.StandardError.ReadToEnd();
  const status = signal === null ? process.ExitCode : null;

  if (error === null && status !== null && status !== 0) {
    error = new Error(stderr.length > 0 ? stderr : `Process exited with code ${String(status)}`);
  }

  return {
    pid: process.Id,
    stdout,
    stderr,
    status,
    signal,
    error,
  };
};

const toStringResult = (
  command: string,
  args: string[] | null | undefined,
  options: ExecOptions | null | undefined,
): SpawnSyncReturns<string> => {
  const result = new SpawnSyncReturns<string>("");

  try {
    const process = startProcess(createExecutableStartInfo(command, args, options));
    const finished = finishSyncProcess(process, options);
    result.pid = finished.pid;
    result.stdout = finished.stdout;
    result.stderr = finished.stderr;
    result.output = [null, finished.stdout, finished.stderr];
    result.status = finished.status;
    result.signal = finished.signal;
    result.error = finished.error;
  } catch (error) {
    result.error = errorFromUnknown(error, "Failed to spawn process");
  }

  return result;
};

const toBinaryResult = (
  command: string,
  args: string[] | null | undefined,
  options: ExecOptions | null | undefined,
): SpawnSyncReturns<Uint8Array> => {
  const empty = new Uint8Array(0);
  const result = new SpawnSyncReturns<Uint8Array>(empty);

  try {
    const process = startProcess(createExecutableStartInfo(command, args, options));
    const finished = finishSyncProcess(process, options);
    result.pid = finished.pid;
    result.stdout = stringToBytes(finished.stdout, "utf8");
    result.stderr = stringToBytes(finished.stderr, "utf8");
    result.output = [null, result.stdout, result.stderr];
    result.status = finished.status;
    result.signal = finished.signal;
    result.error = finished.error;
  } catch (error) {
    result.error = errorFromUnknown(error, "Failed to spawn process");
  }

  return result;
};

const execWithShell = (
  command: string,
  options?: ExecOptions | null,
): ProcessRunResult => {
  const process = startProcess(createShellStartInfo(command, options));
  return finishSyncProcess(process, options);
};

const coerceExecOutput = (
  stdout: string,
  options?: ExecOptions | null,
): string | Uint8Array => {
  const encoding = options?.encoding ?? "buffer";
  return encoding === "buffer" ? stringToBytes(stdout, "utf8") : stdout;
};

// ==================== execSync ====================

export const execSync = (
  command: string,
  options?: ExecOptions | null,
): string | Uint8Array => {
  const finished = execWithShell(command, options);
  if (finished.error !== null) {
    throw finished.error;
  }

  return coerceExecOutput(finished.stdout, options);
};

// ==================== spawnSync ====================

export const spawnSync = (
  command: string,
  args?: string[] | null,
  options?: ExecOptions | null,
): SpawnSyncReturns<Uint8Array> => {
  return toBinaryResult(command, args, options);
};

export const spawnSyncString = (
  command: string,
  args?: string[] | null,
  options?: ExecOptions | null,
): SpawnSyncReturns<string> => {
  return toStringResult(command, args, options);
};

// ==================== execFileSync ====================

export const execFileSync = (
  file: string,
  args?: string[] | null,
  options?: ExecOptions | null,
): string | Uint8Array => {
  if (
    options?.encoding !== null &&
    options?.encoding !== undefined &&
    options.encoding !== "buffer"
  ) {
    const stringResult = toStringResult(file, args, options);
    if (stringResult.error !== null) {
      throw stringResult.error;
    }
    return stringResult.stdout;
  }

  const binaryResult = toBinaryResult(file, args, options);
  if (binaryResult.error !== null) {
    throw binaryResult.error;
  }
  return binaryResult.stdout;
};

// ==================== Async Methods ====================

export function exec(
  command: string,
  callback: (error: Error | null, stdout: string, stderr: string) => void,
): void;
export function exec(
  command: string,
  options: ExecOptions | null,
  callback: (error: Error | null, stdout: string, stderr: string) => void,
): void;
export function exec(
  _command: any,
  _optionsOrCallback: any,
  _callback?: any,
): any {
  throw new Error("Unreachable overload stub");
}

function exec_callback(
  command: string,
  callback: (error: Error | null, stdout: string, stderr: string) => void,
): void {
  try {
    const finished = execWithShell(command, null);
    callback(finished.error, finished.stdout, finished.stderr);
  } catch (error) {
    const resolvedError: Error = errorFromUnknown(error, "Failed to execute process");
    callback(resolvedError, "", "");
  }
}

function exec_options(
  command: string,
  options: ExecOptions | null,
  callback: (error: Error | null, stdout: string, stderr: string) => void,
): void {
  try {
    const finished = execWithShell(command, options);
    callback(finished.error, finished.stdout, finished.stderr);
  } catch (error) {
    const resolvedError: Error = errorFromUnknown(error, "Failed to execute process");
    callback(resolvedError, "", "");
  }
}

export const spawn = (
  command: string,
  args?: string[] | null,
  options?: ExecOptions | null,
): ChildProcess => {
  const child = new ChildProcess();
  const resolvedArgs = normalizeArgs(args);
  child._setSpawnInfo(command, resolvedArgs);

  const process = startProcess(createExecutableStartInfo(command, resolvedArgs, options));
  child._attachProcess(process);
  child.emit("spawn");

  if (process.WaitForExit(25)) {
    child._syncFromProcessExit();
    child.emit("exit", child.exitCode, null);
    child.emit("close", child.exitCode, null);
  }

  return child;
};

export const execFile = (
  file: string,
  args: string[] | null,
  options: ExecOptions | null,
  callback: (error: Error | null, stdout: string, stderr: string) => void,
): void => {
  const result = toStringResult(file, args, options);
  callback(result.error, result.stdout, result.stderr);
};

export const fork = (
  modulePath: string,
  args?: string[] | null,
  options?: ExecOptions | null,
): ChildProcess => {
  const child = spawn(modulePath, args, options);
  child._setConnected(true);
  return child;
};

O(exec_callback).family(exec);
O(exec_options).family(exec);
