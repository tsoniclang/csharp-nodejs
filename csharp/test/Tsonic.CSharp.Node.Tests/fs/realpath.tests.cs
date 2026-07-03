using Xunit;

namespace Tsonic.CSharp.Node.Tests;

public class realpathTests : FsTestBase
{
    [Fact]
    public async Task realpath_ShouldResolveAbsolutePath()
    {
        var filePath = GetTestPath("realpath-test-async.txt");
        File.WriteAllText(filePath, "content");

        var resolved = await fs.realpath(filePath);

        Assert.True(Path.IsPathFullyQualified(resolved));
        Assert.True(File.Exists(resolved));
    }

    [Fact]
    public async Task realpath_RelativePath_ShouldResolveToAbsolute()
    {
        var fileName = "realpath-relative-async.txt";
        var filePath = GetTestPath(fileName);
        File.WriteAllText(filePath, "content");

        // Change to test directory and use relative path
        var originalDir = Directory.GetCurrentDirectory();
        try
        {
            Directory.SetCurrentDirectory(_testDir);
            var resolved = await fs.realpath(fileName);
            Assert.True(Path.IsPathFullyQualified(resolved));
            Assert.Equal(filePath, resolved);
        }
        finally
        {
            Directory.SetCurrentDirectory(originalDir);
        }
    }

    [Fact]
    public async Task realpath_NonExistent_ShouldResolveAnyway()
    {
        var filePath = GetTestPath("nonexistent-realpath-async.txt");

        // realpath resolves paths even if they don't exist
        var resolved = await fs.realpath(filePath);
        Assert.True(Path.IsPathFullyQualified(resolved));
    }

    [Fact]
    public async Task realpath_Directory_ShouldResolve()
    {
        var dirPath = GetTestPath("realpath-dir-async");
        Directory.CreateDirectory(dirPath);

        var resolved = await fs.realpath(dirPath);

        Assert.True(Path.IsPathFullyQualified(resolved));
        Assert.True(Directory.Exists(resolved));
    }
}
