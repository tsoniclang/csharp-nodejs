using System;
using System.IO;
using System.Text;

namespace Tsonic.CSharp.Node;

/// <summary>
/// The readline module provides an interface for reading data from a Readable stream
/// (such as process.stdin) one line at a time.
/// </summary>
public static class readline
{
    private static readonly ReadlinePromises _promises = new();

    /// <summary>
    /// Promise-based readline helpers.
    /// </summary>
    public static ReadlinePromises promises => _promises;

    /// <summary>
    /// Creates a new readline.Interface instance.
    /// </summary>
    /// <param name="options">Configuration options for the interface.</param>
    /// <returns>A new Interface instance.</returns>
    public static Interface createInterface(InterfaceOptions options)
    {
        if (options == null)
            throw new ArgumentNullException(nameof(options));

        if (options.input == null)
            throw new ArgumentException("input stream is required", nameof(options));

        return new Interface(options);
    }

    /// <summary>
    /// Creates a new readline.Interface instance with input and output streams.
    /// </summary>
    /// <param name="input">The input readable stream.</param>
    /// <param name="output">The output writable stream.</param>
    /// <returns>A new Interface instance.</returns>
    public static Interface createInterface(Readable input, Writable? output = null)
    {
        return createInterface(new InterfaceOptions
        {
            input = input,
            output = output
        });
    }

    /// <summary>
    /// Clears the current line of the given TTY stream in a specified direction.
    /// </summary>
    /// <param name="stream">The TTY stream.</param>
    /// <param name="dir">Direction: -1 (left from cursor), 1 (right from cursor), 0 (entire line).</param>
    /// <param name="callback">Optional callback invoked once the operation completes.</param>
    /// <returns>True if stream is a TTY and function succeeded.</returns>
    public static bool clearLine(Writable stream, int dir, Action? callback = null)
    {
        if (stream == null)
            throw new ArgumentNullException(nameof(stream));

        try
        {
            // ANSI escape codes for clearing lines
            // ESC[1K - clear from cursor to beginning
            // ESC[2K - clear entire line
            // ESC[0K - clear from cursor to end
            string escapeCode = dir switch
            {
                -1 => "\x1B[1K", // Clear left
                0 => "\x1B[2K",  // Clear entire line
                1 => "\x1B[0K",  // Clear right
                _ => throw new ArgumentException("dir must be -1, 0, or 1", nameof(dir))
            };

            stream.write(escapeCode);
            callback?.Invoke();
            return true;
        }
        catch
        {
            callback?.Invoke();
            return false;
        }
    }

    /// <summary>
    /// Clears the screen from the current cursor position downward.
    /// </summary>
    /// <param name="stream">The TTY stream.</param>
    /// <param name="callback">Optional callback invoked once the operation completes.</param>
    /// <returns>True if stream is a TTY and function succeeded.</returns>
    public static bool clearScreenDown(Writable stream, Action? callback = null)
    {
        if (stream == null)
            throw new ArgumentNullException(nameof(stream));

        try
        {
            // ANSI escape code ESC[0J - clear from cursor down
            stream.write("\x1B[0J");
            callback?.Invoke();
            return true;
        }
        catch
        {
            callback?.Invoke();
            return false;
        }
    }

    /// <summary>
    /// Moves the cursor to the specified position in the given TTY stream.
    /// </summary>
    /// <param name="stream">The TTY stream.</param>
    /// <param name="x">The column position (0-based).</param>
    /// <param name="y">Optional row position (0-based).</param>
    /// <param name="callback">Optional callback invoked once the operation completes.</param>
    /// <returns>True if stream is a TTY and function succeeded.</returns>
    public static bool cursorTo(Writable stream, int x, int? y = null, Action? callback = null)
    {
        if (stream == null)
            throw new ArgumentNullException(nameof(stream));

        try
        {
            // ANSI escape codes for cursor positioning
            // ESC[{row};{col}H - move to absolute position (1-based)
            // ESC[{col}G - move to column (1-based)
            string escapeCode = y.HasValue
                ? $"\x1B[{y.Value + 1};{x + 1}H"  // Both row and column (convert to 1-based)
                : $"\x1B[{x + 1}G";                 // Column only (convert to 1-based)

            stream.write(escapeCode);
            callback?.Invoke();
            return true;
        }
        catch
        {
            callback?.Invoke();
            return false;
        }
    }

