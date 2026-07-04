using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Tsonic.CSharp.Node;

/// <summary>
/// The Interface class represents a readline interface with an input and output stream.
/// Extends EventEmitter to emit events like 'line', 'close', 'pause', 'resume', etc.
/// </summary>
public partial class Interface : EventEmitter
{
    private readonly Readable? _input;
    private readonly Writable? _output;
    private readonly bool _terminal;
    private string _prompt = "> ";
    private readonly List<string> _history = new();
    private readonly int _historySize;
    private readonly bool _removeHistoryDuplicates;
    private string _line = "";
    private int _cursor = 0;
    private bool _closed = false;
    private bool _paused = false;
    private Action<object?>? _dataListener;
    private Action? _endListener;
    private int _historyIndex = -1; // Current position in history (-1 = not navigating)
    private string _savedLine = ""; // Saved current line when navigating history

    /// <summary>
    /// Current line being processed.
    /// </summary>
    public string line => _line;

    /// <summary>
    /// Cursor position in current line.
    /// </summary>
    public int cursor => _cursor;

    internal Interface(InterfaceOptions options)
    {
        if (options == null)
            throw new ArgumentNullException(nameof(options));

        _input = options.input;
        _output = options.output;
        _terminal = options.terminal ?? false;
        _prompt = options.prompt ?? "> ";
        _historySize = options.historySize ?? 30;
        _removeHistoryDuplicates = options.removeHistoryDuplicates ?? false;

        // Initialize history if provided
        if (options.history != null)
        {
            _history.AddRange(options.history);
        }

        // Set up input stream listeners
        if (_input != null)
        {
            _dataListener = (data) =>
            {
                if (!_paused && data != null)
                {
                    processInput(data.ToString() ?? "");
                }
            };

            _endListener = () =>
            {
                if (!_closed)
                {
                    close();
                }
            };

            _input.on("data", _dataListener);
            _input.on("end", _endListener);
        }
    }

    /// <summary>
    /// Prompts the user for input by writing the configured prompt string to output.
    /// </summary>
    /// <param name="preserveCursor">If true, prevents the cursor from being reset to 0.</param>
    public void prompt(bool preserveCursor = false)
    {
        if (_closed)
            throw new InvalidOperationException("Cannot prompt on closed interface");

        if (_output != null && !string.IsNullOrEmpty(_prompt))
        {
            _output.write(_prompt);
        }

        if (!preserveCursor)
        {
            _cursor = 0;
        }
    }

    /// <summary>
    /// Prompts the user with a query string and calls the callback with the user's response.
    /// </summary>
    /// <param name="query">The question to ask the user.</param>
    /// <param name="callback">Callback invoked with the user's answer.</param>
    public void question(string query, Action<string> callback)
    {
        if (_closed)
            throw new InvalidOperationException("Cannot question on closed interface");

        if (callback == null)
            throw new ArgumentNullException(nameof(callback));

        // Write the question
        if (_output != null)
        {
            _output.write(query);
        }

        // Set up a one-time listener for the next line
        Action<object?[]> lineListener = null!;
        lineListener = (args) =>
        {
            if (args.Length > 0 && args[0] is string answer)
            {
                callback(answer);
            }
        };

        once("line", lineListener);
    }

    /// <summary>
    /// Prompts the user with a query string and returns a Task that resolves with the user's response.
    /// Promise-based version of question() for use with async/await.
    /// </summary>
    /// <param name="query">The question to ask the user.</param>
    /// <returns>A Task that resolves with the user's answer.</returns>
    public Task<string> questionAsync(string query)
    {
        if (_closed)
            throw new InvalidOperationException("Cannot question on closed interface");

        var tcs = new TaskCompletionSource<string>();

        // Write the question
        if (_output != null)
        {
            _output.write(query);
        }

        // Set up a one-time listener for the next line
        Action<object?[]> lineListener = null!;
        lineListener = (args) =>
        {
            if (args.Length > 0 && args[0] is string answer)
            {
                tcs.TrySetResult(answer);
            }
        };

        once("line", lineListener);

        return tcs.Task;
    }

