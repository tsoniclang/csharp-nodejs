using Xunit;
using System;
using System.IO;
using System.Threading.Tasks;

namespace Tsonic.CSharp.Node.Tests;

public class Fs_fstatTests : IDisposable
{
    private readonly string _testDir;
    private readonly string _testFile;

    public Fs_fstatTests()
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
    public async Task fstat_ShouldReturnStats()
    {
        File.WriteAllText(_testFile, "test content");

        var fd = fs.openSync(_testFile, "r");
        var stats = await fs.fstat(fd);

        fs.closeSync(fd);

        Assert.NotNull(stats);
        Assert.True(stats.size > 0);
        Assert.True(stats.isFile);
        Assert.False(stats.isDirectory);
    }

    [Fact]
    public async Task fstat_ShouldReturnCorrectSize()
    {
        var content = "Hello, World!";
        File.WriteAllText(_testFile, content);

        var fd = fs.openSync(_testFile, "r");
        var stats = await fs.fstat(fd);

        fs.closeSync(fd);

        Assert.Equal(content.Length, stats.size);
    }

    [Fact]
    public async Task fstat_WithInvalidDescriptor_ShouldThrow()
    {
        await Assert.ThrowsAsync<ArgumentException>(async () => await fs.fstat(999));
    }

    [Fact]
    public async Task fstat_EmptyFile_ShouldHaveZeroSize()
    {
        File.WriteAllText(_testFile, "");

        var fd = fs.openSync(_testFile, "r");
        var stats = await fs.fstat(fd);

        fs.closeSync(fd);

        Assert.Equal(0, stats.size);
    }
}
