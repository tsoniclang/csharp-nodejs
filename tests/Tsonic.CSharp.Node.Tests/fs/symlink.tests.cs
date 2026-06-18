using Xunit;

namespace Tsonic.CSharp.Node.Tests;

public class symlinkTests : FsTestBase
{
    [Fact]
    public async Task symlink_File_ShouldCreateSymbolicLink()
    {
        var targetPath = GetTestPath("symlink-target-async.txt");
        var linkPath = GetTestPath("symlink-link-async.txt");
        File.WriteAllText(targetPath, "content");

        await fs.symlink(targetPath, linkPath);

        Assert.True(File.Exists(linkPath));
        var fileInfo = new FileInfo(linkPath);
        Assert.True(fileInfo.LinkTarget != null || fileInfo.Attributes.HasFlag(FileAttributes.ReparsePoint));
    }

    [Fact]
    public async Task symlink_Directory_ShouldCreateSymbolicLink()
    {
        var targetPath = GetTestPath("symlink-target-dir-async");
        var linkPath = GetTestPath("symlink-link-dir-async");
        Directory.CreateDirectory(targetPath);

        await fs.symlink(targetPath, linkPath, "dir");

        Assert.True(Directory.Exists(linkPath));
        var dirInfo = new DirectoryInfo(linkPath);
        Assert.True(dirInfo.LinkTarget != null || dirInfo.Attributes.HasFlag(FileAttributes.ReparsePoint));
    }
}
