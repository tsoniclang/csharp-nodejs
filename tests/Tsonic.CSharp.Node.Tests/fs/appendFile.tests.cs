using Xunit;

namespace Tsonic.CSharp.Node.Tests;

public class appendFileTests : FsTestBase
{
    [Fact]
    public async Task appendFile_ShouldAppendToFile()
    {
        var filePath = GetTestPath("append-test-async.txt");
        File.WriteAllText(filePath, "Line 1\n");

        await fs.appendFile(filePath, "Line 2\n", "utf-8");

        var content = File.ReadAllText(filePath);
        Assert.Contains("Line 1", content);
        Assert.Contains("Line 2", content);
    }

    [Fact]
    public async Task appendFile_NonExistentFile_ShouldCreateFile()
    {
        var filePath = GetTestPath("append-new-async.txt");

        await fs.appendFile(filePath, "New content", "utf-8");

        Assert.True(File.Exists(filePath));
        Assert.Equal("New content", File.ReadAllText(filePath));
    }

    [Fact]
    public async Task appendFile_ShouldAppendBufferData()
    {
        var filePath = GetTestPath("append-buffer-async.bin");
        File.WriteAllBytes(filePath, new byte[] { 0x01, 0x02 });

        await fs.appendFile(filePath, Buffer.from(new byte[] { 0x03, 0x04 }));

        Assert.Equal(new byte[] { 0x01, 0x02, 0x03, 0x04 }, File.ReadAllBytes(filePath));
    }
}
