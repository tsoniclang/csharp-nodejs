using Xunit;

namespace Tsonic.CSharp.Node.Tests;

public class symlinkSyncTests : FsTestBase
{
    [Fact]
    public void symlinkSync_File_ShouldCreateSymbolicLink()
    {
        var targetPath = GetTestPath("symlink-target.txt");
        var linkPath = GetTestPath("symlink-link.txt");
        File.WriteAllText(targetPath, "content");

        fs.symlinkSync(targetPath, linkPath);

        Assert.True(File.Exists(linkPath));
        var fileInfo = new FileInfo(linkPath);
        Assert.True(fileInfo.LinkTarget != null || fileInfo.Attributes.HasFlag(FileAttributes.ReparsePoint));
    }

    [Fact]
    public void symlinkSync_Directory_ShouldCreateSymbolicLink()
    {
        var targetPath = GetTestPath("symlink-target-dir");
        var linkPath = GetTestPath("symlink-link-dir");
        Directory.CreateDirectory(targetPath);

        fs.symlinkSync(targetPath, linkPath, "dir");

        Assert.True(Directory.Exists(linkPath));
        var dirInfo = new DirectoryInfo(linkPath);
        Assert.True(dirInfo.LinkTarget != null || dirInfo.Attributes.HasFlag(FileAttributes.ReparsePoint));
    }
}
