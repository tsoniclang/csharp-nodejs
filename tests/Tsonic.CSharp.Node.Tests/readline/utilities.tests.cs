using Xunit;
using System;
using System.Collections.Generic;

namespace Tsonic.CSharp.Node.Tests;

public class Readline_utilitiesTests
{
    [Fact]
    public void clearLine_WithValidStream_ShouldReturnTrue()
    {
        var output = new Writable();
        var result = readline.clearLine(output, 0);

        Assert.True(result);
    }

    [Fact]
    public void clearLine_Direction_Left_ShouldSucceed()
    {
        var output = new Writable();

        var result = readline.clearLine(output, -1);

        Assert.True(result);
    }

    [Fact]
    public void clearLine_Direction_EntireLine_ShouldSucceed()
    {
        var output = new Writable();

        var result = readline.clearLine(output, 0);

        Assert.True(result);
    }

    [Fact]
    public void clearLine_Direction_Right_ShouldSucceed()
    {
        var output = new Writable();

        var result = readline.clearLine(output, 1);

        Assert.True(result);
    }

    [Fact]
    public void clearLine_WithInvalidDirection_ShouldReturnFalse()
    {
        var output = new Writable();

        // Invalid direction throws ArgumentException which is caught by try-catch
        // The method returns false when exception occurs
        var result = readline.clearLine(output, 5);

        Assert.False(result);
    }

    [Fact]
    public void clearLine_WithNullStream_ShouldThrow()
    {
        Assert.Throws<ArgumentNullException>(() =>
            readline.clearLine(null!, 0));
    }

    [Fact]
    public void clearLine_WithCallback_ShouldInvokeCallback()
    {
        var output = new Writable();
        var callbackInvoked = false;

        readline.clearLine(output, 0, () => callbackInvoked = true);

        Assert.True(callbackInvoked);
    }

    [Fact]
    public void clearScreenDown_WithValidStream_ShouldReturnTrue()
    {
        var output = new Writable();
        var result = readline.clearScreenDown(output);

        Assert.True(result);
    }

    [Fact]
    public void clearScreenDown_ShouldSucceed()
    {
        var output = new Writable();

        var result = readline.clearScreenDown(output);

        Assert.True(result);
    }

    [Fact]
    public void clearScreenDown_WithNullStream_ShouldThrow()
    {
        Assert.Throws<ArgumentNullException>(() =>
            readline.clearScreenDown(null!));
    }

    [Fact]
    public void clearScreenDown_WithCallback_ShouldInvokeCallback()
    {
        var output = new Writable();
        var callbackInvoked = false;

        readline.clearScreenDown(output, () => callbackInvoked = true);

        Assert.True(callbackInvoked);
    }

    [Fact]
    public void cursorTo_WithColumnOnly_ShouldReturnTrue()
    {
        var output = new Writable();
        var result = readline.cursorTo(output, 10);

        Assert.True(result);
    }

    [Fact]
    public void cursorTo_WithColumnOnly_ShouldSucceed()
    {
        var output = new Writable();

        var result = readline.cursorTo(output, 5);

        Assert.True(result);
    }

    [Fact]
    public void cursorTo_WithRowAndColumn_ShouldSucceed()
    {
        var output = new Writable();

        var result = readline.cursorTo(output, 10, 5);

        Assert.True(result);
    }

    [Fact]
    public void cursorTo_WithNullStream_ShouldThrow()
    {
        Assert.Throws<ArgumentNullException>(() =>
            readline.cursorTo(null!, 0));
    }

    [Fact]
    public void cursorTo_WithCallback_ShouldInvokeCallback()
    {
        var output = new Writable();
        var callbackInvoked = false;

        readline.cursorTo(output, 0, null, () => callbackInvoked = true);

        Assert.True(callbackInvoked);
    }

    [Fact]
    public void moveCursor_WithPositiveDx_ShouldMoveRight()
    {
        var output = new Writable();

        var result = readline.moveCursor(output, 5, 0);

        Assert.True(result);
    }

    [Fact]
    public void moveCursor_WithNegativeDx_ShouldMoveLeft()
    {
        var output = new Writable();

        var result = readline.moveCursor(output, -3, 0);

        Assert.True(result);
    }

    [Fact]
    public void moveCursor_WithPositiveDy_ShouldMoveDown()
    {
        var output = new Writable();

        var result = readline.moveCursor(output, 0, 2);

        Assert.True(result);
    }

    [Fact]
    public void moveCursor_WithNegativeDy_ShouldMoveUp()
    {
        var output = new Writable();

        var result = readline.moveCursor(output, 0, -4);

        Assert.True(result);
    }

    [Fact]
    public void moveCursor_WithBothDirections_ShouldSucceed()
    {
        var output = new Writable();

        var result = readline.moveCursor(output, 2, -1);

        Assert.True(result);
    }

    [Fact]
    public void moveCursor_WithZeroDelta_ShouldWork()
    {
        var output = new Writable();
        var result = readline.moveCursor(output, 0, 0);

        Assert.True(result);
    }

    [Fact]
    public void moveCursor_WithNullStream_ShouldThrow()
    {
        Assert.Throws<ArgumentNullException>(() =>
            readline.moveCursor(null!, 0, 0));
    }

    [Fact]
    public void moveCursor_WithCallback_ShouldInvokeCallback()
    {
        var output = new Writable();
        var callbackInvoked = false;

        readline.moveCursor(output, 1, 1, () => callbackInvoked = true);

        Assert.True(callbackInvoked);
    }
}
