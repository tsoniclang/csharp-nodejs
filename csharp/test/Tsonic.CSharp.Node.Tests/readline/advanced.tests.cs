using Xunit;
using System;
using System.Threading.Tasks;

namespace Tsonic.CSharp.Node.Tests;

public class Readline_advancedTests
{
    [Fact]
    public async Task Interface_questionAsync_ShouldReturnAnswer()
    {
        var input = new Readable();
        var output = new Writable();
        var rl = readline.createInterface(new InterfaceOptions
        {
            input = input,
            output = output
        });

        // Start the async question
        var questionTask = rl.questionAsync("What is your name? ");

        // Simulate user input
        rl.emit("line", "Alice");

        var answer = await questionTask;

        Assert.Equal("Alice", answer);
    }

    [Fact]
    public async Task Interface_questionAsync_WhenClosed_ShouldThrow()
    {
        var input = new Readable();
        var rl = readline.createInterface(input);

        rl.close();

        await Assert.ThrowsAsync<InvalidOperationException>(async () =>
            await rl.questionAsync("Test?"));
    }

    [Fact]
    public async Task Interface_questionAsync_MultipleQuestions_ShouldWork()
    {
        var input = new Readable();
        var output = new Writable();
        var rl = readline.createInterface(new InterfaceOptions
        {
            input = input,
            output = output
        });

        // First question
        var q1Task = rl.questionAsync("Question 1? ");
        rl.emit("line", "Answer 1");
        var answer1 = await q1Task;

        // Second question
        var q2Task = rl.questionAsync("Question 2? ");
        rl.emit("line", "Answer 2");
        var answer2 = await q2Task;

        Assert.Equal("Answer 1", answer1);
        Assert.Equal("Answer 2", answer2);
    }

    [Fact]
    public void Interface_ArrowKeys_ShouldMoveCursor()
    {
        var input = new Readable();
        var rl = readline.createInterface(input);

        // Simulate typing "hello"
        input.emit("data", "hello");
        Assert.Equal("hello", rl.line);
        Assert.Equal(5, rl.cursor);

        // Simulate left arrow (ESC[D)
        input.emit("data", "\x1B[D");
        Assert.Equal(4, rl.cursor);

        // Simulate another left arrow
        input.emit("data", "\x1B[D");
        Assert.Equal(3, rl.cursor);

        // Simulate right arrow (ESC[C)
        input.emit("data", "\x1B[C");
        Assert.Equal(4, rl.cursor);
    }

    [Fact]
    public void Interface_HomeEndKeys_ShouldMoveCursorToEdges()
    {
        var input = new Readable();
        var rl = readline.createInterface(input);

        // Type some text
        input.emit("data", "hello world");
        Assert.Equal(11, rl.cursor);

        // Home key (ESC[H)
        input.emit("data", "\x1B[H");
        Assert.Equal(0, rl.cursor);

        // End key (ESC[F)
        input.emit("data", "\x1B[F");
        Assert.Equal(11, rl.cursor);
    }

    [Fact]
    public void Interface_DeleteKey_ShouldDeleteCharacterAtCursor()
    {
        var input = new Readable();
        var rl = readline.createInterface(input);

        // Type "hello"
        input.emit("data", "hello");

        // Move cursor to position 2 (between 'e' and 'l')
        input.emit("data", "\x1B[D\x1B[D\x1B[D");
        Assert.Equal(2, rl.cursor);

        // Delete key (ESC[3~)
        input.emit("data", "\x1B[3~");

        // Should have deleted the 'l', leaving "helo"
        Assert.Equal("helo", rl.line);
        Assert.Equal(2, rl.cursor);
    }

    [Fact]
    public void Interface_CtrlA_ShouldMoveToBeginning()
    {
        var input = new Readable();
        var rl = readline.createInterface(input);

        input.emit("data", "hello");
        Assert.Equal(5, rl.cursor);

        // Ctrl+A
        input.emit("data", "\x01");
        Assert.Equal(0, rl.cursor);
    }

    [Fact]
    public void Interface_CtrlE_ShouldMoveToEnd()
    {
        var input = new Readable();
        var rl = readline.createInterface(input);

        input.emit("data", "hello");

        // Move cursor to beginning
        input.emit("data", "\x1B[H");
        Assert.Equal(0, rl.cursor);

        // Ctrl+E
        input.emit("data", "\x05");
        Assert.Equal(5, rl.cursor);
    }

    [Fact]
    public void Interface_CtrlU_ShouldClearLineBeforeCursor()
    {
        var input = new Readable();
        var rl = readline.createInterface(input);

        input.emit("data", "hello world");

        // Move cursor to position 6 (after "hello ")
        input.emit("data", "\x1B[D\x1B[D\x1B[D\x1B[D\x1B[D");
        Assert.Equal(6, rl.cursor);

        // Ctrl+U
        input.emit("data", "\x15");

        // Should leave "world" and cursor at 0
        Assert.Equal("world", rl.line);
        Assert.Equal(0, rl.cursor);
    }

    [Fact]
    public void Interface_CtrlK_ShouldClearLineAfterCursor()
    {
        var input = new Readable();
        var rl = readline.createInterface(input);

        input.emit("data", "hello world");

        // Move cursor to position 5 (after "hello")
        for (int i = 0; i < 6; i++)
        {
            input.emit("data", "\x1B[D");
        }
        Assert.Equal(5, rl.cursor);

        // Ctrl+K
        input.emit("data", "\x0B");

        // Should leave "hello"
        Assert.Equal("hello", rl.line);
        Assert.Equal(5, rl.cursor);
    }

    [Fact]
    public void Interface_CtrlW_ShouldDeleteWordBeforeCursor()
    {
        var input = new Readable();
        var rl = readline.createInterface(input);

        input.emit("data", "hello world test");
        Assert.Equal("hello world test", rl.line);
        Assert.Equal(16, rl.cursor);

        // Ctrl+W should delete "test"
        input.emit("data", "\x17");
        Assert.Equal("hello world ", rl.line);

        // Ctrl+W again should delete "world "
        input.emit("data", "\x17");
        Assert.Equal("hello ", rl.line);

        // Ctrl+W again should delete "hello "
        input.emit("data", "\x17");
        Assert.Equal("", rl.line);
    }

    [Fact]
    public void Interface_TabKey_ShouldInsertSpaces()
    {
        var input = new Readable();
        var rl = readline.createInterface(input);

        input.emit("data", "hello\tworld");

        // Tab should be converted to 4 spaces
        Assert.Equal("hello    world", rl.line);
    }
}
