using Xunit;

namespace Tsonic.CSharp.Node.Tests;

public class rmSyncTests : FsTestBase
{
    [Fact]
    public void rmSync_ShouldRemoveFile()
    {
        var filePath = GetTestPath("rm-test.txt");
        File.WriteAllText(filePath, "content");

        fs.rmSync(filePath);

        Assert.False(File.Exists(filePath));
    }

    [Fact]
    public void rmSync_ShouldRemoveEmptyDirectory()
    {
        var dirPath = GetTestPath("rm-dir");
        Directory.CreateDirectory(dirPath);

        fs.rmSync(dirPath);

        Assert.False(Directory.Exists(dirPath));
    }

    [Fact]
    public void rmSync_Recursive_ShouldRemoveDirectoryWithContents()
    {
        var dirPath = GetTestPath("rm-tree");
        Directory.CreateDirectory(dirPath);
        File.WriteAllText(Path.Combine(dirPath, "file.txt"), "content");
        Directory.CreateDirectory(Path.Combine(dirPath, "subdir"));

        fs.rmSync(dirPath, recursive: true);

        Assert.False(Directory.Exists(dirPath));
    }

    [Fact]
    public void rmSync_NonRecursive_DirectoryWithContents_ShouldThrow()
    {
        var dirPath = GetTestPath("rm-non-recursive");
        Directory.CreateDirectory(dirPath);
        File.WriteAllText(Path.Combine(dirPath, "file.txt"), "content");

        Assert.Throws<IOException>(() => fs.rmSync(dirPath, recursive: false));
    }

    [Fact]
    public void rmSync_NonExistent_ShouldNotThrow()
    {
        var filePath = GetTestPath("nonexistent-rm.txt");

        // rm doesn't throw if path doesn't exist (like Node.js with force: true)
        fs.rmSync(filePath);
    }
}
