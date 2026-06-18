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

        var content = await fs.promises.readFile(file);
        Assert.Equal("hello", content);
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
