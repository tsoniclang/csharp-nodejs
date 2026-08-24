using Xunit;

namespace Tsonic.CSharp.Node.Tests;

public class rmTests : FsTestBase
{
    [Fact]
    public async Task rm_ShouldRemoveFile()
    {
        var filePath = GetTestPath("rm-test-async.txt");
        File.WriteAllText(filePath, "content");

        await fs.rm(filePath);

        Assert.False(File.Exists(filePath));
    }

    [Fact]
    public async Task rm_NonRecursiveOptions_EmptyDirectory_ShouldThrow()
    {
        var dirPath = GetTestPath("rm-dir-async");
        Directory.CreateDirectory(dirPath);

        await Assert.ThrowsAsync<IOException>(async () => await fs.rm(dirPath));
        Assert.True(Directory.Exists(dirPath));
    }

    [Fact]
    public async Task rm_RecursiveOptions_ShouldRemoveDirectoryWithContents()
    {
        var dirPath = GetTestPath("rm-tree-async");
        Directory.CreateDirectory(dirPath);
        File.WriteAllText(Path.Combine(dirPath, "file.txt"), "content");
        Directory.CreateDirectory(Path.Combine(dirPath, "subdir"));

        await fs.rm(dirPath, new RmOptions { recursive = true });

        Assert.False(Directory.Exists(dirPath));
    }

    [Fact]
    public async Task rm_NonRecursiveOptions_DirectoryWithContents_ShouldThrow()
    {
        var dirPath = GetTestPath("rm-non-recursive-async");
        Directory.CreateDirectory(dirPath);
        File.WriteAllText(Path.Combine(dirPath, "file.txt"), "content");

        await Assert.ThrowsAsync<IOException>(async () =>
            await fs.rm(dirPath, new RmOptions { recursive = false }));
    }

    [Fact]
    public async Task rm_MissingPath_RequiresExplicitForce()
    {
        var path = GetTestPath("nonexistent-rm-async.txt");

        await Assert.ThrowsAsync<FileNotFoundException>(async () => await fs.rm(path));
        await fs.rm(path, new RmOptions { force = true });
    }
}
