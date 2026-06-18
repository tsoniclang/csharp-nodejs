using Xunit;

namespace Tsonic.CSharp.Node.Tests;

public class readlinkTests : FsTestBase
{
    [Fact]
    public async Task readlink_ShouldReturnTargetPath()
    {
        var targetPath = GetTestPath("readlink-target-async.txt");
        var linkPath = GetTestPath("readlink-link-async.txt");
        File.WriteAllText(targetPath, "content");
        await fs.symlink(targetPath, linkPath);

        var target = await fs.readlink(linkPath);

        Assert.NotNull(target);
        Assert.NotEmpty(target);
    }

    [Fact]
    public async Task readlink_NonSymlink_ShouldThrow()
    {
        var filePath = GetTestPath("regular-file-async.txt");
        File.WriteAllText(filePath, "content");

        await Assert.ThrowsAsync<IOException>(async () => await fs.readlink(filePath));
    }

    [Fact]
    public async Task readlink_NonExistent_ShouldThrow()
    {
        var linkPath = GetTestPath("nonexistent-link-async.txt");

        await Assert.ThrowsAsync<FileNotFoundException>(async () => await fs.readlink(linkPath));
    }
}
