using Xunit;

namespace Tsonic.CSharp.Node.Tests;

public class writeFileSyncTests : FsTestBase
{
    [Fact]
    public void writeFileSync_ShouldCreateAndWriteFile()
    {
        var filePath = GetTestPath("write-test.txt");
        var content = "Test content";

        fs.writeFileSync(filePath, content, "utf-8");

        Assert.True(File.Exists(filePath));
        Assert.Equal(content, File.ReadAllText(filePath));
    }

    [Fact]
    public void writeFileSync_ShouldOverwriteExistingFile()
    {
        var filePath = GetTestPath("overwrite-test.txt");
        File.WriteAllText(filePath, "Original content");

        fs.writeFileSync(filePath, "New content", "utf-8");

        Assert.Equal("New content", File.ReadAllText(filePath));
    }
}
