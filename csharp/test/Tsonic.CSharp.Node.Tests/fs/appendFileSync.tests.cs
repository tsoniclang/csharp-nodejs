using Xunit;

namespace Tsonic.CSharp.Node.Tests;

public class appendFileSyncTests : FsTestBase
{
    [Fact]
    public void appendFileSync_ShouldAppendToFile()
    {
        var filePath = GetTestPath("append-test.txt");
        File.WriteAllText(filePath, "Line 1\n");

        fs.appendFileSync(filePath, "Line 2\n", "utf-8");

        var content = File.ReadAllText(filePath);
        Assert.Contains("Line 1", content);
        Assert.Contains("Line 2", content);
    }

    [Fact]
    public void appendFileSync_NonExistentFile_ShouldCreateFile()
    {
        var filePath = GetTestPath("append-new.txt");

        fs.appendFileSync(filePath, "New content", "utf-8");

        Assert.True(File.Exists(filePath));
        Assert.Equal("New content", File.ReadAllText(filePath));
    }
}
