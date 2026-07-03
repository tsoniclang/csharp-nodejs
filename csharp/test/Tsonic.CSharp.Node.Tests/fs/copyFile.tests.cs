using Xunit;

namespace Tsonic.CSharp.Node.Tests;

public class copyFileTests : FsTestBase
{
    [Fact]
    public async Task copyFile_ShouldCopyFile()
    {
        var srcPath = GetTestPath("copy-src-async.txt");
        var destPath = GetTestPath("copy-dest-async.txt");
        var content = "Copy this content";
        File.WriteAllText(srcPath, content);

        await fs.copyFile(srcPath, destPath);

        Assert.True(File.Exists(destPath));
        Assert.Equal(content, File.ReadAllText(destPath));
    }

    [Fact]
    public async Task copyFile_ShouldOverwriteDestination()
    {
        var srcPath = GetTestPath("copy-src2-async.txt");
        var destPath = GetTestPath("copy-dest2-async.txt");
        File.WriteAllText(srcPath, "New content");
        File.WriteAllText(destPath, "Old content");

        await fs.copyFile(srcPath, destPath);

        Assert.Equal("New content", File.ReadAllText(destPath));
    }
}
