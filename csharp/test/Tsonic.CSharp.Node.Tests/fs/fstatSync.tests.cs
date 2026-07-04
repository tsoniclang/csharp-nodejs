using System;
using System.IO;
using Xunit;

namespace Tsonic.CSharp.Node.Tests;

public class Fs_fstatSyncTests : IDisposable
{
    private readonly string _testDir;
    private readonly string _testFile;

    public Fs_fstatSyncTests()
    {
        _testDir = Path.Combine(Path.GetTempPath(), $"tsonic_test_{Guid.NewGuid():N}");
        Directory.CreateDirectory(_testDir);
        _testFile = Path.Combine(_testDir, "test.txt");
    }

    public void Dispose()
    {
        if (Directory.Exists(_testDir))
        {
            Directory.Delete(_testDir, true);
        }
    }

    [Fact]
    public void fstatSync_ShouldReturnStats()
    {
        File.WriteAllText(_testFile, "test content");

        var fd = fs.openSync(_testFile, "r");
        var stats = fs.fstatSync(fd);

        fs.closeSync(fd);

        Assert.NotNull(stats);
        Assert.True(stats.size > 0);
        Assert.True(stats.isFile);
        Assert.False(stats.isDirectory);
    }

    [Fact]
    public void fstatSync_ShouldReturnCorrectSize()
    {
        var content = "Hello, World!";
        File.WriteAllText(_testFile, content);

        var fd = fs.openSync(_testFile, "r");
        var stats = fs.fstatSync(fd);

        fs.closeSync(fd);

        Assert.Equal(content.Length, stats.size);
    }

    [Fact]
    public void fstatSync_ShouldHaveValidTimestamps()
    {
        File.WriteAllText(_testFile, "test");

        var fd = fs.openSync(_testFile, "r");
        var stats = fs.fstatSync(fd);

        fs.closeSync(fd);

        Assert.True(stats.atime.getTime() > 0);
        Assert.True(stats.mtime.getTime() > 0);
        Assert.True(stats.ctime.getTime() > 0);
        Assert.True(stats.birthtime.getTime() > 0);
        Assert.True(stats.atimeMs > 0);
        Assert.True(stats.mtimeMs > 0);
        Assert.True(stats.ctimeMs > 0);
        Assert.True(stats.birthtimeMs > 0);
        Assert.Equal((long)stats.atimeMs, stats.atime.getTime());
        Assert.Equal((long)stats.mtimeMs, stats.mtime.getTime());
    }

    [Fact]
    public void fstatSync_WithInvalidDescriptor_ShouldThrow()
    {
        Assert.Throws<ArgumentException>(() => fs.fstatSync(999));
    }

    [Fact]
    public void fstatSync_AfterWrite_ShouldReflectNewSize()
    {
        var fd = fs.openSync(_testFile, "w");
        fs.writeSync(fd, "Hello", null, null);
        fs.closeSync(fd);

        // Reopen to get accurate stats
        fd = fs.openSync(_testFile, "r");
        var stats = fs.fstatSync(fd);
        fs.closeSync(fd);

        Assert.Equal(5, stats.size);
    }

    [Fact]
    public void fstatSync_EmptyFile_ShouldHaveZeroSize()
    {
        File.WriteAllText(_testFile, "");

        var fd = fs.openSync(_testFile, "r");
        var stats = fs.fstatSync(fd);

        fs.closeSync(fd);

        Assert.Equal(0, stats.size);
    }
}
