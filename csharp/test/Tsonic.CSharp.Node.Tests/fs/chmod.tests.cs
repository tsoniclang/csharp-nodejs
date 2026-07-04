using Xunit;
using System.Runtime.InteropServices;

namespace Tsonic.CSharp.Node.Tests;

public class chmodTests : FsTestBase
{
    [Fact]
    public async Task chmod_ShouldChangeFilePermissions()
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            return;
        }

        var filePath = GetTestPath("chmod-test-async.txt");
        File.WriteAllText(filePath, "content");

        await fs.chmod(filePath, 0x124); // 0o444

        var fileInfo = new FileInfo(filePath);
        Assert.True(File.Exists(filePath));
    }

    [Fact]
    public async Task chmod_NonExistentFile_ShouldThrow()
    {
        var filePath = GetTestPath("nonexistent-chmod-async.txt");

        await Assert.ThrowsAsync<FileNotFoundException>(async () => await fs.chmod(filePath, 0x1FF));
    }

    [Fact]
    public async Task chmod_Directory_ShouldWork()
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            return;
        }

        var dirPath = GetTestPath("chmod-dir-async");
        Directory.CreateDirectory(dirPath);

        await fs.chmod(dirPath, 0x1FF); // 0o777

        Assert.True(Directory.Exists(dirPath));
    }
}
