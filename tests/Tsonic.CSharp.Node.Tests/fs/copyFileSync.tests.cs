using Xunit;

namespace Tsonic.CSharp.Node.Tests;

public class copyFileSyncTests : FsTestBase
{
    [Fact]
    public void copyFileSync_ShouldCopyFile()
    {
        var srcPath = GetTestPath("copy-src.txt");
        var destPath = GetTestPath("copy-dest.txt");
        var content = "Copy this content";
        File.WriteAllText(srcPath, content);

        fs.copyFileSync(srcPath, destPath);

        Assert.True(File.Exists(destPath));
        Assert.Equal(content, File.ReadAllText(destPath));
    }

    [Fact]
    public void copyFileSync_ShouldOverwriteDestination()
    {
        var srcPath = GetTestPath("copy-src2.txt");
        var destPath = GetTestPath("copy-dest2.txt");
        File.WriteAllText(srcPath, "New content");
        File.WriteAllText(destPath, "Old content");

        fs.copyFileSync(srcPath, destPath);

        Assert.Equal("New content", File.ReadAllText(destPath));
    }
}
