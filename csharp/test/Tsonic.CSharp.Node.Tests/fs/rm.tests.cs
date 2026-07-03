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
    public async Task rm_ShouldRemoveEmptyDirectory()
    {
        var dirPath = GetTestPath("rm-dir-async");
        Directory.CreateDirectory(dirPath);

        await fs.rm(dirPath);

        Assert.False(Directory.Exists(dirPath));
    }

    [Fact]
    public async Task rm_Recursive_ShouldRemoveDirectoryWithContents()
    {
        var dirPath = GetTestPath("rm-tree-async");
        Directory.CreateDirectory(dirPath);
        File.WriteAllText(Path.Combine(dirPath, "file.txt"), "content");
        Directory.CreateDirectory(Path.Combine(dirPath, "subdir"));

        await fs.rm(dirPath, recursive: true);

        Assert.False(Directory.Exists(dirPath));
    }

    [Fact]
    public async Task rm_NonRecursive_DirectoryWithContents_ShouldThrow()
    {
        var dirPath = GetTestPath("rm-non-recursive-async");
        Directory.CreateDirectory(dirPath);
        File.WriteAllText(Path.Combine(dirPath, "file.txt"), "content");

        await Assert.ThrowsAsync<IOException>(async () => await fs.rm(dirPath, recursive: false));
    }

    [Fact]
    public async Task rm_NonExistent_ShouldNotThrow()
    {
        var filePath = GetTestPath("nonexistent-rm-async.txt");

        // rm doesn't throw if path doesn't exist (like Node.js with force: true)
        await fs.rm(filePath);
    }
}
