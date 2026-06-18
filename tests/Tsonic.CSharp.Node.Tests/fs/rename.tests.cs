using Xunit;

namespace Tsonic.CSharp.Node.Tests;

public class renameTests : FsTestBase
{
    [Fact]
    public async Task rename_File_ShouldRenameFile()
    {
        var oldPath = GetTestPath("old-name-async.txt");
        var newPath = GetTestPath("new-name-async.txt");
        var content = "Content";
        File.WriteAllText(oldPath, content);

        await fs.rename(oldPath, newPath);

        Assert.False(File.Exists(oldPath));
        Assert.True(File.Exists(newPath));
        Assert.Equal(content, File.ReadAllText(newPath));
    }

    [Fact]
    public async Task rename_Directory_ShouldRenameDirectory()
    {
        var oldPath = GetTestPath("old-dir-async");
        var newPath = GetTestPath("new-dir-async-renamed");
        Directory.CreateDirectory(oldPath);
        File.WriteAllText(Path.Combine(oldPath, "file.txt"), "content");

        await fs.rename(oldPath, newPath);

        Assert.False(Directory.Exists(oldPath));
        Assert.True(Directory.Exists(newPath));
        Assert.True(File.Exists(Path.Combine(newPath, "file.txt")));
    }

    [Fact]
    public async Task rename_NonExistent_ShouldThrow()
    {
        var oldPath = GetTestPath("nonexistent-async-rename.txt");
        var newPath = GetTestPath("new-async.txt");

        await Assert.ThrowsAsync<FileNotFoundException>(async () => await fs.rename(oldPath, newPath));
    }
}
