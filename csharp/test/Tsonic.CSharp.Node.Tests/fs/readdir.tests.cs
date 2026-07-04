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

    [Fact]
    public async Task readdir_WithFileTypes_ShouldReturnDirents()
    {
        var dirPath = GetTestPath("read-dir-async-dirents");
        Directory.CreateDirectory(dirPath);
        File.WriteAllText(Path.Combine(dirPath, "file.txt"), "content");
        Directory.CreateDirectory(Path.Combine(dirPath, "subdir"));

        var entries = await fs.readdir(dirPath, withFileTypes: true);

        var file = Assert.IsType<Dirent>(Assert.Single(entries, entry => ((Dirent)entry).name == "file.txt"));
        var subdir = Assert.IsType<Dirent>(Assert.Single(entries, entry => ((Dirent)entry).name == "subdir"));
        Assert.True(file.isFile());
        Assert.False(file.isDirectory());
        Assert.True(subdir.isDirectory());
        Assert.False(subdir.isFile());
        Assert.Equal(dirPath, file.parentPath);
    }

    [Fact]
    public async Task readdirDirents_ShouldReturnTypedDirents()
    {
        var dirPath = GetTestPath("read-dir-async-typed-dirents");
        Directory.CreateDirectory(dirPath);
        File.WriteAllText(Path.Combine(dirPath, "file.txt"), "content");

        var entries = await fs.readdirDirents(dirPath);

        var entry = Assert.Single(entries);
        Assert.Equal("file.txt", entry.name);
        Assert.True(entry.isFile());
    }
}
