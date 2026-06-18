using Xunit;
using System;
using System.IO;
using System.Threading.Tasks;

namespace Tsonic.CSharp.Node.Tests;

public class Fs_closeTests : IDisposable
{
    private readonly string _testDir;
    private readonly string _testFile;

    public Fs_closeTests()
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
    public async Task close_ShouldCloseValidDescriptor()
    {
        File.WriteAllText(_testFile, "test");
        var fd = fs.openSync(_testFile, "r");

        await fs.close(fd);

        // Trying to use closed fd should throw
        Assert.Throws<ArgumentException>(() => fs.fstatSync(fd));
    }

    [Fact]
    public async Task close_WithInvalidDescriptor_ShouldThrow()
    {
        await Assert.ThrowsAsync<ArgumentException>(async () => await fs.close(999));
    }

    [Fact]
    public async Task close_AfterWrite_ShouldFlushAndClose()
    {
        var fd = fs.openSync(_testFile, "w");
        var data = System.Text.Encoding.UTF8.GetBytes("test content");
        fs.writeSync(fd, data, 0, data.Length, null);

        await fs.close(fd);

        // Verify data was written
        var content = File.ReadAllText(_testFile);
        Assert.Equal("test content", content);
    }
}
