using Xunit;
using System;

namespace Tsonic.CSharp.Node.Tests;

public class Readline_createInterfaceTests
{
    [Fact]
    public void createInterface_WithValidOptions_ShouldCreateInterface()
    {
        var input = new Readable();
        var output = new Writable();

        var rl = readline.createInterface(new InterfaceOptions
        {
            input = input,
            output = output
        });

        Assert.NotNull(rl);
        Assert.IsType<Interface>(rl);
    }

    [Fact]
    public void createInterface_WithInputOnly_ShouldWork()
    {
        var input = new Readable();

        var rl = readline.createInterface(new InterfaceOptions
        {
            input = input
        });

        Assert.NotNull(rl);
    }

    [Fact]
    public void createInterface_WithNullOptions_ShouldThrow()
    {
        Assert.Throws<ArgumentNullException>(() =>
            readline.createInterface((InterfaceOptions)null!));
    }

    [Fact]
    public void createInterface_WithNullInput_ShouldThrow()
    {
        Assert.Throws<ArgumentException>(() =>
            readline.createInterface(new InterfaceOptions()));
    }

    [Fact]
    public void createInterface_WithStreams_ShouldWork()
    {
        var input = new Readable();
        var output = new Writable();

        var rl = readline.createInterface(input, output);

        Assert.NotNull(rl);
    }

    [Fact]
    public void createInterface_WithCustomPrompt_ShouldUsePrompt()
    {
        var input = new Readable();
        var customPrompt = "custom> ";

        var rl = readline.createInterface(new InterfaceOptions
        {
            input = input,
            prompt = customPrompt
        });

        Assert.Equal(customPrompt, rl.getPrompt());
    }

    [Fact]
    public void createInterface_WithHistory_ShouldInitializeHistory()
    {
        var input = new Readable();
        var history = new[] { "line1", "line2", "line3" };

        var rl = readline.createInterface(new InterfaceOptions
        {
            input = input,
            history = history
        });

        Assert.NotNull(rl);
        // History is initialized (we can't directly access it, but no exception)
    }

    [Fact]
    public void createInterface_WithTerminalOption_ShouldWork()
    {
        var input = new Readable();

        var rl = readline.createInterface(new InterfaceOptions
        {
            input = input,
            terminal = true
        });

        Assert.NotNull(rl);
    }

    [Fact]
    public void createInterface_WithHistorySize_ShouldWork()
    {
        var input = new Readable();

        var rl = readline.createInterface(new InterfaceOptions
        {
            input = input,
            historySize = 100
        });

        Assert.NotNull(rl);
    }
}
