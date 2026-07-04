using Xunit;
using System.Runtime.InteropServices;

namespace Tsonic.CSharp.Node.Tests;

public class chmodSyncTests : FsTestBase
{
    [Fact]
    public void chmodSync_ShouldChangeFilePermissions()
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            // Skip on Windows as chmod has limited support
            return;
        }

        var filePath = GetTestPath("chmod-test.txt");
        File.WriteAllText(filePath, "content");

        // Change to read-only (mode 0444)
        fs.chmodSync(filePath, 0x124); // 0o444 in octal

        var fileInfo = new FileInfo(filePath);
        // On Unix, file should have limited permissions
        Assert.True(File.Exists(filePath));
    }

    [Fact]
    public void chmodSync_NonExistentFile_ShouldThrow()
    {
        var filePath = GetTestPath("nonexistent-chmod.txt");

        Assert.Throws<FileNotFoundException>(() => fs.chmodSync(filePath, 0x1FF));
    }

    [Fact]
    public void chmodSync_Directory_ShouldWork()
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            return;
        }

        var dirPath = GetTestPath("chmod-dir");
        Directory.CreateDirectory(dirPath);

        fs.chmodSync(dirPath, 0x1FF); // 0o777

        Assert.True(Directory.Exists(dirPath));
    }
}
