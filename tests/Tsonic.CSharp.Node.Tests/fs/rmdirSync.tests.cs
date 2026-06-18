using Xunit;

namespace Tsonic.CSharp.Node.Tests;

public class rmdirSyncTests : FsTestBase
{
    [Fact]
    public void rmdirSync_ShouldRemoveEmptyDirectory()
    {
        var dirPath = GetTestPath("remove-dir");
        Directory.CreateDirectory(dirPath);

        fs.rmdirSync(dirPath);

        Assert.False(Directory.Exists(dirPath));
    }

    [Fact]
    public void rmdirSync_Recursive_ShouldRemoveDirectoryWithContents()
    {
        var dirPath = GetTestPath("remove-tree");
        Directory.CreateDirectory(dirPath);
        File.WriteAllText(Path.Combine(dirPath, "file.txt"), "content");
        Directory.CreateDirectory(Path.Combine(dirPath, "subdir"));

        fs.rmdirSync(dirPath, recursive: true);

        Assert.False(Directory.Exists(dirPath));
    }
}
