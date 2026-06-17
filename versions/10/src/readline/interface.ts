/**
 * The Interface class represents a readline interface with an input and output
 * stream. Extends EventEmitter to emit events like 'line', 'close', 'pause',
 * 'resume', etc.
 *
 */
import { EventEmitter } from "../events-module.ts";
import type { int } from "@tsonic/core/types.js";
import type { Readable } from "../stream/readable.ts";
import type { Writable } from "../stream/writable.ts";
import { InterfaceOptions, CursorPosition } from "./interface-options.ts";
import type { RuntimeValue } from "../runtime-value.ts";

export class Interface extends EventEmitter {
  _input: Readable | undefined;
  _output: Writable | undefined;
  _terminal: boolean;
  _prompt: string = "> ";
  _history: string[] = [];
  _historySize: int;
  _removeHistoryDuplicates: boolean;
  _line: string = "";
  _cursor: int = 0;
  _closed: boolean = false;
  _paused: boolean = false;
  _dataListener: ((...args: RuntimeValue[]) => void) | undefined;
  _endListener: ((...args: RuntimeValue[]) => void) | undefined;
  _historyIndex: int = -1;
  _savedLine: string = "";

  /** Current line being processed. */
  get line(): string {
    return this._line;
  }

  /** Cursor position in current line. */
  get cursor(): int {
    return this._cursor;
  }

  constructor(options: InterfaceOptions) {
    super();

    this._input = options.input;
    this._output = options.output;
    this._terminal = options.terminal ?? false;
    this._prompt = options.prompt ?? "> ";
    this._historySize = options.historySize ?? 30;
    this._removeHistoryDuplicates = options.removeHistoryDuplicates ?? false;

    // Initialize history if provided
    if (options.history !== undefined) {
      for (const entry of options.history) {
        this._history.push(entry);
      }
    }

    // Set up input stream listeners
    // TODO: Interactive I/O — stdin/stdout listeners require runtime stream
    // integration. The listener wiring below is structurally correct but
    // depends on Readable emitting 'data' and 'end' events at runtime.
    if (this._input !== undefined) {
      const dataListener = (...args: RuntimeValue[]): void => {
        if (!this._paused && args.length > 0 && args[0] !== undefined && args[0] !== null) {
          this.processInput(String(args[0]));
        }
      };

      const endListener = (..._args: RuntimeValue[]): void => {
        if (!this._closed) {
          this.close();
        }
      };

      this._dataListener = dataListener;
      this._endListener = endListener;

      this._input.on("data", dataListener);
      this._input.on("end", endListener);
    }
  }

  /**
   * Prompts the user for input by writing the configured prompt string to
   * output.
   */
  prompt(preserveCursor?: boolean): void {
    if (this._closed) {
      throw new Error("Cannot prompt on closed interface");
    }

    // TODO: Interactive I/O — writing the prompt to the output stream requires
    // a connected Writable with working write() at runtime.
    if (this._output !== undefined && this._prompt.length > 0) {
      this._output.write(this._prompt);
    }

    if (preserveCursor !== true) {
      this._cursor = 0;
    }
  }

  /**
   * Prompts the user with a query string and calls the callback with the
   * user's response.
   */
  question(query: string, callback: (answer: string) => void): void {
    if (this._closed) {
      throw new Error("Cannot question on closed interface");
    }

    if (callback === undefined || callback === null) {
      throw new Error("callback is required");
    }

    // TODO: Interactive I/O — writing the query to the output stream requires
    // a connected Writable with working write() at runtime.
    if (this._output !== undefined) {
      this._output.write(query);
    }

    // Set up a one-time listener for the next line
    const lineListener = (...args: RuntimeValue[]): void => {
      if (args.length > 0 && args[0] !== undefined && args[0] !== null) {
        callback(String(args[0]));
      }
    };

    this.once("line", lineListener);
  }

  /**
   * Prompts the user with a query string and returns a Promise that resolves
   * with the user's response. Promise-based version of question().
   */
  questionAsync(query: string): Promise<string> {
    if (this._closed) {
      throw new Error("Cannot question on closed interface");
    }

    // TODO: Interactive I/O — writing the query to the output stream requires
    // a connected Writable with working write() at runtime.
    if (this._output !== undefined) {
      this._output.write(query);
    }

    return new Promise<string>((resolve) => {
      const lineListener = (...args: RuntimeValue[]): void => {
        if (args.length > 0 && args[0] !== undefined && args[0] !== null) {
          resolve(String(args[0]));
        }
      };

      this.once("line", lineListener);
    });
  }

