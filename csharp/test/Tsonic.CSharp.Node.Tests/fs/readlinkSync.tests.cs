using Xunit;

namespace Tsonic.CSharp.Node.Tests;

public class readlinkSyncTests : FsTestBase
{
    [Fact]
    public void readlinkSync_ShouldReturnTargetPath()
    {
        var targetPath = GetTestPath("readlink-target.txt");
        var linkPath = GetTestPath("readlink-link.txt");
        File.WriteAllText(targetPath, "content");
        fs.symlinkSync(targetPath, linkPath);

        var target = fs.readlinkSync(linkPath);

        Assert.NotNull(target);
        Assert.NotEmpty(target);
    }

    [Fact]
    public void readlinkSync_NonSymlink_ShouldThrow()
    {
        var filePath = GetTestPath("regular-file.txt");
        File.WriteAllText(filePath, "content");

        Assert.Throws<IOException>(() => fs.readlinkSync(filePath));
    }

    [Fact]
    public void readlinkSync_NonExistent_ShouldThrow()
    {
        var linkPath = GetTestPath("nonexistent-link.txt");

        Assert.Throws<FileNotFoundException>(() => fs.readlinkSync(linkPath));
    }
}
