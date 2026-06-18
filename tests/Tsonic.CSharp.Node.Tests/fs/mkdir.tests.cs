using Xunit;

namespace Tsonic.CSharp.Node.Tests;

public class mkdirTests : FsTestBase
{
    [Fact]
    public async Task mkdir_ShouldCreateDirectory()
    {
        var dirPath = GetTestPath("new-dir-async");

        await fs.mkdir(dirPath);

        Assert.True(Directory.Exists(dirPath));
    }

    [Fact]
    public async Task mkdir_Recursive_ShouldCreateNestedDirectories()
    {
        var dirPath = GetTestPath("parent-async/child/grandchild");

        await fs.mkdir(dirPath, recursive: true);

        Assert.True(Directory.Exists(dirPath));
    }

    [Fact]
    public async Task mkdir_NonRecursive_MissingParent_ShouldThrow()
    {
        var dirPath = GetTestPath("missing-parent-async/child");

        await Assert.ThrowsAsync<DirectoryNotFoundException>(async () => await fs.mkdir(dirPath, recursive: false));
    }

    [Fact]
    public async Task mkdir_MkdirOptions_Recursive_ShouldCreateNestedDirectories()
    {
        var dirPath = GetTestPath("opts-parent-async/child/grandchild");

        await fs.mkdir(dirPath, new MkdirOptions { recursive = true });

        Assert.True(Directory.Exists(dirPath));
    }

    [Fact]
    public async Task mkdir_MkdirOptions_NonRecursive_MissingParent_ShouldThrow()
    {
        var dirPath = GetTestPath("opts-missing-parent-async/child");

        await Assert.ThrowsAsync<DirectoryNotFoundException>(async () =>
            await fs.mkdir(dirPath, new MkdirOptions { recursive = false }));
    }

    [Fact]
    public async Task mkdir_ObjectOptions_Recursive_ShouldCreateNestedDirectories()
    {
        var dirPath = GetTestPath("obj-parent-async/child/grandchild");

        await fs.mkdir(dirPath, (object)new MkdirOptions { recursive = true });

        Assert.True(Directory.Exists(dirPath));
    }
}
