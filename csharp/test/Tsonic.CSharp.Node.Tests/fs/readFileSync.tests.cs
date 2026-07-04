using Xunit;

namespace Tsonic.CSharp.Node.Tests;

public class readFileSyncTests : FsTestBase
{
    [Fact]
    public void readFileSync_ShouldReadTextFile()
    {
        var filePath = GetTestPath("test.txt");
        var content = "Hello, World!";
        File.WriteAllText(filePath, content);

        var result = fs.readFileSync(filePath, "utf-8");
        Assert.Equal(content, result);
    }

    [Fact]
    public void readFileSync_NonExistentFile_ShouldThrow()
    {
        var filePath = GetTestPath("nonexistent.txt");
        Assert.Throws<FileNotFoundException>(() => fs.readFileSync(filePath, "utf-8"));
    }

    [Fact]
    public void readFileSync_WithoutEncoding_ShouldReturnBuffer()
    {
        var filePath = GetTestPath("buffer.bin");
        var content = new byte[] { 0x01, 0x02, 0x41, 0x42, 0x43 };
        File.WriteAllBytes(filePath, content);

        var result = fs.readFileSync(filePath);

        Assert.NotNull(result);
        Assert.Equal(content.Length, result.length);
        for (var i = 0; i < content.Length; i++)
        {
            Assert.Equal(content[i], result[i]);
        }
    }
}
