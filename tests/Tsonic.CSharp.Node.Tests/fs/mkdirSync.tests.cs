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
    public void mkdirSync_Recursive_ShouldCreateNestedDirectories()
    {
        var dirPath = GetTestPath("parent/child/grandchild");

        fs.mkdirSync(dirPath, recursive: true);

        Assert.True(Directory.Exists(dirPath));
    }

    [Fact]
    public void mkdirSync_NonRecursive_MissingParent_ShouldThrow()
    {
        var dirPath = GetTestPath("missing-parent/child");

        Assert.Throws<DirectoryNotFoundException>(() => fs.mkdirSync(dirPath, recursive: false));
    }

    [Fact]
    public void mkdirSync_MkdirOptions_Recursive_ShouldCreateNestedDirectories()
    {
        var dirPath = GetTestPath("opts-parent/child/grandchild");

        fs.mkdirSync(dirPath, new MkdirOptions { recursive = true });

        Assert.True(Directory.Exists(dirPath));
    }

    [Fact]
    public void mkdirSync_MkdirOptions_NonRecursive_MissingParent_ShouldThrow()
    {
        var dirPath = GetTestPath("opts-missing-parent/child");

        Assert.Throws<DirectoryNotFoundException>(() =>
            fs.mkdirSync(dirPath, new MkdirOptions { recursive = false }));
    }

    [Fact]
    public void mkdirSync_ObjectOptions_Recursive_ShouldCreateNestedDirectories()
    {
        var dirPath = GetTestPath("obj-parent/child/grandchild");

        fs.mkdirSync(dirPath, (object)new MkdirOptions { recursive = true });

        Assert.True(Directory.Exists(dirPath));
    }
}
