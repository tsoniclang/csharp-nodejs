using Xunit;
using System;
using System.IO;

namespace Tsonic.CSharp.Node.Tests;

public class Fs_closeSyncTests : IDisposable
{
    private readonly string _testDir;
    private readonly string _testFile;

    public Fs_closeSyncTests()
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
    public void closeSync_ShouldCloseValidDescriptor()
    {
        File.WriteAllText(_testFile, "test");
        var fd = fs.openSync(_testFile, "r");

        fs.closeSync(fd);

        // Trying to use closed fd should throw
        Assert.Throws<ArgumentException>(() => fs.fstatSync(fd));
    }

    [Fact]
    public void closeSync_WithInvalidDescriptor_ShouldThrow()
    {
        Assert.Throws<ArgumentException>(() => fs.closeSync(999));
    }

    [Fact]
    public void closeSync_CalledTwice_ShouldThrowSecondTime()
    {
        File.WriteAllText(_testFile, "test");
        var fd = fs.openSync(_testFile, "r");

        fs.closeSync(fd);

        Assert.Throws<ArgumentException>(() => fs.closeSync(fd));
    }

    [Fact]
    public void closeSync_AfterWrite_ShouldFlushAndClose()
    {
        var fd = fs.openSync(_testFile, "w");
        var data = System.Text.Encoding.UTF8.GetBytes("test content");
        fs.writeSync(fd, data, 0, data.Length, null);

        fs.closeSync(fd);

        // Verify data was written
        var content = File.ReadAllText(_testFile);
        Assert.Equal("test content", content);
    }
}
