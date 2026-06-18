using Xunit;
using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace Tsonic.CSharp.Node.Tests;

public class Fs_readTests : IDisposable
{
    private readonly string _testDir;
    private readonly string _testFile;

    public Fs_readTests()
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
    public async Task read_ShouldReadEntireFile()
    {
        var content = "Hello, World!";
        File.WriteAllText(_testFile, content);

        var fd = fs.openSync(_testFile, "r");
        var buffer = new byte[100];
        var bytesRead = await fs.read(fd, buffer, 0, buffer.Length, null);

        fs.closeSync(fd);

        var result = Encoding.UTF8.GetString(buffer, 0, bytesRead);
        Assert.Equal(content, result);
    }

    [Fact]
    public async Task read_WithPosition_ShouldReadFromSpecificPosition()
    {
        File.WriteAllText(_testFile, "Hello, World!");

        var fd = fs.openSync(_testFile, "r");
        var buffer = new byte[5];
        var bytesRead = await fs.read(fd, buffer, 0, 5, 7);

        fs.closeSync(fd);

        var result = Encoding.UTF8.GetString(buffer, 0, bytesRead);
        Assert.Equal("World", result);
    }

    [Fact]
    public async Task read_WithInvalidDescriptor_ShouldThrow()
    {
        var buffer = new byte[10];

        await Assert.ThrowsAsync<ArgumentException>(async () =>
            await fs.read(999, buffer, 0, 10, null));
    }

    [Fact]
    public async Task read_EmptyFile_ShouldReturnZero()
    {
        File.WriteAllText(_testFile, "");

        var fd = fs.openSync(_testFile, "r");
        var buffer = new byte[10];
        var bytesRead = await fs.read(fd, buffer, 0, 10, null);

        fs.closeSync(fd);

        Assert.Equal(0, bytesRead);
    }
}
