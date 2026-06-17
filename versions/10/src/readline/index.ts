/**
 * Node.js readline module — provides an interface for reading data from a
 * Readable stream (such as process.stdin) one line at a time.
 *
 */

import type {} from "../type-bootstrap.ts";
import type { RuntimeValue } from "../runtime-value.ts";

import type { Readable } from "../stream/readable.ts";
import type { Writable } from "../stream/writable.ts";
import { Interface } from "./interface.ts";
import { InterfaceOptions, CursorPosition } from "./interface-options.ts";
import { ReadlinePromises } from "./promises.ts";

export { Interface } from "./interface.ts";
export { InterfaceOptions, CursorPosition } from "./interface-options.ts";
export { ReadlinePromises } from "./promises.ts";

const _promises = new ReadlinePromises();

/**
 * Promise-based readline helpers.
 */
export const promises: ReadlinePromises = _promises;

/**
 * Creates a new readline.Interface instance.
 */
export const createInterface = (
  optionsOrInput: InterfaceOptions | Readable,
  output?: Writable,
): Interface => {
  if (optionsOrInput === undefined || optionsOrInput === null) {
    throw new Error("options or input is required");
  }

  if (optionsOrInput instanceof InterfaceOptions) {
    if (optionsOrInput.input === undefined || optionsOrInput.input === null) {
      throw new Error("input stream is required");
    }
    return new Interface(optionsOrInput);
  } else {
    const options = new InterfaceOptions();
    options.input = optionsOrInput as Readable;
    options.output = output;
    return new Interface(options);
  }
};

/**
 * Clears the current line of the given TTY stream in a specified direction.
 *
 * @param stream - The TTY stream.
 * @param dir - Direction: -1 (left from cursor), 1 (right from cursor),
 *   0 (entire line).
 * @param callback - Optional callback invoked once the operation completes.
 * @returns True if stream is a TTY and function succeeded.
 */
export const clearLine = (
  stream: Writable | null,
  dir: number,
  callback?: () => void,
): boolean => {
  if (stream === undefined || stream === null) {
    throw new Error("stream is required");
  }

  try {
    // ANSI escape codes for clearing lines
    // ESC[1K - clear from cursor to beginning
    // ESC[2K - clear entire line
    // ESC[0K - clear from cursor to end
    let escapeCode: string;
    if (dir === -1) {
      escapeCode = "\x1B[1K"; // Clear left
    } else if (dir === 0) {
      escapeCode = "\x1B[2K"; // Clear entire line
    } else if (dir === 1) {
      escapeCode = "\x1B[0K"; // Clear right
    } else {
      throw new Error("dir must be -1, 0, or 1");
    }

    // TODO: Interactive I/O — writing escape codes to the stream requires a
    // connected TTY Writable with working write() at runtime.
    stream.write(escapeCode);
    if (callback !== undefined) {
      callback();
    }
    return true;
  } catch {
    if (callback !== undefined) {
      callback();
    }
    return false;
  }
};

/**
 * Clears the screen from the current cursor position downward.
 *
 * @param stream - The TTY stream.
 * @param callback - Optional callback invoked once the operation completes.
 * @returns True if stream is a TTY and function succeeded.
 */
export const clearScreenDown = (
  stream: Writable | null,
  callback?: () => void,
): boolean => {
  if (stream === undefined || stream === null) {
    throw new Error("stream is required");
  }

  try {
    // ANSI escape code ESC[0J - clear from cursor down
    // TODO: Interactive I/O — writing escape codes to the stream requires a
    // connected TTY Writable with working write() at runtime.
    stream.write("\x1B[0J");
    if (callback !== undefined) {
      callback();
    }
    return true;
  } catch {
    if (callback !== undefined) {
      callback();
    }
    return false;
  }
};

/**
 * Moves the cursor to the specified position in the given TTY stream.
 *
 * @param stream - The TTY stream.
 * @param x - The column position (0-based).
 * @param y - Optional row position (0-based).
 * @param callback - Optional callback invoked once the operation completes.
 * @returns True if stream is a TTY and function succeeded.
 */
export const cursorTo = (
  stream: Writable | null,
  x: number,
  y?: number | null,
  callback?: () => void,
): boolean => {
  if (stream === undefined || stream === null) {
    throw new Error("stream is required");
  }

  try {
    // ANSI escape codes for cursor positioning
    // ESC[{row};{col}H - move to absolute position (1-based)
    // ESC[{col}G - move to column (1-based)
    const escapeCode =
      y !== undefined && y !== null
        ? `\x1B[${String(y + 1)};${String(x + 1)}H` // Both row and column (convert to 1-based)
        : `\x1B[${String(x + 1)}G`; // Column only (convert to 1-based)

    // TODO: Interactive I/O — writing escape codes to the stream requires a
    // connected TTY Writable with working write() at runtime.
    stream.write(escapeCode);
    if (callback !== undefined) {
      callback();
    }
    return true;
  } catch {
    if (callback !== undefined) {
      callback();
    }
    return false;
  }
};

/**
 * Moves the cursor relative to its current position in the given TTY stream.
 *
 * @param stream - The TTY stream.
 * @param dx - Horizontal movement (positive = right, negative = left).
 * @param dy - Vertical movement (positive = down, negative = up).
 * @param callback - Optional callback invoked once the operation completes.
 * @returns True if stream is a TTY and function succeeded.
 */
export const moveCursor = (
  stream: Writable | null,
  dx: number,
  dy: number,
  callback?: () => void,
): boolean => {
  if (stream === undefined || stream === null) {
    throw new Error("stream is required");
  }

  try {
    let result = "";

    // Horizontal movement
    if (dx < 0) {
      // Move left: ESC[{n}D
      result += `\x1B[${String(-dx)}D`;
    } else if (dx > 0) {
      // Move right: ESC[{n}C
      result += `\x1B[${String(dx)}C`;
    }

    // Vertical movement
    if (dy < 0) {
      // Move up: ESC[{n}A
      result += `\x1B[${String(-dy)}A`;
    } else if (dy > 0) {
      // Move down: ESC[{n}B
      result += `\x1B[${String(dy)}B`;
    }

    if (result.length > 0) {
      // TODO: Interactive I/O — writing escape codes to the stream requires a
      // connected TTY Writable with working write() at runtime.
      stream.write(result);
    }

    if (callback !== undefined) {
      callback();
    }
    return true;
  } catch {
    if (callback !== undefined) {
      callback();
    }
    return false;
  }
};

/**
 * Configures a readable stream to emit keypress events on the given interface.
 *
 * @param stream - The readable stream to listen on.
 * @param rl - Optional readline interface to emit keypress events to.
 */
export const emitKeypressEvents = (
  stream: Readable,
  rl?: Interface,
): void => {
  if (stream === undefined || stream === null) {
    throw new Error("stream is required");
  }

  // TODO: Interactive I/O — listening for 'data' events on stdin and
  // translating them into 'keypress' events requires runtime stream
  // integration.
  stream.on("data", (chunk: RuntimeValue) => {
    if (rl === undefined) {
      return;
    }
    const key = chunk !== undefined && chunk !== null ? String(chunk) : "";
    rl.emit("keypress", key, key);
  });
};
