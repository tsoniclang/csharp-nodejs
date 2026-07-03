using Xunit;

namespace Tsonic.CSharp.Node.Tests;

public class execPathTests
{
    [Fact]
    public void execPath_ShouldReturnNonEmptyString()
    {
        var execPath = process.execPath;

        Assert.NotNull(execPath);
        Assert.NotEmpty(execPath);
    }

    [Fact]
    public void execPath_ShouldBeAbsolutePath()
    {
        var execPath = process.execPath;

        Assert.True(Path.IsPathRooted(execPath));
    }

    [Fact]
    public void execPath_ShouldPointToExecutable()
    {
        var execPath = process.execPath;

        // The file should exist (or be the dotnet executable)
        var exists = File.Exists(execPath);
        Assert.True(exists, $"Executable path does not exist: {execPath}");
    }
}
