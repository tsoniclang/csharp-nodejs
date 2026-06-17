/**
 * Instances of ChildProcess represent spawned child processes.
 * Instances are not intended to be created directly.
 * Use spawn(), exec(), execFile(), or fork() to create instances.
 *
 */
import { Process } from "@tsonic/dotnet/System.Diagnostics.js";
import { EventEmitter } from "../events-module.ts";
import type { Readable } from "../stream/readable.ts";
import type { Writable } from "../stream/writable.ts";
import type { RuntimeValue } from "../runtime-value.ts";

/**
 * Options for exec, spawn, and related methods.
 *
 */
export class ExecOptions {
  /** Current working directory of the child process. */
  cwd: string | null = null;

  /** Environment variables to pass to the child process. */
  env: Map<string, string> | null = null;

  /**
   * Encoding to use for string output ('utf8', 'buffer', etc).
   * Default is 'buffer' (returns Uint8Array).
   */
  encoding: string | null = null;

  /**
   * Shell to execute the command with
   * (default: '/bin/sh' on Unix, 'cmd.exe' on Windows).
   */
  shell: string | null = null;

  /** Timeout in milliseconds (default: 0 = no timeout). */
  timeout: number = 0;

  /**
   * Largest amount of data in bytes allowed on stdout or stderr
   * (default: 1024*1024).
   */
  maxBuffer: number = 1024 * 1024;

  /** Signal to use to kill the process (default: 'SIGTERM'). */
  killSignal: string | null = null;

  /** Hide the subprocess console window on Windows (default: false). */
  windowsHide: boolean = false;

  /** No quoting or escaping of arguments on Windows (default: false). */
  windowsVerbatimArguments: boolean = false;

  /**
   * Prepare child to run independently of its parent process (Unix only).
   */
  detached: boolean = false;

  /** User identity of the process (Unix only). */
  uid: number | null = null;

  /** Group identity of the process (Unix only). */
  gid: number | null = null;

  /** Explicitly set the value of argv[0] sent to the child process. */
  argv0: string | null = null;

  /** stdio configuration ('pipe', 'inherit', 'ignore'). */
  stdio: string | null = null;

  /** Input to be sent to stdin (for sync methods). */
  input: string | null = null;
}

/**
 * Represents a spawned child process.
 *
 */
export class ChildProcess extends EventEmitter {
  _killed: boolean = false;
  _pid: number = -1;
  _process: Process | null = null;

  /**
   * A Writable Stream that represents the child process's stdin.
   * If the child was spawned with stdio[0] set to anything other than
   * 'pipe', this will be null.
   */
  stdin: Writable | null = null;

  /**
   * A Readable Stream that represents the child process's stdout.
   * If the child was spawned with stdio[1] set to anything other than
   * 'pipe', this will be null.
   */
  stdout: Readable | null = null;

  /**
   * A Readable Stream that represents the child process's stderr.
   * If the child was spawned with stdio[2] set to anything other than
   * 'pipe', this will be null.
   */
  stderr: Readable | null = null;

  /**
   * The process identifier (PID) of the child process.
   */
  get pid(): number {
    return this._pid;
  }

  /**
   * Indicates whether it is still possible to send and receive messages
   * from the child process.
   */
  connected: boolean = false;

  /**
   * Indicates whether the child process successfully received a signal
   * from kill().
   */
  get killed(): boolean {
    return this._killed;
  }

  /**
   * Indicates whether the child process is referenced (parent will wait
   * for it). When true, the parent process will wait for this child to
   * exit. When false (unreferenced), the parent can exit independently.
   */
  referenced: boolean = true;

  /**
   * The exit code of the child process. Returns null if the process has
   * not yet exited.
   */
  exitCode: number | null = null;

  /**
   * The signal by which the child process was terminated. Returns null
   * if not terminated by signal.
   */
  signalCode: string | null = null;

  /**
   * The full list of command-line arguments the child process was
   * launched with.
   */
  spawnargs: string[] = [];

  /**
   * The executable file name of the child process that is launched.
   */
  spawnfile: string = "";

  /**
   * The subprocess.kill() method sends a signal to the child process.
   *
   * @param signal - The signal to send (default: 'SIGTERM').
   * @returns True if the signal was sent successfully.
   */
  kill(signal?: string | null): boolean {
    const normalizedSignal = signal ?? "SIGTERM";

    if (this._process === null) {
      if (this.exitCode !== null || this.signalCode !== null) {
        return false;
      }

      this._killed = true;
      this.signalCode = normalizedSignal;
      this.connected = false;
      this.emit("exit", null, normalizedSignal);
      this.emit("close", null, normalizedSignal);
      return true;
    }

    if (this._process.HasExited) {
      this._syncFromProcessExit();
      return false;
    }

    this._process.Kill();
    this._process.WaitForExit();

    this._killed = true;
    this.signalCode = normalizedSignal;
    this.connected = false;
    this._syncFromProcessExit();
    this.emit("exit", null, normalizedSignal);
    this.emit("close", null, normalizedSignal);
    return true;
  }

  /** Closes the IPC channel between parent and child. */
  disconnect(): void {
    this.connected = false;
    this.emit("disconnect");
  }

  /**
   * When an IPC channel exists, the send() method sends messages to the
   * child process.
   *
   * @param message - The message to send.
   * @param sendHandle - Optional handle to send.
   * @param options - Optional send options.
   * @param callback - Optional callback when message is sent.
   * @returns True if message was queued successfully.
   */
  send(
    message: RuntimeValue,
    sendHandle?: RuntimeValue,
    options?: RuntimeValue,
    callback?: ((error: Error | null) => void) | null,
  ): boolean {
    void sendHandle;
    void options;
    if (!this.connected) {
      if (callback !== undefined && callback !== null) {
        callback(new Error("Channel closed"));
      }
      return false;
    }

    // TODO: IPC implementation
    if (callback !== undefined && callback !== null) {
      callback(null);
    }
    return true;
  }

  /**
   * Calling ref() on a child process will prevent the parent process
   * from exiting until the child exits. This is the default behavior.
   */
  ref(): void {
    this.referenced = true;
  }

  /**
   * Calling unref() on a child process will allow the parent process to
   * exit independently of the child. The child process will continue
   * running in the background.
   */
  unref(): void {
    this.referenced = false;
  }

  /**
   * Set the process identifier. Internal use only by spawn helpers.
   */
  _setPid(pid: number): void {
    this._pid = pid;
  }

  /**
   * Mark the child as killed. Internal use only by spawn helpers.
   */
  _setKilled(killed: boolean): void {
    this._killed = killed;
  }

  _attachProcess(process: Process): void {
    this._process = process;
    this._pid = process.Id;
  }

  _setSpawnInfo(file: string, args: string[]): void {
    this.spawnfile = file;
    this.spawnargs = [file, ...args];
  }

  _setConnected(connected: boolean): void {
    this.connected = connected;
  }

  _syncFromProcessExit(): void {
    if (this._process === null || !this._process.HasExited) {
      return;
    }

    this.exitCode = this._process.ExitCode;
    this.connected = false;
  }

  _recordSyntheticExit(
    exitCode: number | null,
    signalCode: string | null,
  ): void {
    this.exitCode = exitCode;
    this.signalCode = signalCode;
    this.connected = false;
  }
}
