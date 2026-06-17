
import type {} from "./type-bootstrap.ts";

import { Console as DotnetConsole } from "@tsonic/dotnet/System.js";
import { Stopwatch } from "@tsonic/dotnet/System.Diagnostics.js";
import type { RuntimeValue } from "./runtime-value.ts";

import * as util from "./util-module.ts";

const counters = new Map<string, number>();
const timers = new Map<string, Stopwatch>();
let groupIndentation = 0;
const groupIndentationSize = 2;

const withIndent = (message: string): string => {
  if (groupIndentation <= 0) {
    return message;
  }

  return `${" ".repeat(groupIndentation * groupIndentationSize)}${message}`;
};

const writeLine = (message: string): void => {
  DotnetConsole.WriteLine(withIndent(message));
};

const writeError = (message: string): void => {
  DotnetConsole.Error.WriteLine(withIndent(message));
};

const formatConsoleMessage = (
  message?: RuntimeValue,
  optionalParams: readonly RuntimeValue[] = []
): string => {
  let result = "";
  let hasValue = false;

  if (message !== undefined) {
    result = util.inspect(message);
    hasValue = true;
  }

  for (let index = 0; index < optionalParams.length; index += 1) {
    if (hasValue) {
      result += " ";
    }
    result += util.inspect(optionalParams[index]!);
    hasValue = true;
  }

  return result;
};

const formatElapsed = (stopwatch: Stopwatch): string => {
  const elapsed = stopwatch.Elapsed;
  if (elapsed.TotalMilliseconds < 1) {
    return `${String(elapsed.TotalMilliseconds)}ms`;
  }
  if (elapsed.TotalSeconds < 1) {
    return `${String(Math.round(elapsed.TotalMilliseconds))}ms`;
  }
  if (elapsed.TotalMinutes < 1) {
    return `${String(Math.round(elapsed.TotalSeconds * 1000) / 1000)}s`;
  }
  return `${String(Math.round(elapsed.TotalMinutes * 100) / 100)}m`;
};

export class ConsoleConstructor {
  constructor(
    _stdout?: object,
    _stderr?: object,
    _ignoreErrors: boolean = true,
    _colorMode?: object,
    _inspectOptions?: object,
    _groupIndentation: boolean = true
  ) {}

  assert(value: boolean, message?: string, ...optionalParams: RuntimeValue[]): void {
    consoleModuleInstance.assert(value, message, ...optionalParams);
  }
  clear(): void {
    consoleModuleInstance.clear();
  }
  count(label?: string): void {
    consoleModuleInstance.count(label);
  }
  countReset(label?: string): void {
    consoleModuleInstance.countReset(label);
  }
  debug(message?: RuntimeValue, ...optionalParams: RuntimeValue[]): void {
    consoleModuleInstance.debug(message, ...optionalParams);
  }
  dir(value?: RuntimeValue, ...options: RuntimeValue[]): void {
    consoleModuleInstance.dir(value, ...options);
  }
  dirxml(...data: RuntimeValue[]): void {
    consoleModuleInstance.dirxml(...data);
  }
  error(message?: RuntimeValue, ...optionalParams: RuntimeValue[]): void {
    consoleModuleInstance.error(message, ...optionalParams);
  }
  group(...label: RuntimeValue[]): void {
    consoleModuleInstance.group(...label);
  }
  groupCollapsed(...label: RuntimeValue[]): void {
    consoleModuleInstance.groupCollapsed(...label);
  }
  groupEnd(): void {
    consoleModuleInstance.groupEnd();
  }
  info(message?: RuntimeValue, ...optionalParams: RuntimeValue[]): void {
    consoleModuleInstance.info(message, ...optionalParams);
  }
  log(message?: RuntimeValue, ...optionalParams: RuntimeValue[]): void {
    consoleModuleInstance.log(message, ...optionalParams);
  }
  profile(label?: string): void {
    consoleModuleInstance.profile(label);
  }
  profileEnd(label?: string): void {
    consoleModuleInstance.profileEnd(label);
  }
  table(tabularData?: RuntimeValue, properties?: string[]): void {
    consoleModuleInstance.table(tabularData, properties);
  }
  time(label?: string): void {
    consoleModuleInstance.time(label);
  }
  timeEnd(label?: string): void {
    consoleModuleInstance.timeEnd(label);
  }
  timeLog(label?: string, ...data: RuntimeValue[]): void {
    consoleModuleInstance.timeLog(label, ...data);
  }
  timeStamp(label?: string): void {
    consoleModuleInstance.timeStamp(label);
  }
  trace(message?: RuntimeValue, ...optionalParams: RuntimeValue[]): void {
    consoleModuleInstance.trace(message, ...optionalParams);
  }
  warn(message?: RuntimeValue, ...optionalParams: RuntimeValue[]): void {
    consoleModuleInstance.warn(message, ...optionalParams);
  }
}

