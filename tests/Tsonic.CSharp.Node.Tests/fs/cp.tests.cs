using Xunit;

namespace Tsonic.CSharp.Node.Tests;

public class cpTests : FsTestBase
{
    [Fact]
    public async Task cp_File_ShouldCopyFile()
    {
        var srcPath = GetTestPath("cp-src-async.txt");
        var destPath = GetTestPath("cp-dest-async.txt");
        var content = "Copy this content";
        File.WriteAllText(srcPath, content);

        await fs.cp(srcPath, destPath);

        Assert.True(File.Exists(destPath));
        Assert.Equal(content, File.ReadAllText(destPath));
    }

    [Fact]
    public async Task cp_ShouldOverwriteDestination()
    {
        var srcPath = GetTestPath("cp-src2-async.txt");
        var destPath = GetTestPath("cp-dest2-async.txt");
        File.WriteAllText(srcPath, "New content");
        File.WriteAllText(destPath, "Old content");

        await fs.cp(srcPath, destPath);

        Assert.Equal("New content", File.ReadAllText(destPath));
    }

    [Fact]
    public async Task cp_Directory_NonRecursive_ShouldThrow()
    {
        var srcPath = GetTestPath("cp-src-dir-async");
        var destPath = GetTestPath("cp-dest-dir-async");
        Directory.CreateDirectory(srcPath);
        File.WriteAllText(Path.Combine(srcPath, "file.txt"), "content");

        await Assert.ThrowsAsync<IOException>(async () => await fs.cp(srcPath, destPath, recursive: false));
    }

    [Fact]
    public async Task cp_Directory_Recursive_ShouldCopyAll()
    {
        var srcPath = GetTestPath("cp-src-tree-async");
        var destPath = GetTestPath("cp-dest-tree-async");
        Directory.CreateDirectory(srcPath);
        File.WriteAllText(Path.Combine(srcPath, "file1.txt"), "content1");
        var subdir = Path.Combine(srcPath, "subdir");
        Directory.CreateDirectory(subdir);
        File.WriteAllText(Path.Combine(subdir, "file2.txt"), "content2");

        await fs.cp(srcPath, destPath, recursive: true);

        Assert.True(Directory.Exists(destPath));
        Assert.True(File.Exists(Path.Combine(destPath, "file1.txt")));
        Assert.True(Directory.Exists(Path.Combine(destPath, "subdir")));
        Assert.True(File.Exists(Path.Combine(destPath, "subdir", "file2.txt")));
        Assert.Equal("content1", File.ReadAllText(Path.Combine(destPath, "file1.txt")));
        Assert.Equal("content2", File.ReadAllText(Path.Combine(destPath, "subdir", "file2.txt")));
    }

    [Fact]
    public async Task cp_NonExistentSource_ShouldThrow()
    {
        var srcPath = GetTestPath("nonexistent-cp-async.txt");
        var destPath = GetTestPath("dest-async.txt");

        await Assert.ThrowsAsync<FileNotFoundException>(async () => await fs.cp(srcPath, destPath));
    }
}
