using System.Threading.Tasks;
using Xunit;

namespace Tsonic.CSharp.Node.Tests;

public class fsPromisesTests : FsTestBase
{
    [Fact]
    public async Task writeFile_And_readFile_ShouldRoundTrip()
    {
        var file = GetTestPath("promises.txt");
        await fs.promises.writeFile(file, "hello");

        var content = await fs.promises.readFile(file, "utf-8");
        Assert.Equal("hello", content);

        var buffer = await fs.promises.readFile(file);
        Assert.Equal(5, buffer.length);
    }

    [Fact]
    public async Task readFile_WithoutEncoding_ShouldReturnBuffer()
    {
        var file = GetTestPath("promises-buffer.bin");
        var content = new byte[] { 0x48, 0x65, 0x6c, 0x6c, 0x6f };
        File.WriteAllBytes(file, content);

        var buffer = await fs.promises.readFile(file);

        Assert.Equal(content.Length, buffer.length);
        for (var i = 0; i < content.Length; i++)
        {
            Assert.Equal(content[i], buffer[i]);
        }
        Assert.Equal("Hello", buffer.toString("utf-8"));
    }

    [Fact]
    public async Task stat_ShouldReturnFileInfo()
    {
        var file = GetTestPath("stats.txt");
        await fs.promises.writeFile(file, "abc");

        var stats = await fs.promises.stat(file);
        Assert.True(stats.isFile);
    }
}
