using Xunit;
using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace Tsonic.CSharp.Node.Tests;

public class Fs_writeTests : IDisposable
{
    private readonly string _testDir;
    private readonly string _testFile;

    public Fs_writeTests()
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
    public async Task write_WithBytes_ShouldWriteToFile()
    {
        var fd = fs.openSync(_testFile, "w");
        var data = Encoding.UTF8.GetBytes("Hello, World!");
        var bytesWritten = await fs.write(fd, data, 0, data.Length, null);

        fs.closeSync(fd);

        var content = File.ReadAllText(_testFile);
        Assert.Equal("Hello, World!", content);
        Assert.Equal(data.Length, bytesWritten);
    }

    [Fact]
    public async Task write_WithString_ShouldWriteToFile()
    {
        var fd = fs.openSync(_testFile, "w");
        var bytesWritten = await fs.write(fd, "Hello, World!", null, null);

        fs.closeSync(fd);

        var content = File.ReadAllText(_testFile);
        Assert.Equal("Hello, World!", content);
        Assert.True(bytesWritten > 0);
    }

    [Fact]
    public async Task write_WithPosition_ShouldWriteAtSpecificPosition()
    {
        File.WriteAllText(_testFile, "XXXXXXXXXX");

        var fd = fs.openSync(_testFile, "r+");
        var data = Encoding.UTF8.GetBytes("Hi");
        await fs.write(fd, data, 0, data.Length, 3);

        fs.closeSync(fd);

        var content = File.ReadAllText(_testFile);
        Assert.Equal("XXXHiXXXXX", content);
    }

    [Fact]
    public async Task write_WithInvalidDescriptor_ShouldThrow()
    {
        var data = Encoding.UTF8.GetBytes("test");

        await Assert.ThrowsAsync<ArgumentException>(async () =>
            await fs.write(999, data, 0, data.Length, null));
    }

    [Fact]
    public void write_EmptyString_ShouldCreateEmptyFile()
    {
        var fd = fs.openSync(_testFile, "w");
        fs.closeSync(fd);

        var content = File.ReadAllText(_testFile);
        Assert.Empty(content);
    }
}
