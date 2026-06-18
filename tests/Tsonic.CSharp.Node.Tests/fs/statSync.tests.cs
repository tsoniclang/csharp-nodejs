using Xunit;

namespace Tsonic.CSharp.Node.Tests;

public class statSyncTests : FsTestBase
{
    [Fact]
    public void statSync_File_ShouldReturnFileStats()
    {
        var filePath = GetTestPath("stat-test.txt");
        var content = "Test content";
        File.WriteAllText(filePath, content);

        var stats = fs.statSync(filePath);

        Assert.True(stats.isFile);
        Assert.False(stats.isDirectory);
        Assert.True(stats.size >= content.Length);
        Assert.True(stats.IsFile());
        Assert.False(stats.IsDirectory());
    }

    [Fact]
    public void statSync_Directory_ShouldReturnDirectoryStats()
    {
        var dirPath = GetTestPath("stat-dir");
        Directory.CreateDirectory(dirPath);

        var stats = fs.statSync(dirPath);

        Assert.False(stats.isFile);
        Assert.True(stats.isDirectory);
        Assert.Equal(0, stats.size);
        Assert.False(stats.IsFile());
        Assert.True(stats.IsDirectory());
    }

    [Fact]
    public void statSync_NonExistent_ShouldThrow()
    {
        var filePath = GetTestPath("nonexistent.txt");
        Assert.Throws<FileNotFoundException>(() => fs.statSync(filePath));
    }

    [Fact]
    public void stats_Methods_ShouldWorkCorrectly()
    {
        var filePath = GetTestPath("stats-methods.txt");
        File.WriteAllText(filePath, "content");
        var stats = fs.statSync(filePath);

        // File checks
        Assert.True(stats.IsFile());
        Assert.False(stats.IsDirectory());
        Assert.False(stats.IsSymbolicLink());
        Assert.False(stats.IsBlockDevice());
        Assert.False(stats.IsCharacterDevice());
        Assert.False(stats.IsFIFO());
        Assert.False(stats.IsSocket());
    }

    [Fact]
    public void statSync_ShouldExposeUnixMillisecondTimestamps()
    {
        var filePath = GetTestPath("stats-time.txt");
        File.WriteAllText(filePath, "content");

        var stats = fs.statSync(filePath);

        Assert.True(stats.atimeMs > 0);
        Assert.True(stats.mtimeMs > 0);
        Assert.True(stats.ctimeMs > 0);
        Assert.True(stats.birthtimeMs > 0);
    }
}