    /// <summary>
    /// Writes data to output stream or simulates keypresses.
    /// </summary>
    /// <param name="data">The data to write or null.</param>
    /// <param name="key">Optional key object to simulate keypress.</param>
    public void write(object? data, object? key = null)
    {
        if (_closed)
            throw new InvalidOperationException("Cannot write on closed interface");

        if (data != null)
        {
            var text = data.ToString() ?? "";
            if (_output != null)
            {
                _output.write(text);
            }
        }

        if (key != null)
        {
            // Simulate keypress (simplified)
            processInput(key.ToString() ?? "");
        }
    }

    /// <summary>
    /// Pauses the input stream, allowing it to be resumed later.
    /// </summary>
    /// <returns>This Interface instance.</returns>
    public Interface pause()
    {
        if (_closed || _paused)
            return this;

        _paused = true;
        if (_input != null)
        {
            _input.pause();
        }
        emit("pause");
        return this;
    }

    /// <summary>
    /// Resumes the input stream if it has been paused.
    /// </summary>
    /// <returns>This Interface instance.</returns>
    public Interface resume()
    {
        if (_closed || !_paused)
            return this;

        _paused = false;
        if (_input != null)
        {
            _input.resume();
        }
        emit("resume");
        return this;
    }

    /// <summary>
    /// Closes the Interface instance and relinquishes control over input/output streams.
    /// </summary>
    public void close()
    {
        if (_closed)
            return;

        _closed = true;

        // Close input stream if we own it
        if (_input != null)
        {
            if (_dataListener != null)
                _input.removeListener("data", _dataListener);
            if (_endListener != null)
                _input.removeListener("end", _endListener);
        }

        emit("close");
    }

    /// <summary>
    /// Sets the prompt string that will be written to output when prompt() is called.
    /// </summary>
    /// <param name="prompt">The new prompt string.</param>
    public void setPrompt(string prompt)
    {
        _prompt = prompt ?? "";
    }

    /// <summary>
    /// Returns the current prompt used by the interface.
    /// </summary>
    /// <returns>The current prompt string.</returns>
    public string getPrompt()
    {
        return _prompt;
    }

    /// <summary>
    /// Returns the real position of the cursor in relation to the input prompt + string.
    /// </summary>
    /// <returns>Object with rows and cols properties.</returns>
    public CursorPosition getCursorPos()
    {
        // Simplified: just return cursor position relative to current line
        var promptLength = _prompt?.Length ?? 0;
        var totalLength = promptLength + _cursor;

        // Assume 80 column terminal
        var cols = totalLength % 80;
        var rows = totalLength / 80;

        return new CursorPosition { rows = rows, cols = cols };
    }

}

/// <summary>
/// Options for creating a readline Interface.
/// </summary>
public class InterfaceOptions
{
    /// <summary>
    /// The Readable stream to listen to. Required.
    /// </summary>
    public Readable? input { get; set; }

    /// <summary>
    /// The Writable stream to write readline data to.
    /// </summary>
    public Writable? output { get; set; }

    /// <summary>
    /// true if input and output streams should be treated as TTY and have ANSI/VT100 escape codes written.
    /// </summary>
    public bool? terminal { get; set; }

    /// <summary>
    /// The prompt string to use.
    /// </summary>
    public string? prompt { get; set; }

    /// <summary>
    /// Initial list of history lines.
    /// </summary>
    public string[]? history { get; set; }

    /// <summary>
    /// Maximum number of history lines retained. Default is 30.
    /// </summary>
    public int? historySize { get; set; }

    /// <summary>
    /// If true, when a new input line equals an old one in history, removes the old line. Default is false.
    /// </summary>
    public bool? removeHistoryDuplicates { get; set; }

    /// <summary>
    /// The duration readline will wait for a character (in ms).
    /// </summary>
    public int? escapeCodeTimeout { get; set; }

    /// <summary>
    /// The number of spaces a tab is equal to. Default is 8.
    /// </summary>
    public int? tabSize { get; set; }
}

/// <summary>
/// Represents the cursor position with row and column.
/// </summary>
public class CursorPosition
{
    /// <summary>
    /// Row position (0-based).
    /// </summary>
    public int rows { get; set; }

    /// <summary>
    /// Column position (0-based).
    /// </summary>
    public int cols { get; set; }
}
