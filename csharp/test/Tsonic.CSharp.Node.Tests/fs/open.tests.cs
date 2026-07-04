using Xunit;
using System;
using System.IO;
using System.Threading.Tasks;

namespace Tsonic.CSharp.Node.Tests;

public class Fs_openTests : IDisposable
{
    private readonly string _testDir;
    private readonly string _testFile;

    public Fs_openTests()
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
    public async Task open_WithReadFlag_ShouldOpenExistingFile()
    {
        File.WriteAllText(_testFile, "test content");

        var fd = await fs.open(_testFile, "r");
        Assert.True(fd >= 0);

        fs.closeSync(fd);
    }

    [Fact]
    public async Task open_WithReadFlag_NonExistentFile_ShouldThrow()
    {
        var nonExistentFile = Path.Combine(_testDir, "nonexistent.txt");

        await Assert.ThrowsAsync<IOException>(async () => await fs.open(nonExistentFile, "r"));
    }

    [Fact]
    public async Task open_WithWriteFlag_ShouldCreateFile()
    {
        var fd = await fs.open(_testFile, "w");
        Assert.True(fd >= 0);

        fs.closeSync(fd);
        Assert.True(File.Exists(_testFile));
    }

    [Fact]
    public async Task open_WithAppendFlag_ShouldCreateFile()
    {
        var fd = await fs.open(_testFile, "a");
        Assert.True(fd >= 0);

        fs.closeSync(fd);
        Assert.True(File.Exists(_testFile));
    }

    [Fact]
    public async Task open_WithReadPlusFlag_ShouldOpenForReadWrite()
    {
        File.WriteAllText(_testFile, "test");

        var fd = await fs.open(_testFile, "r+");
        Assert.True(fd >= 0);

        fs.closeSync(fd);
    }

    [Fact]
    public async Task open_WithExclusiveWriteFlag_ExistingFile_ShouldThrow()
    {
        File.WriteAllText(_testFile, "test");

        await Assert.ThrowsAsync<IOException>(async () => await fs.open(_testFile, "wx"));
    }
}
