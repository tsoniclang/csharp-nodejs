using Xunit;

namespace Tsonic.CSharp.Node.Tests;

public class renameSyncTests : FsTestBase
{
    [Fact]
    public void renameSync_File_ShouldRenameFile()
    {
        var oldPath = GetTestPath("old-name.txt");
        var newPath = GetTestPath("new-name.txt");
        var content = "Content";
        File.WriteAllText(oldPath, content);

        fs.renameSync(oldPath, newPath);

        Assert.False(File.Exists(oldPath));
        Assert.True(File.Exists(newPath));
        Assert.Equal(content, File.ReadAllText(newPath));
    }

    [Fact]
    public void renameSync_Directory_ShouldRenameDirectory()
    {
        var oldPath = GetTestPath("old-dir");
        var newPath = GetTestPath("new-dir");
        Directory.CreateDirectory(oldPath);
        File.WriteAllText(Path.Combine(oldPath, "file.txt"), "content");

        fs.renameSync(oldPath, newPath);

        Assert.False(Directory.Exists(oldPath));
        Assert.True(Directory.Exists(newPath));
        Assert.True(File.Exists(Path.Combine(newPath, "file.txt")));
    }

    [Fact]
    public void renameSync_NonExistent_ShouldThrow()
    {
        var oldPath = GetTestPath("nonexistent.txt");
        var newPath = GetTestPath("new.txt");

        Assert.Throws<FileNotFoundException>(() => fs.renameSync(oldPath, newPath));
    }
}
