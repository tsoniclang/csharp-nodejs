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

        var utf8Content = await fs.promises.readFile(file, "utf8");
        Assert.Equal("hello", utf8Content);

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

    [Fact]
    public async Task writeFile_And_appendFile_ShouldAcceptBufferData()
    {
        var file = GetTestPath("promises-buffer-write.bin");

        await fs.promises.writeFile(file, Buffer.from(new byte[] { 0x61, 0x62 }));
        await fs.promises.appendFile(file, Buffer.from(new byte[] { 0x63 }));

        Assert.Equal(new byte[] { 0x61, 0x62, 0x63 }, File.ReadAllBytes(file));
    }
}