  /**
   * Writes data to output stream or simulates keypresses.
   */
  write(data: RuntimeValue, key?: RuntimeValue): void {
    if (this._closed) {
      throw new Error("Cannot write on closed interface");
    }

    if (data !== undefined && data !== null) {
      const text = String(data);
      // TODO: Interactive I/O — writing to output stream requires a connected
      // Writable with working write() at runtime.
      if (this._output !== undefined) {
        this._output.write(text);
      }
    }

    if (key !== undefined && key !== null) {
      // Simulate keypress (simplified)
      this.processInput(String(key));
    }
  }

  /**
   * Pauses the input stream, allowing it to be resumed later.
   */
  pause(): Interface {
    if (this._closed || this._paused) {
      return this;
    }

    this._paused = true;
    if (this._input !== undefined) {
      this._input.pause();
    }
    this.emit("pause");
    return this;
  }

  /**
   * Resumes the input stream if it has been paused.
   */
  resume(): Interface {
    if (this._closed || !this._paused) {
      return this;
    }

    this._paused = false;
    if (this._input !== undefined) {
      this._input.resume();
    }
    this.emit("resume");
    return this;
  }

  /**
   * Closes the Interface instance and relinquishes control over input/output
   * streams.
   */
  close(): void {
    if (this._closed) {
      return;
    }

    this._closed = true;

    // Remove input stream listeners
    if (this._input !== undefined) {
      if (this._dataListener !== undefined) {
        this._input.removeListener("data", this._dataListener);
      }
      if (this._endListener !== undefined) {
        this._input.removeListener("end", this._endListener);
      }
    }

    this.emit("close");
  }

  /**
   * Sets the prompt string that will be written to output when prompt() is
   * called.
   */
  setPrompt(prompt: string): void {
    this._prompt = prompt ?? "";
  }

  /**
   * Returns the current prompt used by the interface.
   */
  getPrompt(): string {
    return this._prompt;
  }

  /**
   * Returns the real position of the cursor in relation to the input
   * prompt + string.
   */
  getCursorPos(): CursorPosition {
    // Simplified: just return cursor position relative to current line
    const promptLength = this._prompt.length;
    const totalLength = promptLength + this._cursor;

    // Assume 80 column terminal
    const pos = new CursorPosition();
    pos.cols = totalLength % 80;
    pos.rows = (totalLength / 80) | 0;
    return pos;
  }

