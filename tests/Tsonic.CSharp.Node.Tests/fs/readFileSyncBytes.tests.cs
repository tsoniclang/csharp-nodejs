using Xunit;

namespace Tsonic.CSharp.Node.Tests;

public class readFileSyncBytesTests : FsTestBase
{
    [Fact]
    public void readFileSyncBytes_ShouldReadBinaryData()
    {
        var filePath = GetTestPath("binary-test.bin");
        var data = new byte[] { 0x48, 0x65, 0x6C, 0x6C, 0x6F }; // "Hello" in ASCII
        File.WriteAllBytes(filePath, data);

        var result = fs.readFileSyncBytes(filePath);

        Assert.Equal(data, result);
    }
}
