using Xunit;

namespace Tsonic.CSharp.Node.Tests;

public class cpSyncTests : FsTestBase
{
    [Fact]
    public void cpSync_File_ShouldCopyFile()
    {
        var srcPath = GetTestPath("cp-src.txt");
        var destPath = GetTestPath("cp-dest.txt");
        var content = "Copy this content";
        File.WriteAllText(srcPath, content);

        fs.cpSync(srcPath, destPath);

        Assert.True(File.Exists(destPath));
        Assert.Equal(content, File.ReadAllText(destPath));
    }

    [Fact]
    public void cpSync_ShouldOverwriteDestination()
    {
        var srcPath = GetTestPath("cp-src2.txt");
        var destPath = GetTestPath("cp-dest2.txt");
        File.WriteAllText(srcPath, "New content");
        File.WriteAllText(destPath, "Old content");

        fs.cpSync(srcPath, destPath);

        Assert.Equal("New content", File.ReadAllText(destPath));
    }

    [Fact]
    public void cpSync_Directory_NonRecursive_ShouldThrow()
    {
        var srcPath = GetTestPath("cp-src-dir");
        var destPath = GetTestPath("cp-dest-dir");
        Directory.CreateDirectory(srcPath);
        File.WriteAllText(Path.Combine(srcPath, "file.txt"), "content");

        Assert.Throws<IOException>(() => fs.cpSync(srcPath, destPath, recursive: false));
    }

    [Fact]
    public void cpSync_Directory_Recursive_ShouldCopyAll()
    {
        var srcPath = GetTestPath("cp-src-tree");
        var destPath = GetTestPath("cp-dest-tree");
        Directory.CreateDirectory(srcPath);
        File.WriteAllText(Path.Combine(srcPath, "file1.txt"), "content1");
        var subdir = Path.Combine(srcPath, "subdir");
        Directory.CreateDirectory(subdir);
        File.WriteAllText(Path.Combine(subdir, "file2.txt"), "content2");

        fs.cpSync(srcPath, destPath, recursive: true);

        Assert.True(Directory.Exists(destPath));
        Assert.True(File.Exists(Path.Combine(destPath, "file1.txt")));
        Assert.True(Directory.Exists(Path.Combine(destPath, "subdir")));
        Assert.True(File.Exists(Path.Combine(destPath, "subdir", "file2.txt")));
        Assert.Equal("content1", File.ReadAllText(Path.Combine(destPath, "file1.txt")));
        Assert.Equal("content2", File.ReadAllText(Path.Combine(destPath, "subdir", "file2.txt")));
    }

    [Fact]
    public void cpSync_NonExistentSource_ShouldThrow()
    {
        var srcPath = GetTestPath("nonexistent-cp.txt");
        var destPath = GetTestPath("dest.txt");

        Assert.Throws<FileNotFoundException>(() => fs.cpSync(srcPath, destPath));
    }
}
