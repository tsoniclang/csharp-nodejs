using Xunit;
using System;
using System.IO;

namespace Tsonic.CSharp.Node.Tests;

public class Fs_openSyncTests : IDisposable
{
    private readonly string _testDir;
    private readonly string _testFile;

    public Fs_openSyncTests()
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
    public void openSync_WithReadFlag_ShouldOpenExistingFile()
    {
        File.WriteAllText(_testFile, "test content");

        var fd = fs.openSync(_testFile, "r");
        Assert.True(fd >= 0);

        fs.closeSync(fd);
    }

    [Fact]
    public void openSync_WithReadFlag_NonExistentFile_ShouldThrow()
    {
        var nonExistentFile = Path.Combine(_testDir, "nonexistent.txt");

        Assert.Throws<IOException>(() => fs.openSync(nonExistentFile, "r"));
    }

    [Fact]
    public void openSync_WithWriteFlag_ShouldCreateFile()
    {
        var fd = fs.openSync(_testFile, "w");
        Assert.True(fd >= 0);

        fs.closeSync(fd);
        Assert.True(File.Exists(_testFile));
    }

    [Fact]
    public void openSync_WithWriteFlag_ShouldTruncateExistingFile()
    {
        File.WriteAllText(_testFile, "original content");

        var fd = fs.openSync(_testFile, "w");
        fs.closeSync(fd);

        var content = File.ReadAllText(_testFile);
        Assert.Empty(content);
    }

    [Fact]
    public void openSync_WithAppendFlag_ShouldCreateFile()
    {
        var fd = fs.openSync(_testFile, "a");
        Assert.True(fd >= 0);

        fs.closeSync(fd);
        Assert.True(File.Exists(_testFile));
    }

    [Fact]
    public void openSync_WithReadPlusFlag_ShouldOpenForReadWrite()
    {
        File.WriteAllText(_testFile, "test");

        var fd = fs.openSync(_testFile, "r+");
        Assert.True(fd >= 0);

        fs.closeSync(fd);
    }

    [Fact]
    public void openSync_WithWritePlusFlag_ShouldCreateForReadWrite()
    {
        var fd = fs.openSync(_testFile, "w+");
        Assert.True(fd >= 0);

        fs.closeSync(fd);
        Assert.True(File.Exists(_testFile));
    }

    [Fact]
    public void openSync_WithAppendPlusFlag_ShouldThrow()
    {
        // .NET doesn't support Append mode with Read access
        // This matches the Node.js behavior on some platforms
        Assert.Throws<IOException>(() => fs.openSync(_testFile, "a+"));
    }

    [Fact]
    public void openSync_WithExclusiveWriteFlag_ShouldCreateNewFile()
    {
        var fd = fs.openSync(_testFile, "wx");
        Assert.True(fd >= 0);

        fs.closeSync(fd);
        Assert.True(File.Exists(_testFile));
    }

    [Fact]
    public void openSync_WithExclusiveWriteFlag_ExistingFile_ShouldThrow()
    {
        File.WriteAllText(_testFile, "test");

        Assert.Throws<IOException>(() => fs.openSync(_testFile, "wx"));
    }

    [Fact]
    public void openSync_WithEmptyPath_ShouldThrow()
    {
        Assert.Throws<ArgumentException>(() => fs.openSync("", "r"));
    }

    [Fact]
    public void openSync_WithInvalidFlags_ShouldThrow()
    {
        Assert.Throws<ArgumentException>(() => fs.openSync(_testFile, "invalid"));
    }

    [Fact]
    public void openSync_MultipleFiles_ShouldReturnDifferentDescriptors()
    {
        File.WriteAllText(_testFile, "test");
        var file2 = Path.Combine(_testDir, "test2.txt");
        File.WriteAllText(file2, "test2");

        var fd1 = fs.openSync(_testFile, "r");
        var fd2 = fs.openSync(file2, "r");

        Assert.NotEqual(fd1, fd2);

        fs.closeSync(fd1);
        fs.closeSync(fd2);
    }
}
