using Xunit;

namespace Tsonic.CSharp.Node.Tests;

public class statTests : FsTestBase
{
    [Fact]
    public async Task stat_File_ShouldReturnFileStats()
    {
        var filePath = GetTestPath("stat-test-async.txt");
        var content = "Test content";
        File.WriteAllText(filePath, content);

        var stats = await fs.stat(filePath);

        Assert.True(stats.isFile);
        Assert.False(stats.isDirectory);
        Assert.True(stats.size >= content.Length);
        Assert.True(stats.IsFile());
        Assert.False(stats.IsDirectory());
    }

    [Fact]
    public async Task stat_Directory_ShouldReturnDirectoryStats()
    {
        var dirPath = GetTestPath("stat-dir-async");
        Directory.CreateDirectory(dirPath);

        var stats = await fs.stat(dirPath);

        Assert.False(stats.isFile);
        Assert.True(stats.isDirectory);
        Assert.Equal(0, stats.size);
        Assert.False(stats.IsFile());
        Assert.True(stats.IsDirectory());
    }

    [Fact]
    public async Task stat_NonExistent_ShouldThrow()
    {
        var filePath = GetTestPath("nonexistent-async.txt");
        await Assert.ThrowsAsync<FileNotFoundException>(async () => await fs.stat(filePath));
    }
}