    /// <summary>
    /// Moves the cursor relative to its current position in the given TTY stream.
    /// </summary>
    /// <param name="stream">The TTY stream.</param>
    /// <param name="dx">Horizontal movement (positive = right, negative = left).</param>
    /// <param name="dy">Vertical movement (positive = down, negative = up).</param>
    /// <param name="callback">Optional callback invoked once the operation completes.</param>
    /// <returns>True if stream is a TTY and function succeeded.</returns>
    public static bool moveCursor(Writable stream, int dx, int dy, Action? callback = null)
    {
        if (stream == null)
            throw new ArgumentNullException(nameof(stream));

        try
        {
            var result = new StringBuilder();

            // Horizontal movement
            if (dx < 0)
            {
                // Move left: ESC[{n}D
                result.Append($"\x1B[{-dx}D");
            }
            else if (dx > 0)
            {
                // Move right: ESC[{n}C
                result.Append($"\x1B[{dx}C");
            }

            // Vertical movement
            if (dy < 0)
            {
                // Move up: ESC[{n}A
                result.Append($"\x1B[{-dy}A");
            }
            else if (dy > 0)
            {
                // Move down: ESC[{n}B
                result.Append($"\x1B[{dy}B");
            }

            if (result.Length > 0)
            {
                stream.write(result.ToString());
            }

            callback?.Invoke();
            return true;
        }
        catch
        {
            callback?.Invoke();
            return false;
        }
    }

    /// <summary>
    /// Configures a readable stream to emit keypress events on the given interface.
    /// </summary>
    public static void emitKeypressEvents(Readable stream, Interface? rl = null)
    {
        if (stream == null)
            throw new ArgumentNullException(nameof(stream));

        stream.on("data", (Action<object?>)(chunk =>
        {
            if (rl == null) return;
            var key = chunk?.ToString() ?? string.Empty;
            rl.emit("keypress", key, key);
        }));
    }

    /// <summary>
    /// Returns a new Async Iterator object that iterates through all lines in the input stream as strings.
    /// This method allows readline.Interface to be consumed using async iteration.
    /// </summary>
    /// <param name="input">A Readable stream.</param>
    /// <param name="options">Optional interface options.</param>
    /// <returns>An async enumerator for lines.</returns>
    public static async System.Collections.Generic.IAsyncEnumerable<string> createAsyncIterator(
        Readable input,
        InterfaceOptions? options = null)
    {
        if (input == null)
            throw new ArgumentNullException(nameof(input));

        var opts = options ?? new InterfaceOptions();
        opts.input = input;

        var rl = createInterface(opts);
        var lines = new System.Collections.Generic.Queue<string>();
        var tcs = new System.Threading.Tasks.TaskCompletionSource<bool>();
        var closed = false;

        rl.on("line", (Action<object?[]>)(args =>
        {
            if (args.Length > 0 && args[0] is string line)
            {
                lines.Enqueue(line);
                tcs.TrySetResult(true);
            }
        }));

        rl.on("close", (Action)(() =>
        {
            closed = true;
            tcs.TrySetResult(false);
        }));

        while (!closed || lines.Count > 0)
        {
            if (lines.Count > 0)
            {
                yield return lines.Dequeue();
            }
            else if (!closed)
            {
                await tcs.Task;
                tcs = new System.Threading.Tasks.TaskCompletionSource<bool>();
            }
        }

        rl.close();
    }
}
