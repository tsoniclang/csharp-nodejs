using Xunit;
using System;
using System.IO;
using System.Text;

namespace Tsonic.CSharp.Node.Tests;

public class Fs_writeSyncTests : IDisposable
{
    private readonly string _testDir;
    private readonly string _testFile;

    public Fs_writeSyncTests()
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
    public void writeSync_WithBytes_ShouldWriteToFile()
    {
        var fd = fs.openSync(_testFile, "w");
        var data = Encoding.UTF8.GetBytes("Hello, World!");
        var bytesWritten = fs.writeSync(fd, data, 0, data.Length, null);

        fs.closeSync(fd);

        var content = File.ReadAllText(_testFile);
        Assert.Equal("Hello, World!", content);
        Assert.Equal(data.Length, bytesWritten);
    }

    [Fact]
    public void writeSync_WithString_ShouldWriteToFile()
    {
        var fd = fs.openSync(_testFile, "w");
        var bytesWritten = fs.writeSync(fd, "Hello, World!", null, null);

        fs.closeSync(fd);

        var content = File.ReadAllText(_testFile);
        Assert.Equal("Hello, World!", content);
        Assert.True(bytesWritten > 0);
    }

    [Fact]
    public void writeSync_WithOffset_ShouldWritePartialBuffer()
    {
        var fd = fs.openSync(_testFile, "w");
        var data = Encoding.UTF8.GetBytes("ABCDEFGH");
        var bytesWritten = fs.writeSync(fd, data, 2, 4, null); // Write "CDEF"

        fs.closeSync(fd);

        var content = File.ReadAllText(_testFile);
        Assert.Equal("CDEF", content);
        Assert.Equal(4, bytesWritten);
    }

    [Fact]
    public void writeSync_WithPosition_ShouldWriteAtSpecificPosition()
    {
        // Create file with initial content
        File.WriteAllText(_testFile, "XXXXXXXXXX");

        var fd = fs.openSync(_testFile, "r+");
        var data = Encoding.UTF8.GetBytes("Hi");
        fs.writeSync(fd, data, 0, data.Length, 3); // Write at position 3

        fs.closeSync(fd);

        var content = File.ReadAllText(_testFile);
        Assert.Equal("XXXHiXXXXX", content);
    }

    [Fact]
    public void writeSync_MultipleCalls_ShouldAdvanceFilePosition()
    {
        var fd = fs.openSync(_testFile, "w");

        fs.writeSync(fd, "Hello", null, null);
        fs.writeSync(fd, ", ", null, null);
        fs.writeSync(fd, "World!", null, null);

        fs.closeSync(fd);

        var content = File.ReadAllText(_testFile);
        Assert.Equal("Hello, World!", content);
    }

    [Fact]
    public void writeSync_WithAppendFlag_ShouldAppendToFile()
    {
        File.WriteAllText(_testFile, "Hello");

        var fd = fs.openSync(_testFile, "a");
        fs.writeSync(fd, ", World!", null, null);

        fs.closeSync(fd);

        var content = File.ReadAllText(_testFile);
        Assert.Equal("Hello, World!", content);
    }

    [Fact]
    public void writeSync_WithDifferentEncodings_ShouldWork()
    {
        var fd = fs.openSync(_testFile, "w");

        fs.writeSync(fd, "UTF8", null, "utf8");

        fs.closeSync(fd);

        var content = File.ReadAllText(_testFile);
        Assert.Equal("UTF8", content);
    }

    [Fact]
    public void writeSync_WithNullBuffer_ShouldThrow()
    {
        var fd = fs.openSync(_testFile, "w");

        Assert.Throws<ArgumentNullException>(() => fs.writeSync(fd, (byte[])null!, 0, 10, null));

        fs.closeSync(fd);
    }

    [Fact]
    public void writeSync_WithInvalidDescriptor_ShouldThrow()
    {
        var data = Encoding.UTF8.GetBytes("test");

        Assert.Throws<ArgumentException>(() => fs.writeSync(999, data, 0, data.Length, null));
    }

    [Fact]
    public void writeSync_WithInvalidOffset_ShouldThrow()
    {
        var fd = fs.openSync(_testFile, "w");
        var data = new byte[10];

        Assert.Throws<ArgumentOutOfRangeException>(() => fs.writeSync(fd, data, -1, 5, null));
        Assert.Throws<ArgumentOutOfRangeException>(() => fs.writeSync(fd, data, 20, 5, null));

        fs.closeSync(fd);
    }

    [Fact]
    public void writeSync_WithInvalidLength_ShouldThrow()
    {
        var fd = fs.openSync(_testFile, "w");
        var data = new byte[10];

        Assert.Throws<ArgumentOutOfRangeException>(() => fs.writeSync(fd, data, 0, -1, null));
        Assert.Throws<ArgumentOutOfRangeException>(() => fs.writeSync(fd, data, 5, 10, null));

        fs.closeSync(fd);
    }

    [Fact]
    public void writeSync_EmptyString_ShouldCreateEmptyFile()
    {
        var fd = fs.openSync(_testFile, "w");
        fs.closeSync(fd);

        var content = File.ReadAllText(_testFile);
        Assert.Empty(content);
    }

    [Fact]
    public void writeSync_LargeData_ShouldWriteCompletely()
    {
        var fd = fs.openSync(_testFile, "w");
        var largeData = new string('A', 10000);
        var bytesWritten = fs.writeSync(fd, largeData, null, null);

        fs.closeSync(fd);

        var content = File.ReadAllText(_testFile);
        Assert.Equal(10000, content.Length);
        Assert.Equal(largeData, content);
    }
}
