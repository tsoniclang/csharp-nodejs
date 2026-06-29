using System.IO;
using System.Threading.Tasks;
using Xunit;

namespace Tsonic.CSharp.Node.Tests;

public class FsPromisesModuleTests : FsTestBase
{
    [Fact]
    public async Task FsPromisesModule_ReadsAndWritesFiles()
    {
        var path = GetTestPath("sample.txt");

        await fs_promises.writeFile(path, "hello");
        var text = await fs_promises.readFile(path);

        Assert.Equal("hello", text);
        Assert.True(File.Exists(path));
    }

    [Fact]
    public async Task FsPromisesModule_StatsAndRemovesFiles()
    {
        var path = GetTestPath("remove-me.txt");

        await fs_promises.writeFile(path, "hello");
        var stats = await fs_promises.stat(path);
        await fs_promises.unlink(path);

        Assert.True(stats.isFile);
        Assert.False(File.Exists(path));
    }

    [Fact]
    public async Task FsPromisesModule_CreatesAndReadsDirectory()
    {
        var dir = GetTestPath("dir");
        var child = Path.Combine(dir, "child.txt");

        await fs_promises.mkdir(dir);
        await fs_promises.writeFile(child, "data");
        var entries = await fs_promises.readdir(dir);

        Assert.Contains("child.txt", entries);
    }
}
