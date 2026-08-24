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
    public void rmSync_RecursiveOptions_ShouldRemoveDirectoryWithContents()
    {
        var dirPath = GetTestPath("rm-tree");
        Directory.CreateDirectory(dirPath);
        File.WriteAllText(Path.Combine(dirPath, "file.txt"), "content");
        Directory.CreateDirectory(Path.Combine(dirPath, "subdir"));

        fs.rmSync(dirPath, new RmOptions { recursive = true });

        Assert.False(Directory.Exists(dirPath));
    }

    [Fact]
    public void rmSync_NonRecursiveOptions_DirectoryWithContents_ShouldThrow()
    {
        var dirPath = GetTestPath("rm-non-recursive");
        Directory.CreateDirectory(dirPath);
        File.WriteAllText(Path.Combine(dirPath, "file.txt"), "content");

        Assert.Throws<IOException>(() =>
            fs.rmSync(dirPath, new RmOptions { recursive = false }));
    }

    [Fact]
    public void rmSync_NonRecursiveOptions_EmptyDirectory_ShouldThrow()
    {
        var dirPath = GetTestPath("rm-empty-non-recursive");
        Directory.CreateDirectory(dirPath);

        Assert.Throws<IOException>(() => fs.rmSync(dirPath));
        Assert.True(Directory.Exists(dirPath));
    }

    [Fact]
    public void rmSync_MissingPath_RequiresExplicitForce()
    {
        var path = GetTestPath("nonexistent-rm.txt");

        Assert.Throws<FileNotFoundException>(() => fs.rmSync(path));
        fs.rmSync(path, new RmOptions { force = true });
    }

    [Fact]
    public void rmSync_InvalidRetryOptions_ShouldThrow()
    {
        var path = GetTestPath("invalid-retry-rm.txt");

        Assert.Throws<ArgumentOutOfRangeException>(() =>
            fs.rmSync(path, new RmOptions
            {
                force = true,
                maxRetries = 1.5
            }));
    }

    [Fact]
    public void rmSync_BrokenSymbolicLink_RemovesLinkWithoutFollowingTarget()
    {
        if (OperatingSystem.IsWindows()) return;
        var target = GetTestPath("missing-link-target");
        var link = GetTestPath("broken-link");
        File.CreateSymbolicLink(link, target);

        fs.rmSync(link);

        Assert.Throws<FileNotFoundException>(() => File.GetAttributes(link));
    }

    [Fact]
    public void rmSync_DirectorySymbolicLink_DoesNotRemoveTargetContents()
    {
        if (OperatingSystem.IsWindows()) return;
        var target = GetTestPath("link-target");
        var link = GetTestPath("directory-link");
        Directory.CreateDirectory(target);
        File.WriteAllText(Path.Combine(target, "retained.txt"), "content");
        Directory.CreateSymbolicLink(link, target);

        fs.rmSync(link, new RmOptions { recursive = true });

        Assert.True(File.Exists(Path.Combine(target, "retained.txt")));
        Assert.Throws<FileNotFoundException>(() => File.GetAttributes(link));
    }
}