class ConsoleModule {
  Console: ConsoleConstructor = new ConsoleConstructor();

  assert(
    value: boolean,
    message?: string,
    ...optionalParams: RuntimeValue[]
  ): void {
    if (value) {
      return;
    }

    let fullMessage = "Assertion failed";
    if (message !== undefined && message.length > 0) {
      fullMessage += `: ${util.format(message, ...optionalParams)}`;
    }
    writeError(fullMessage);
  }

  clear(): void {
    try {
      DotnetConsole.Clear();
    } catch {
      return;
    }
  }

  count(label: string = "default"): void {
    const next = (counters.get(label) ?? 0) + 1;
    counters.set(label, next);
    writeLine(`${label}: ${String(next)}`);
  }

  countReset(label: string = "default"): void {
    counters.set(label, 0);
  }

  debug(message?: RuntimeValue, ...optionalParams: RuntimeValue[]): void {
    this.log(message, ...optionalParams);
  }

  dir(value?: RuntimeValue, ..._options: RuntimeValue[]): void {
    writeLine(util.inspect(value));
  }

  dirxml(...data: RuntimeValue[]): void {
    this.log(undefined, ...data);
  }

  error(message?: RuntimeValue, ...optionalParams: RuntimeValue[]): void {
    writeError(formatConsoleMessage(message, optionalParams));
  }

  group(...label: RuntimeValue[]): void {
    if (label.length > 0) {
      let rendered = util.inspect(label[0]!);
      for (let index = 1; index < label.length; index += 1) {
        rendered += ` ${util.inspect(label[index]!)}`;
      }
      writeLine(rendered);
    }
    groupIndentation += 1;
  }

  groupCollapsed(...label: RuntimeValue[]): void {
    this.group(...label);
  }

  groupEnd(): void {
    if (groupIndentation > 0) {
      groupIndentation -= 1;
    }
  }

  info(message?: RuntimeValue, ...optionalParams: RuntimeValue[]): void {
    this.log(message, ...optionalParams);
  }

  log(message?: RuntimeValue, ...optionalParams: RuntimeValue[]): void {
    writeLine(formatConsoleMessage(message, optionalParams));
  }

  table(tabularData?: RuntimeValue, _properties?: string[]): void {
    writeLine(util.inspect(tabularData));
  }

  time(label: string = "default"): void {
    if (timers.has(label)) {
      return;
    }

    const stopwatch = new Stopwatch();
    stopwatch.Start();
    timers.set(label, stopwatch);
  }

  timeEnd(label: string = "default"): void {
    const stopwatch = timers.get(label);
    if (stopwatch === undefined) {
      return;
    }

    stopwatch.Stop();
    writeLine(`${label}: ${formatElapsed(stopwatch)}`);
    timers.delete(label);
  }

  timeLog(label: string = "default", ...data: RuntimeValue[]): void {
    const stopwatch = timers.get(label);
    if (stopwatch === undefined) {
      return;
    }

    let extras = "";
    for (let index = 0; index < data.length; index += 1) {
      extras += ` ${util.inspect(data[index]!)}`;
    }
    writeLine(`${label}: ${formatElapsed(stopwatch)}${extras}`);
  }

  trace(message?: RuntimeValue, ...optionalParams: RuntimeValue[]): void {
    const rendered = formatConsoleMessage(message, optionalParams);
    writeError(`Trace: ${rendered}`);
  }

  warn(message?: RuntimeValue, ...optionalParams: RuntimeValue[]): void {
    this.error(message, ...optionalParams);
  }

  profile(_label?: string): void {}

  profileEnd(_label?: string): void {}

  timeStamp(_label?: string): void {}
}

const consoleModuleInstance = new ConsoleModule();

export const console = consoleModuleInstance;
export const Console: ConsoleConstructor = consoleModuleInstance.Console;
