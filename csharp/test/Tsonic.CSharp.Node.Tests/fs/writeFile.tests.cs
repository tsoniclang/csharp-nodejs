using Xunit;

namespace Tsonic.CSharp.Node.Tests;

public class writeFileTests : FsTestBase
{
    [Fact]
    public async Task writeFile_ShouldCreateAndWriteFile()
    {
        var filePath = GetTestPath("write-test-async.txt");
        var content = "Test content async";

        await fs.writeFile(filePath, content, "utf-8");

        Assert.True(File.Exists(filePath));
        Assert.Equal(content, File.ReadAllText(filePath));
    }

    [Fact]
    public async Task writeFile_ShouldOverwriteExistingFile()
    {
        var filePath = GetTestPath("overwrite-test-async.txt");
        File.WriteAllText(filePath, "Original content");

        await fs.writeFile(filePath, "New content", "utf-8");

        Assert.Equal("New content", File.ReadAllText(filePath));
    }
}
