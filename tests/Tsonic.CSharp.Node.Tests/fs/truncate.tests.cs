using Xunit;

namespace Tsonic.CSharp.Node.Tests;

public class truncateTests : FsTestBase
{
    [Fact]
    public async Task truncate_ShouldTruncateFileToLength()
    {
        var filePath = GetTestPath("truncate-test-async.txt");
        File.WriteAllText(filePath, "Hello, World!");

        await fs.truncate(filePath, 5);

        var content = File.ReadAllText(filePath);
        Assert.Equal(5, content.Length);
        Assert.Equal("Hello", content);
    }

    [Fact]
    public async Task truncate_ZeroLength_ShouldEmptyFile()
    {
        var filePath = GetTestPath("truncate-zero-async.txt");
        File.WriteAllText(filePath, "Content to remove");

        await fs.truncate(filePath, 0);

        var content = File.ReadAllText(filePath);
        Assert.Empty(content);
    }

    [Fact]
    public async Task truncate_LongerLength_ShouldPadWithZeros()
    {
        var filePath = GetTestPath("truncate-extend-async.txt");
        File.WriteAllText(filePath, "Short");

        await fs.truncate(filePath, 10);

        var fileInfo = new FileInfo(filePath);
        Assert.Equal(10, fileInfo.Length);
    }

    [Fact]
    public async Task truncate_NonExistentFile_ShouldThrow()
    {
        var filePath = GetTestPath("nonexistent-truncate-async.txt");

        await Assert.ThrowsAsync<FileNotFoundException>(async () => await fs.truncate(filePath, 0));
    }
}
