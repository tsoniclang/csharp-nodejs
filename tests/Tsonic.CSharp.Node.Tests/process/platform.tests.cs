using Xunit;
using System.Runtime.InteropServices;

namespace Tsonic.CSharp.Node.Tests;

public class platformTests
{
    [Fact]
    public void platform_ShouldReturnValidPlatformString()
    {
        var platform = process.platform;

        Assert.NotNull(platform);
        Assert.NotEmpty(platform);

        // Should be one of the known platforms
        var validPlatforms = new[] { "win32", "linux", "darwin", "freebsd", "openbsd", "sunos", "aix" };
        Assert.Contains(platform, validPlatforms, StringComparer.OrdinalIgnoreCase);
    }

    [Fact]
    public void platform_ShouldMatchOSPlatform()
    {
        var platform = process.platform;

        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            Assert.Equal("win32", platform);
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            Assert.Equal("linux", platform);
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            Assert.Equal("darwin", platform);
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.FreeBSD))
        {
            Assert.Equal("freebsd", platform);
        }
    }
}
