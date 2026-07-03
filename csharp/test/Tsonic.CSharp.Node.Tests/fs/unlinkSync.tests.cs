using Xunit;

namespace Tsonic.CSharp.Node.Tests;

public class unlinkSyncTests : FsTestBase
{
    [Fact]
    public void unlinkSync_ShouldDeleteFile()
    {
        var filePath = GetTestPath("delete-test.txt");
        File.WriteAllText(filePath, "content");

        fs.unlinkSync(filePath);

        Assert.False(File.Exists(filePath));
    }

    [Fact]
    public void unlinkSync_NonExistentFile_ShouldNotThrow()
    {
        var filePath = GetTestPath("nonexistent.txt");
        // File.Delete doesn't throw if file doesn't exist
        fs.unlinkSync(filePath);
    }
}
