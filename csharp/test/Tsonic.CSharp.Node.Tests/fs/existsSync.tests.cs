using Xunit;

namespace Tsonic.CSharp.Node.Tests;

public class existsSyncTests : FsTestBase
{
    [Fact]
    public void existsSync_ExistingFile_ShouldReturnTrue()
    {
        var filePath = GetTestPath("exists-test.txt");
        File.WriteAllText(filePath, "content");

        Assert.True(fs.existsSync(filePath));
    }

    [Fact]
    public void existsSync_ExistingDirectory_ShouldReturnTrue()
    {
        var dirPath = GetTestPath("exists-dir");
        Directory.CreateDirectory(dirPath);

        Assert.True(fs.existsSync(dirPath));
    }

    [Fact]
    public void existsSync_NonExistent_ShouldReturnFalse()
    {
        var filePath = GetTestPath("nonexistent.txt");
        Assert.False(fs.existsSync(filePath));
    }
}
