using Xunit;

namespace Tsonic.CSharp.Node.Tests;

public class writeFileBytesTests : FsTestBase
{
    [Fact]
    public async Task writeFileBytes_ShouldWriteBinaryData()
    {
        var filePath = GetTestPath("binary-write-async.bin");
        var data = new byte[] { 0x01, 0x02, 0x03, 0x04, 0x05 };

        await fs.writeFileBytes(filePath, data);

        Assert.True(File.Exists(filePath));
        Assert.Equal(data, File.ReadAllBytes(filePath));
    }
}
