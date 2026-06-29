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

    [Fact]
    public void readdirSync_WithFileTypes_ShouldReturnDirents()
    {
        var dirPath = GetTestPath("read-dir-dirents");
        Directory.CreateDirectory(dirPath);
        File.WriteAllText(Path.Combine(dirPath, "file.txt"), "content");
        Directory.CreateDirectory(Path.Combine(dirPath, "subdir"));

        var entries = fs.readdirSync(dirPath, withFileTypes: true);

        var file = Assert.IsType<Dirent>(Assert.Single(entries, entry => ((Dirent)entry).name == "file.txt"));
        var subdir = Assert.IsType<Dirent>(Assert.Single(entries, entry => ((Dirent)entry).name == "subdir"));
        Assert.True(file.isFile());
        Assert.False(file.isDirectory());
        Assert.True(subdir.isDirectory());
        Assert.False(subdir.isFile());
    }

    [Fact]
    public void readdirDirentsSync_ShouldReturnTypedDirents()
    {
        var dirPath = GetTestPath("read-dir-typed-dirents");
        Directory.CreateDirectory(dirPath);
        File.WriteAllText(Path.Combine(dirPath, "file.txt"), "content");

        var entries = fs.readdirDirentsSync(dirPath);

        var entry = Assert.Single(entries);
        Assert.Equal("file.txt", entry.name);
        Assert.True(entry.isFile());
    }
}
