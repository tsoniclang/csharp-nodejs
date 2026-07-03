using Xunit;

namespace Tsonic.CSharp.Node.Tests;

public class rmdirTests : FsTestBase
{
    [Fact]
    public async Task rmdir_ShouldRemoveEmptyDirectory()
    {
        var dirPath = GetTestPath("remove-dir-async");
        Directory.CreateDirectory(dirPath);

        await fs.rmdir(dirPath);

        Assert.False(Directory.Exists(dirPath));
    }

    [Fact]
    public async Task rmdir_Recursive_ShouldRemoveDirectoryWithContents()
    {
        var dirPath = GetTestPath("remove-tree-async");
        Directory.CreateDirectory(dirPath);
        File.WriteAllText(Path.Combine(dirPath, "file.txt"), "content");
        Directory.CreateDirectory(Path.Combine(dirPath, "subdir"));

        await fs.rmdir(dirPath, recursive: true);

        Assert.False(Directory.Exists(dirPath));
    }
}
