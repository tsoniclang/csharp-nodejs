using Xunit;

namespace Tsonic.CSharp.Node.Tests;

public class unlinkTests : FsTestBase
{
    [Fact]
    public async Task unlink_ShouldDeleteFile()
    {
        var filePath = GetTestPath("delete-test-async.txt");
        File.WriteAllText(filePath, "content");

        await fs.unlink(filePath);

        Assert.False(File.Exists(filePath));
    }

    [Fact]
    public async Task unlink_NonExistentFile_ShouldNotThrow()
    {
        var filePath = GetTestPath("nonexistent-async-unlink.txt");
        await fs.unlink(filePath);
    }
}
