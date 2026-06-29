using System.Linq;
using Xunit;

namespace Tsonic.CSharp.Node.Tests;

public class OsConstantsTests
{
    [Fact]
    public void Constants_ShouldExposeErrnoSignalsPriorityDlopenAndUv()
    {
        Assert.Equal(2, os.errnoConstant("ENOENT"));
        Assert.Equal(2, os.signalConstant("SIGINT"));
        Assert.Equal(0, os.priorityConstant("PRIORITY_NORMAL"));
        Assert.Equal(1, os.dlopenConstant("RTLD_LAZY"));
        Assert.Equal(4, os.uvConstant("UV_UDP_REUSEADDR"));

        Assert.True(os.constants.errno.ContainsKey("EACCES"));
        Assert.True(os.constants.signals.ContainsKey("SIGTERM"));
        Assert.Equal(-20, os.constants.priority.PRIORITY_HIGHEST);
    }

    [Fact]
    public void NetworkInterfaces_ShouldReturnClosedInterfaceRecords()
    {
        var interfaces = os.networkInterfaces();

        Assert.NotNull(interfaces);
        foreach (var entry in interfaces.Values.SelectMany(item => item))
        {
            Assert.False(string.IsNullOrWhiteSpace(entry.address));
            Assert.True(entry.family == "IPv4" || entry.family == "IPv6");
            Assert.NotNull(entry.mac);
        }
    }

    [Fact]
    public void UserInfoOptions_ShouldBeAccepted()
    {
        var info = os.userInfo(new UserInfoOptions { encoding = "utf8" });

        Assert.False(string.IsNullOrWhiteSpace(info.username));
    }
}
