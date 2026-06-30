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
    private void processInput(string input)
    {
        for (int i = 0; i < input.Length; i++)
        {
            var ch = input[i];

            // Handle ANSI escape sequences (arrow keys, delete, home, end, etc.)
            if (ch == '\x1B' && i + 1 < input.Length && input[i + 1] == '[')
            {
                // ANSI escape sequence
                i += 2; // Skip ESC and [

                if (i < input.Length)
                {
                    var code = input[i];

                    switch (code)
                    {
                        case 'A': // Up arrow - previous history
                            navigateHistory(true);
                            break;
                        case 'B': // Down arrow - next history
                            navigateHistory(false);
                            break;
                        case 'C': // Right arrow - move cursor right
                            if (_cursor < _line.Length)
                                _cursor++;
                            break;
                        case 'D': // Left arrow - move cursor left
                            if (_cursor > 0)
                                _cursor--;
                            break;
                        case 'H': // Home - move to beginning
                            _cursor = 0;
                            break;
                        case 'F': // End - move to end
                            _cursor = _line.Length;
                            break;
                        case '3': // Delete key (ESC[3~)
                            if (i + 1 < input.Length && input[i + 1] == '~')
                            {
                                i++; // Skip ~
                                if (_cursor < _line.Length)
                                {
                                    _line = _line.Remove(_cursor, 1);
                                }
                            }
                            break;
                    }
                }
                continue;
            }

            // Handle control characters
            if (ch < ' ' && ch != '\n' && ch != '\r' && ch != '\t' && ch != '\b')
            {
                // Control+A - move to beginning
                if (ch == '\x01') // Ctrl+A
                {
                    _cursor = 0;
                    continue;
                }
                // Control+E - move to end
                else if (ch == '\x05') // Ctrl+E
                {
                    _cursor = _line.Length;
                    continue;
                }
                // Control+U - clear line before cursor
                else if (ch == '\x15') // Ctrl+U
                {
                    _line = _line.Substring(_cursor);
                    _cursor = 0;
                    continue;
                }
                // Control+K - clear line after cursor
                else if (ch == '\x0B') // Ctrl+K
                {
                    _line = _line.Substring(0, _cursor);
                    continue;
                }
                // Control+W - delete word before cursor
                else if (ch == '\x17') // Ctrl+W
                {
                    deleteWordBeforeCursor();
                    continue;
                }
            }

            // Handle regular characters
            if (ch == '\n' || ch == '\r')
            {
                // Line complete
                var completedLine = _line;

                // Add to history if not empty
                if (!string.IsNullOrWhiteSpace(completedLine))
                {
                    addToHistory(completedLine);
                }

                // Emit line event
                emit("line", completedLine);

                // Reset line buffer and history navigation
                _line = "";
                _cursor = 0;
                _historyIndex = -1;
                _savedLine = "";
            }
            else if (ch == '\b' || ch == '\x7f') // Backspace or DEL
            {
                if (_cursor > 0)
                {
                    _line = _line.Remove(_cursor - 1, 1);
                    _cursor--;
                }
            }
            else if (ch == '\t') // Tab
            {
                // Simple tab handling - insert spaces
                _line = _line.Insert(_cursor, "    ");
                _cursor += 4;
            }
            else if (ch >= ' ') // Printable characters
            {
                // Add character to line
                _line = _line.Insert(_cursor, ch.ToString());
                _cursor++;

                // Reset history navigation when typing
                _historyIndex = -1;
            }
        }
    }

    private void navigateHistory(bool previous)
    {
        if (_history.Count == 0)
            return;

        // Save current line if starting history navigation
        if (_historyIndex == -1)
        {
            _savedLine = _line;
        }

        if (previous)
        {
            // Navigate to previous (older) history entry
            if (_historyIndex < _history.Count - 1)
            {
                _historyIndex++;
                _line = _history[_history.Count - 1 - _historyIndex];
                _cursor = _line.Length;
            }
        }
        else
        {
            // Navigate to next (newer) history entry
            if (_historyIndex > 0)
            {
                _historyIndex--;
                _line = _history[_history.Count - 1 - _historyIndex];
                _cursor = _line.Length;
            }
            else if (_historyIndex == 0)
            {
                // Restore saved line
                _historyIndex = -1;
                _line = _savedLine;
                _cursor = _line.Length;
            }
        }
    }

    private void deleteWordBeforeCursor()
    {
        if (_cursor == 0)
            return;

        // Find the start of the word
        int wordStart = _cursor - 1;

        // Skip trailing whitespace
        while (wordStart >= 0 && char.IsWhiteSpace(_line[wordStart]))
        {
            wordStart--;
        }

        // Find the beginning of the word
        while (wordStart >= 0 && !char.IsWhiteSpace(_line[wordStart]))
        {
            wordStart--;
        }

        wordStart++; // Move to first character of word

        // Delete from wordStart to cursor
        int deleteCount = _cursor - wordStart;
        _line = _line.Remove(wordStart, deleteCount);
        _cursor = wordStart;
    }

    private void addToHistory(string line)
    {
        if (_removeHistoryDuplicates)
        {
            _history.RemoveAll(h => h == line);
        }

        _history.Add(line);

        // Trim history if it exceeds size limit
        while (_history.Count > _historySize)
        {
            _history.RemoveAt(0);
        }
    }
}
