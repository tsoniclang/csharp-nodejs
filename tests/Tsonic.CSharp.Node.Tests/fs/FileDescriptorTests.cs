using System;
using System.IO;
using System.Text;
using Xunit;

namespace Tsonic.CSharp.Node.Tests;

public class FileDescriptorTests : IDisposable
{
    private readonly string _testDir;

    public FileDescriptorTests()
    {
        _testDir = Path.Combine(Path.GetTempPath(), $"tsonic_fd_test_{Guid.NewGuid()}");
        Directory.CreateDirectory(_testDir);
    }

    public void Dispose()
    {
        if (Directory.Exists(_testDir))
        {
            Directory.Delete(_testDir, true);
        }
    }

    [Fact]
    public void OpenSync_ReadMode_ReturnsValidFd()
    {
        var filePath = Path.Combine(_testDir, "test.txt");
        File.WriteAllText(filePath, "test content");

        var fd = fs.openSync(filePath, "r");
        Assert.True(fd >= 3); // FDs start at 3

        fs.closeSync(fd);
    }

    [Fact]
    public void OpenSync_WriteMode_CreatesFile()
    {
        var filePath = Path.Combine(_testDir, "new.txt");

        var fd = fs.openSync(filePath, "w");
        Assert.True(fd >= 3);

        fs.closeSync(fd);
        Assert.True(File.Exists(filePath));
    }

    [Fact]
    public void ReadSync_ReadsDataIntoBuffer()
    {
        var filePath = Path.Combine(_testDir, "read.txt");
        var content = "Hello, World!";
        File.WriteAllText(filePath, content);

        var fd = fs.openSync(filePath, "r");
        var buffer = new byte[100];
        var bytesRead = fs.readSync(fd, buffer, 0, buffer.Length, 0);

        Assert.Equal(content.Length, bytesRead);
        Assert.Equal(content, Encoding.UTF8.GetString(buffer, 0, bytesRead));

        fs.closeSync(fd);
    }

    [Fact]
    public void WriteSync_WritesDataFromBuffer()
    {
        var filePath = Path.Combine(_testDir, "write.txt");
        var content = "Test write";
        var buffer = Encoding.UTF8.GetBytes(content);

        var fd = fs.openSync(filePath, "w");
        var bytesWritten = fs.writeSync(fd, buffer, 0, buffer.Length, null);

        Assert.Equal(buffer.Length, bytesWritten);
        fs.closeSync(fd);

        var written = File.ReadAllText(filePath);
        Assert.Equal(content, written);
    }

    [Fact]
    public void WriteSync_String_WritesText()
    {
        var filePath = Path.Combine(_testDir, "write-str.txt");
        var content = "String write test";

        var fd = fs.openSync(filePath, "w");
        var bytesWritten = fs.writeSync(fd, content, null, "utf8");

        Assert.True(bytesWritten > 0);
        fs.closeSync(fd);

        var written = File.ReadAllText(filePath);
        Assert.Equal(content, written);
    }

    [Fact]
    public void FstatSync_ReturnsStats()
    {
        var filePath = Path.Combine(_testDir, "stat.txt");
        File.WriteAllText(filePath, "test");

        var fd = fs.openSync(filePath, "r");
        var stats = fs.fstatSync(fd);

        Assert.True(stats.size > 0);
        Assert.True(stats.isFile);
        Assert.False(stats.isDirectory);

        fs.closeSync(fd);
    }

    [Fact]
    public void CloseSync_InvalidFd_Throws()
    {
        Assert.Throws<ArgumentException>(() => fs.closeSync(999));
    }

    [Fact]
    public async System.Threading.Tasks.Task Open_Async_Works()
    {
        var filePath = Path.Combine(_testDir, "async.txt");
        File.WriteAllText(filePath, "async test");

        var fd = await fs.open(filePath, "r");
        Assert.True(fd >= 3);

        await fs.close(fd);
    }

    [Fact]
    public async System.Threading.Tasks.Task Read_Async_Works()
    {
        var filePath = Path.Combine(_testDir, "async-read.txt");
        var content = "Async read test";
        File.WriteAllText(filePath, content);

        var fd = await fs.open(filePath, "r");
        var buffer = new byte[100];
        var bytesRead = await fs.read(fd, buffer, 0, buffer.Length, 0);

        Assert.Equal(content.Length, bytesRead);
        await fs.close(fd);
    }
}
