using Xunit;

namespace Tsonic.CSharp.Node.Tests;

public class realpathSyncTests : FsTestBase
{
    [Fact]
    public void realpathSync_ShouldResolveAbsolutePath()
    {
        var filePath = GetTestPath("realpath-test.txt");
        File.WriteAllText(filePath, "content");

        var resolved = fs.realpathSync(filePath);

        Assert.True(Path.IsPathFullyQualified(resolved));
        Assert.True(File.Exists(resolved));
    }

    [Fact]
    public void realpathSync_RelativePath_ShouldResolveToAbsolute()
    {
        var fileName = "realpath-relative.txt";
        var filePath = GetTestPath(fileName);
        File.WriteAllText(filePath, "content");

        // Change to test directory and use relative path
        var originalDir = Directory.GetCurrentDirectory();
        try
        {
            Directory.SetCurrentDirectory(_testDir);
            var resolved = fs.realpathSync(fileName);
            Assert.True(Path.IsPathFullyQualified(resolved));
            Assert.Equal(filePath, resolved);
        }
        finally
        {
            Directory.SetCurrentDirectory(originalDir);
        }
    }

    [Fact]
    public void realpathSync_NonExistent_ShouldResolveAnyway()
    {
        var filePath = GetTestPath("nonexistent-realpath.txt");

        // realpathSync resolves paths even if they don't exist
        var resolved = fs.realpathSync(filePath);
        Assert.True(Path.IsPathFullyQualified(resolved));
    }

    [Fact]
    public void realpathSync_Directory_ShouldResolve()
    {
        var dirPath = GetTestPath("realpath-dir");
        Directory.CreateDirectory(dirPath);

        var resolved = fs.realpathSync(dirPath);

        Assert.True(Path.IsPathFullyQualified(resolved));
        Assert.True(Directory.Exists(resolved));
    }
}
