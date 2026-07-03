using Xunit;

namespace Tsonic.CSharp.Node.Tests;

public class writeFileSyncBytesTests : FsTestBase
{
    [Fact]
    public void writeFileSyncBytes_ShouldWriteBinaryData()
    {
        var filePath = GetTestPath("binary-write.bin");
        var data = new byte[] { 0x01, 0x02, 0x03, 0x04, 0x05 };

        fs.writeFileSyncBytes(filePath, data);

        Assert.True(File.Exists(filePath));
        Assert.Equal(data, File.ReadAllBytes(filePath));
    }
}