  processInput(input: string): void {
    let i = 0;
    while (i < input.length) {
      const ch = input.charAt(i);
      const code = input.charCodeAt(i);

      // Handle ANSI escape sequences (arrow keys, delete, home, end, etc.)
      if (
        ch === "\x1B" &&
        i + 1 < input.length &&
        input.charAt(i + 1) === "["
      ) {
        i += 2; // Skip ESC and [

        if (i < input.length) {
          const escCode = input.charAt(i);

          if (escCode === "A") {
            // Up arrow - previous history
            this.navigateHistory(true);
          } else if (escCode === "B") {
            // Down arrow - next history
            this.navigateHistory(false);
          } else if (escCode === "C") {
            // Right arrow - move cursor right
            if (this._cursor < this._line.length) {
              this._cursor += 1;
            }
          } else if (escCode === "D") {
            // Left arrow - move cursor left
            if (this._cursor > 0) {
              this._cursor -= 1;
            }
          } else if (escCode === "H") {
            // Home - move to beginning
            this._cursor = 0;
          } else if (escCode === "F") {
            // End - move to end
            this._cursor = this._line.length;
          } else if (escCode === "3") {
            // Delete key (ESC[3~)
            if (i + 1 < input.length && input.charAt(i + 1) === "~") {
              i += 1; // Skip ~
              if (this._cursor < this._line.length) {
                this._line =
                  this._line.substring(0, this._cursor) +
                  this._line.substring(this._cursor + 1);
              }
            }
          }
        }

        i += 1;
        continue;
      }

      // Handle control characters
      if (code < 32 && ch !== "\n" && ch !== "\r" && ch !== "\t" && ch !== "\b") {
        if (code === 0x01) {
          // Ctrl+A - move to beginning
          this._cursor = 0;
          i += 1;
          continue;
        }
        if (code === 0x05) {
          // Ctrl+E - move to end
          this._cursor = this._line.length;
          i += 1;
          continue;
        }
        if (code === 0x15) {
          // Ctrl+U - clear line before cursor
          this._line = this._line.substring(this._cursor);
          this._cursor = 0;
          i += 1;
          continue;
        }
        if (code === 0x0b) {
          // Ctrl+K - clear line after cursor
          this._line = this._line.substring(0, this._cursor);
          i += 1;
          continue;
        }
        if (code === 0x17) {
          // Ctrl+W - delete word before cursor
          this.deleteWordBeforeCursor();
          i += 1;
          continue;
        }
        // Other control characters: skip
        i += 1;
        continue;
      }

      // Handle regular characters
      if (ch === "\n" || ch === "\r") {
        // Line complete
        const completedLine = this._line;

        // Add to history if not empty
        if (completedLine.trim().length > 0) {
          this.addToHistory(completedLine);
        }

        // Emit line event
        this.emit("line", completedLine);

        // Reset line buffer and history navigation
        this._line = "";
        this._cursor = 0;
        this._historyIndex = -1;
        this._savedLine = "";
      } else if (ch === "\b" || code === 0x7f) {
        // Backspace or DEL
        if (this._cursor > 0) {
          this._line =
            this._line.substring(0, this._cursor - 1) +
            this._line.substring(this._cursor);
          this._cursor -= 1;
        }
      } else if (ch === "\t") {
        // Tab - insert spaces
        this._line =
          this._line.substring(0, this._cursor) +
          "    " +
          this._line.substring(this._cursor);
        this._cursor += 4;
      } else if (code >= 32) {
        // Printable characters
        this._line =
          this._line.substring(0, this._cursor) +
          ch +
          this._line.substring(this._cursor);
        this._cursor += 1;

        // Reset history navigation when typing
        this._historyIndex = -1;
      }

      i += 1;
    }
  }

  navigateHistory(previous: boolean): void {
    if (this._history.length === 0) {
      return;
    }

    // Save current line if starting history navigation
    if (this._historyIndex === -1) {
      this._savedLine = this._line;
    }

    if (previous) {
      // Navigate to previous (older) history entry
      if (this._historyIndex < this._history.length - 1) {
        this._historyIndex += 1;
        this._line =
          this._history[this._history.length - 1 - this._historyIndex]!;
        this._cursor = this._line.length;
      }
    } else {
      // Navigate to next (newer) history entry
      if (this._historyIndex > 0) {
        this._historyIndex -= 1;
        this._line =
          this._history[this._history.length - 1 - this._historyIndex]!;
        this._cursor = this._line.length;
      } else if (this._historyIndex === 0) {
        // Restore saved line
        this._historyIndex = -1;
        this._line = this._savedLine;
        this._cursor = this._line.length;
      }
    }
  }

  deleteWordBeforeCursor(): void {
    if (this._cursor === 0) {
      return;
    }

    // Find the start of the word
    let wordStart = this._cursor - 1;

    // Skip trailing whitespace
    while (wordStart >= 0 && this._line.charAt(wordStart) === " ") {
      wordStart -= 1;
    }

    // Find the beginning of the word
    while (wordStart >= 0 && this._line.charAt(wordStart) !== " ") {
      wordStart -= 1;
    }

    wordStart += 1; // Move to first character of word

    // Delete from wordStart to cursor
    const deleteCount = this._cursor - wordStart;
    this._line =
      this._line.substring(0, wordStart) +
      this._line.substring(wordStart + deleteCount);
    this._cursor = wordStart;
  }

  addToHistory(line: string): void {
    if (this._removeHistoryDuplicates) {
      // Remove existing duplicates
      const filtered: string[] = [];
      for (const h of this._history) {
        if (h !== line) {
          filtered.push(h);
        }
      }
      while (this._history.length > 0) {
        this._history.pop();
      }
      for (let index = 0; index < filtered.length; index += 1) {
        this._history.push(filtered[index]!);
      }
    }

    this._history.push(line);

    // Trim history if it exceeds size limit
    while (this._history.length > this._historySize) {
      this._history.shift();
    }
  }
}
