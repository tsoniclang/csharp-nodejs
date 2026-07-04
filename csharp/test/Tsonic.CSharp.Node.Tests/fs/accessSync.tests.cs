using Xunit;

namespace Tsonic.CSharp.Node.Tests;

public class accessSyncTests : FsTestBase
{
    [Fact]
    public void accessSync_ExistingFile_ShouldNotThrow()
    {
        var filePath = GetTestPath("access-test.txt");
        File.WriteAllText(filePath, "content");

        // Should not throw for existing file
        fs.accessSync(filePath, 0); // F_OK - check existence
    }

    [Fact]
    public void accessSync_ExistingDirectory_ShouldNotThrow()
    {
        var dirPath = GetTestPath("access-dir");
        Directory.CreateDirectory(dirPath);

        // Should not throw for existing directory
        fs.accessSync(dirPath, 0);
    }

    [Fact]
    public void accessSync_NonExistent_ShouldThrow()
    {
        var filePath = GetTestPath("nonexistent-access.txt");

        Assert.Throws<FileNotFoundException>(() => fs.accessSync(filePath, 0));
    }

    [Fact]
    public void accessSync_ReadableFile_ShouldNotThrow()
    {
        var filePath = GetTestPath("readable-test.txt");
        File.WriteAllText(filePath, "content");

        // Should not throw for readable file
        fs.accessSync(filePath, 4); // R_OK - check readability
    }
}
