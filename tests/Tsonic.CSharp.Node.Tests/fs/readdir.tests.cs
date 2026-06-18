using Xunit;

namespace Tsonic.CSharp.Node.Tests;

public class readdirTests : FsTestBase
{
    [Fact]
    public async Task readdir_ShouldListDirectoryContents()
    {
        var dirPath = GetTestPath("read-dir-async");
        Directory.CreateDirectory(dirPath);
        File.WriteAllText(Path.Combine(dirPath, "file1.txt"), "content");
        File.WriteAllText(Path.Combine(dirPath, "file2.txt"), "content");
        Directory.CreateDirectory(Path.Combine(dirPath, "subdir"));

        var files = await fs.readdir(dirPath);

        Assert.Equal(3, files.Length);
        Assert.Contains("file1.txt", files);
        Assert.Contains("file2.txt", files);
        Assert.Contains("subdir", files);
    }

    [Fact]
    public async Task readdir_EmptyDirectory_ShouldReturnEmptyArray()
    {
        var dirPath = GetTestPath("empty-dir-async");
        Directory.CreateDirectory(dirPath);

        var files = await fs.readdir(dirPath);

        Assert.Empty(files);
    }
}
