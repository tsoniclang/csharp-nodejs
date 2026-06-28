using Xunit;
using System;
using System.IO;
using System.Text;

namespace Tsonic.CSharp.Node.Tests;

public class Fs_readSyncTests : IDisposable
{
    private readonly string _testDir;
    private readonly string _testFile;

    public Fs_readSyncTests()
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
    public void readSync_ShouldReadEntireFile()
    {
        var content = "Hello, World!";
        File.WriteAllText(_testFile, content);

        var fd = fs.openSync(_testFile, "r");
        var buffer = new byte[100];
        var bytesRead = fs.readSync(fd, buffer, 0, buffer.Length, null);

        fs.closeSync(fd);

        var result = Encoding.UTF8.GetString(buffer, 0, bytesRead);
        Assert.Equal(content, result);
    }

    [Fact]
    public void readSync_WithOffset_ShouldReadIntoBufferAtOffset()
    {
        File.WriteAllText(_testFile, "Hello");

        var fd = fs.openSync(_testFile, "r");
        var buffer = new byte[10];
        buffer[0] = (byte)'X';
        buffer[1] = (byte)'Y';

        var bytesRead = fs.readSync(fd, buffer, 2, 5, null);

        fs.closeSync(fd);

        Assert.Equal((byte)'X', buffer[0]);
        Assert.Equal((byte)'Y', buffer[1]);
        Assert.Equal((byte)'H', buffer[2]);
        Assert.Equal(5, bytesRead);
    }

    [Fact]
    public void readSync_WithPosition_ShouldReadFromSpecificPosition()
    {
        File.WriteAllText(_testFile, "Hello, World!");

        var fd = fs.openSync(_testFile, "r");
        var buffer = new byte[5];
        var bytesRead = fs.readSync(fd, buffer, 0, 5, 7); // Read "World" starting at position 7

        fs.closeSync(fd);

        var result = Encoding.UTF8.GetString(buffer, 0, bytesRead);
        Assert.Equal("World", result);
    }

    [Fact]
    public void readSync_MultipleCalls_ShouldAdvanceFilePosition()
    {
        File.WriteAllText(_testFile, "ABCDEFGH");

        var fd = fs.openSync(_testFile, "r");
        var buffer1 = new byte[3];
        var buffer2 = new byte[3];

        var bytesRead1 = fs.readSync(fd, buffer1, 0, 3, null);
        var bytesRead2 = fs.readSync(fd, buffer2, 0, 3, null);

        fs.closeSync(fd);

        Assert.Equal("ABC", Encoding.UTF8.GetString(buffer1, 0, bytesRead1));
        Assert.Equal("DEF", Encoding.UTF8.GetString(buffer2, 0, bytesRead2));
    }

    [Fact]
    public void readSync_ReadPastEndOfFile_ShouldReturnZero()
    {
        File.WriteAllText(_testFile, "Hi");

        var fd = fs.openSync(_testFile, "r");
        var buffer = new byte[10];

        // Read all content first
        fs.readSync(fd, buffer, 0, 10, null);

        // Try to read again, should return 0
        var bytesRead = fs.readSync(fd, buffer, 0, 10, null);

        fs.closeSync(fd);

        Assert.Equal(0, bytesRead);
    }

    [Fact]
    public void readSync_WithNullBuffer_ShouldThrow()
    {
        File.WriteAllText(_testFile, "test");
        var fd = fs.openSync(_testFile, "r");

        Assert.Throws<ArgumentNullException>(() => fs.readSync(fd, (byte[])null!, 0, 10, null));
        Assert.Throws<ArgumentNullException>(() => fs.readSync(fd, (Buffer)null!, 0, 10, null));

        fs.closeSync(fd);
    }

    [Fact]
    public void readSync_WithInvalidDescriptor_ShouldThrow()
    {
        var buffer = new byte[10];

        Assert.Throws<ArgumentException>(() => fs.readSync(999, buffer, 0, 10, null));
    }

    [Fact]
    public void readSync_WithInvalidOffset_ShouldThrow()
    {
        File.WriteAllText(_testFile, "test");
        var fd = fs.openSync(_testFile, "r");
        var buffer = new byte[10];

        Assert.Throws<ArgumentOutOfRangeException>(() => fs.readSync(fd, buffer, -1, 5, null));
        Assert.Throws<ArgumentOutOfRangeException>(() => fs.readSync(fd, buffer, 20, 5, null));

        fs.closeSync(fd);
    }

    [Fact]
    public void readSync_WithInvalidLength_ShouldThrow()
    {
        File.WriteAllText(_testFile, "test");
        var fd = fs.openSync(_testFile, "r");
        var buffer = new byte[10];

        Assert.Throws<ArgumentOutOfRangeException>(() => fs.readSync(fd, buffer, 0, -1, null));
        Assert.Throws<ArgumentOutOfRangeException>(() => fs.readSync(fd, buffer, 5, 10, null));

        fs.closeSync(fd);
    }

    [Fact]
    public void readSync_EmptyFile_ShouldReturnZero()
    {
        File.WriteAllText(_testFile, "");

        var fd = fs.openSync(_testFile, "r");
        var buffer = new byte[10];
        var bytesRead = fs.readSync(fd, buffer, 0, 10, null);

        fs.closeSync(fd);

        Assert.Equal(0, bytesRead);
    }
}
