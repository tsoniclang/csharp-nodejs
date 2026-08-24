using Xunit;

namespace Tsonic.CSharp.Node.Tests;

public class mkdirSyncTests : FsTestBase
{
    [Fact]
    public void mkdirSync_ShouldCreateDirectory()
    {
        var dirPath = GetTestPath("new-dir");

        fs.mkdirSync(dirPath);

        Assert.True(Directory.Exists(dirPath));
    }

    [Fact]
    public void mkdirSync_RecursiveOptions_ShouldCreateNestedDirectories()
    {
        var dirPath = GetTestPath("parent/child/grandchild");

        fs.mkdirSync(dirPath, new MakeDirectoryOptions { recursive = true });
        fs.mkdirSync(dirPath, new MakeDirectoryOptions { recursive = true });

        Assert.True(Directory.Exists(dirPath));
    }

    [Fact]
    public void mkdirSync_NonRecursiveOptions_MissingParent_ShouldThrow()
    {
        var dirPath = GetTestPath("missing-parent/child");

        Assert.Throws<DirectoryNotFoundException>(() =>
            fs.mkdirSync(dirPath, new MakeDirectoryOptions { recursive = false }));
    }

    [Fact]
    public void mkdirSync_NonRecursiveOptions_ExistingDirectory_ShouldThrow()
    {
        var dirPath = GetTestPath("existing-directory");
        Directory.CreateDirectory(dirPath);

        Assert.Throws<IOException>(() => fs.mkdirSync(dirPath));
        fs.mkdirSync(dirPath, new MakeDirectoryOptions { recursive = true });
    }

    [Fact]
    public void mkdirSync_InvalidMode_ShouldThrow()
    {
        var dirPath = GetTestPath("invalid-mode");

        Assert.Throws<ArgumentOutOfRangeException>(() =>
            fs.mkdirSync(dirPath, new MakeDirectoryOptions { mode = 1.5 }));
    }
}
