using Xunit;

namespace Tsonic.CSharp.Node.Tests;

public class readFileBytesTests : FsTestBase
{
    [Fact]
    public async Task readFileBytes_ShouldReadBinaryData()
    {
        var filePath = GetTestPath("binary-test-async.bin");
        var data = new byte[] { 0x48, 0x65, 0x6C, 0x6C, 0x6F }; // "Hello" in ASCII
        File.WriteAllBytes(filePath, data);

        var result = await fs.readFileBytes(filePath);

        Assert.Equal(data, result);
    }
}
