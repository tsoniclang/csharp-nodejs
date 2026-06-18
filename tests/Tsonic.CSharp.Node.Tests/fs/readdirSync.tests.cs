using Xunit;

namespace Tsonic.CSharp.Node.Tests;

public class readdirSyncTests : FsTestBase
{
    [Fact]
    public void readdirSync_ShouldListDirectoryContents()
    {
        var dirPath = GetTestPath("read-dir");
        Directory.CreateDirectory(dirPath);
        File.WriteAllText(Path.Combine(dirPath, "file1.txt"), "content");
        File.WriteAllText(Path.Combine(dirPath, "file2.txt"), "content");
        Directory.CreateDirectory(Path.Combine(dirPath, "subdir"));

        var files = fs.readdirSync(dirPath);

        Assert.Equal(3, files.Length);
        Assert.Contains("file1.txt", files);
        Assert.Contains("file2.txt", files);
        Assert.Contains("subdir", files);
    }

    [Fact]
    public void readdirSync_EmptyDirectory_ShouldReturnEmptyArray()
    {
        var dirPath = GetTestPath("empty-dir");
        Directory.CreateDirectory(dirPath);

        var files = fs.readdirSync(dirPath);

        Assert.Empty(files);
    }
}
