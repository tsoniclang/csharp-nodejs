using Xunit;

namespace Tsonic.CSharp.Node.Tests;

public class truncateSyncTests : FsTestBase
{
    [Fact]
    public void truncateSync_ShouldTruncateFileToLength()
    {
        var filePath = GetTestPath("truncate-test.txt");
        File.WriteAllText(filePath, "Hello, World!");

        fs.truncateSync(filePath, 5);

        var content = File.ReadAllText(filePath);
        Assert.Equal(5, content.Length);
        Assert.Equal("Hello", content);
    }

    [Fact]
    public void truncateSync_ZeroLength_ShouldEmptyFile()
    {
        var filePath = GetTestPath("truncate-zero.txt");
        File.WriteAllText(filePath, "Content to remove");

        fs.truncateSync(filePath, 0);

        var content = File.ReadAllText(filePath);
        Assert.Empty(content);
    }

    [Fact]
    public void truncateSync_LongerLength_ShouldPadWithZeros()
    {
        var filePath = GetTestPath("truncate-extend.txt");
        File.WriteAllText(filePath, "Short");

        fs.truncateSync(filePath, 10);

        var fileInfo = new FileInfo(filePath);
        Assert.Equal(10, fileInfo.Length);
    }

    [Fact]
    public void truncateSync_NonExistentFile_ShouldThrow()
    {
        var filePath = GetTestPath("nonexistent-truncate.txt");

        Assert.Throws<FileNotFoundException>(() => fs.truncateSync(filePath, 0));
    }
}
