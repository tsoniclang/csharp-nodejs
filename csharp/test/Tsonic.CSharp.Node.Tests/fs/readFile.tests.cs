using Xunit;

namespace Tsonic.CSharp.Node.Tests;

public class readFileTests : FsTestBase
{
    [Fact]
    public async Task readFile_ShouldReadTextFile()
    {
        var filePath = GetTestPath("test-async.txt");
        var content = "Hello, Async World!";
        File.WriteAllText(filePath, content);

        var result = await fs.readFile(filePath, "utf-8");
        Assert.Equal(content, result);
    }

    [Fact]
    public async Task readFile_NonExistentFile_ShouldThrow()
    {
        var filePath = GetTestPath("nonexistent-async.txt");
        await Assert.ThrowsAsync<FileNotFoundException>(async () => await fs.readFile(filePath, "utf-8"));
    }

    [Fact]
    public async Task readFile_WithoutEncoding_ShouldReturnBuffer()
    {
        var filePath = GetTestPath("buffer-async.bin");
        var content = new byte[] { 0x10, 0x20, 0x30, 0x40 };
        File.WriteAllBytes(filePath, content);

        var result = await fs.readFile(filePath);

        Assert.NotNull(result);
        Assert.Equal(content.Length, result.length);
        for (var i = 0; i < content.Length; i++)
        {
            Assert.Equal(content[i], result[i]);
        }
    }
}
