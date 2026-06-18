using Xunit;
using System;
using System.Collections.Generic;

namespace Tsonic.CSharp.Node.Tests;

public class Readline_InterfaceTests
{
    [Fact]
    public void Interface_line_ShouldReturnCurrentLine()
    {
        var input = new Readable();
        var rl = readline.createInterface(input);

        Assert.Equal("", rl.line);
    }

    [Fact]
    public void Interface_cursor_ShouldReturnCursorPosition()
    {
        var input = new Readable();
        var rl = readline.createInterface(input);

        Assert.Equal(0, rl.cursor);
    }

    [Fact]
    public void Interface_setPrompt_ShouldChangePrompt()
    {
        var input = new Readable();
        var rl = readline.createInterface(input);

        rl.setPrompt("new> ");

        Assert.Equal("new> ", rl.getPrompt());
    }

    [Fact]
    public void Interface_getPrompt_ShouldReturnCurrentPrompt()
    {
        var input = new Readable();
        var rl = readline.createInterface(new InterfaceOptions
        {
            input = input,
            prompt = "test> "
        });

        Assert.Equal("test> ", rl.getPrompt());
    }

    [Fact]
    public void Interface_prompt_ShouldNotThrow()
    {
        var input = new Readable();
        var output = new Writable();
        var rl = readline.createInterface(new InterfaceOptions
        {
            input = input,
            output = output,
            prompt = "$ "
        });

        // Should not throw
        rl.prompt();

        // If we got here without exception, test passes
        Assert.True(true);
    }

    [Fact]
    public void Interface_pause_ShouldPauseInput()
    {
        var input = new Readable();
        var rl = readline.createInterface(input);

        var pauseEmitted = false;
        rl.on("pause", (Action)(() => pauseEmitted = true));

        var result = rl.pause();

        Assert.Same(rl, result); // Should return this
        Assert.True(pauseEmitted);
    }

    [Fact]
    public void Interface_resume_ShouldResumeInput()
    {
        var input = new Readable();
        var rl = readline.createInterface(input);

        rl.pause();

        var resumeEmitted = false;
        rl.on("resume", (Action)(() => resumeEmitted = true));

        var result = rl.resume();

        Assert.Same(rl, result); // Should return this
        Assert.True(resumeEmitted);
    }

    [Fact]
    public void Interface_close_ShouldEmitCloseEvent()
    {
        var input = new Readable();
        var rl = readline.createInterface(input);

        var closeEmitted = false;
        rl.on("close", (Action)(() => closeEmitted = true));

        rl.close();

        Assert.True(closeEmitted);
    }

    [Fact]
    public void Interface_close_WhenAlreadyClosed_ShouldNotEmitAgain()
    {
        var input = new Readable();
        var rl = readline.createInterface(input);

        var closeCount = 0;
        rl.on("close", (Action)(() => closeCount++));

        rl.close();
        rl.close(); // Second close

        Assert.Equal(1, closeCount);
    }

    [Fact]
    public void Interface_question_ShouldInvokeCallback()
    {
        var input = new Readable();
        var output = new Writable();
        var rl = readline.createInterface(new InterfaceOptions
        {
            input = input,
            output = output
        });

        string? answer = null;
        rl.question("What is your name? ", (a) => answer = a);

        // Simulate user input
        rl.emit("line", "Alice");

        Assert.Equal("Alice", answer);
    }

    [Fact]
    public void Interface_question_WithNullCallback_ShouldThrow()
    {
        var input = new Readable();
        var rl = readline.createInterface(input);

        Assert.Throws<ArgumentNullException>(() =>
            rl.question("Test? ", null!));
    }

    [Fact]
    public void Interface_write_ShouldNotThrow()
    {
        var input = new Readable();
        var output = new Writable();
        var rl = readline.createInterface(new InterfaceOptions
        {
            input = input,
            output = output
        });

        rl.write("test data");

        // If we got here without exception, test passes
        Assert.True(true);
    }

    [Fact]
    public void Interface_getCursorPos_ShouldReturnPosition()
    {
        var input = new Readable();
        var rl = readline.createInterface(input);

        var pos = rl.getCursorPos();

        Assert.NotNull(pos);
        Assert.True(pos.rows >= 0);
        Assert.True(pos.cols >= 0);
    }

    [Fact]
    public void Interface_prompt_WhenClosed_ShouldThrow()
    {
        var input = new Readable();
        var rl = readline.createInterface(input);

        rl.close();

        Assert.Throws<InvalidOperationException>(() => rl.prompt());
    }

    [Fact]
    public void Interface_question_WhenClosed_ShouldThrow()
    {
        var input = new Readable();
        var rl = readline.createInterface(input);

        rl.close();

        Assert.Throws<InvalidOperationException>(() =>
            rl.question("Test?", (a) => { }));
    }

    [Fact]
    public void Interface_write_WhenClosed_ShouldThrow()
    {
        var input = new Readable();
        var rl = readline.createInterface(input);

        rl.close();

        Assert.Throws<InvalidOperationException>(() => rl.write("test"));
    }
}
