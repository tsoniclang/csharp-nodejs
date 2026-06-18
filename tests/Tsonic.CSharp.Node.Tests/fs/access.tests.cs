using Xunit;

namespace Tsonic.CSharp.Node.Tests;

public class accessTests : FsTestBase
{
    [Fact]
    public async Task access_ExistingFile_ShouldNotThrow()
    {
        var filePath = GetTestPath("access-test-async.txt");
        File.WriteAllText(filePath, "content");

        // Should not throw for existing file
        await fs.access(filePath, 0);
    }

    [Fact]
    public async Task access_ExistingDirectory_ShouldNotThrow()
    {
        var dirPath = GetTestPath("access-dir-async");
        Directory.CreateDirectory(dirPath);

        // Should not throw for existing directory
        await fs.access(dirPath, 0);
    }

    [Fact]
    public async Task access_NonExistent_ShouldThrow()
    {
        var filePath = GetTestPath("nonexistent-access-async.txt");

        await Assert.ThrowsAsync<FileNotFoundException>(async () => await fs.access(filePath, 0));
    }

    [Fact]
    public async Task access_ReadableFile_ShouldNotThrow()
    {
        var filePath = GetTestPath("readable-test-async.txt");
        File.WriteAllText(filePath, "content");

        // Should not throw for readable file
        await fs.access(filePath, 4); // R_OK
    }
}
