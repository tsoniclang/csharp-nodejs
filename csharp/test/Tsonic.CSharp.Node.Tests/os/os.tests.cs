using Xunit;
using System.Runtime.InteropServices;

namespace Tsonic.CSharp.Node.Tests;

public class osTests
{
    [Fact]
    public void platform_ShouldReturnValidPlatform()
    {
        var platform = os.platform();
        Assert.NotNull(platform);
        Assert.NotEmpty(platform);

        var validPlatforms = new[] { "win32", "linux", "darwin", "freebsd" };
        Assert.Contains(platform, validPlatforms);
    }

    [Fact]
    public void arch_ShouldReturnValidArchitecture()
    {
        var arch = os.arch();
        Assert.NotNull(arch);
        Assert.NotEmpty(arch);

        var validArchs = new[] { "x64", "ia32", "arm", "arm64", "wasm", "s390x" };
        Assert.Contains(arch, validArchs);
    }

    [Fact]
    public void hostname_ShouldReturnNonEmptyString()
    {
        var hostname = os.hostname();
        Assert.NotNull(hostname);
        Assert.NotEmpty(hostname);
    }

    [Fact]
    public void tmpdir_ShouldReturnValidPath()
    {
        var tmpdir = os.tmpdir();
        Assert.NotNull(tmpdir);
        Assert.NotEmpty(tmpdir);
        Assert.True(Directory.Exists(tmpdir));
    }

    [Fact]
    public void homedir_ShouldReturnValidPath()
    {
        var homedir = os.homedir();
        Assert.NotNull(homedir);
        Assert.NotEmpty(homedir);
        Assert.True(Directory.Exists(homedir));
    }

    [Fact]
    public void EOL_ShouldBeCorrectForPlatform()
    {
        var eol = os.EOL;
        Assert.NotNull(eol);

        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            Assert.Equal("\r\n", eol);
        }
        else
        {
            Assert.Equal("\n", eol);
        }
    }

    [Fact]
    public void devNull_ShouldBeCorrectForPlatform()
    {
        var devNull = os.devNull;
        Assert.NotNull(devNull);

        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            Assert.Equal("\\\\.\\nul", devNull);
        }
        else
        {
            Assert.Equal("/dev/null", devNull);
        }
    }

    [Fact]
    public void type_ShouldReturnValidType()
    {
        var type = os.type();
        Assert.NotNull(type);
        Assert.NotEmpty(type);

        var validTypes = new[] { "Windows_NT", "Linux", "Darwin", "FreeBSD" };
        Assert.Contains(type, validTypes);
    }

    [Fact]
    public void release_ShouldReturnNonEmptyString()
    {
        var release = os.release();
        Assert.NotNull(release);
        Assert.NotEmpty(release);
    }

    [Fact]
    public void endianness_ShouldReturnBEorLE()
    {
        var endianness = os.endianness();
        Assert.True(endianness == "BE" || endianness == "LE");
    }

    [Fact]
    public void totalmem_ShouldReturnPositiveValue()
    {
        var totalmem = os.totalmem();
        Assert.True(totalmem > 0);
    }

    [Fact]
    public void freemem_ShouldReturnPositiveValue()
    {
        var freemem = os.freemem();
        Assert.True(freemem > 0);
    }

    [Fact]
    public void uptime_ShouldReturnPositiveValue()
    {
        var uptime = os.uptime();
        Assert.True(uptime >= 0);
    }

    [Fact]
    public void loadavg_ShouldReturnArrayOfThree()
    {
        var loadavg = os.loadavg();
        Assert.NotNull(loadavg);
        Assert.Equal(3, loadavg.Length);
    }

    [Fact]
    public void cpus_ShouldReturnNonEmptyArray()
    {
        var cpus = os.cpus();
        Assert.NotNull(cpus);
        Assert.True(cpus.Length > 0);

        foreach (var cpu in cpus)
        {
            Assert.NotNull(cpu);
            Assert.NotNull(cpu.model);
            Assert.NotNull(cpu.times);
        }
    }

    [Fact]
    public void availableParallelism_ShouldReturnPositiveValue()
    {
        var parallelism = os.availableParallelism();
        Assert.True(parallelism > 0);
    }

    [Fact]
    public void availableParallelism_ShouldMatchProcessorCount()
    {
        var parallelism = os.availableParallelism();
        Assert.Equal(Environment.ProcessorCount, parallelism);
    }

    [Fact]
    public void userInfo_ShouldReturnValidInfo()
    {
        var userInfo = os.userInfo();
        Assert.NotNull(userInfo);
        Assert.NotNull(userInfo.username);
        Assert.NotEmpty(userInfo.username);
        Assert.NotNull(userInfo.homedir);
        Assert.NotEmpty(userInfo.homedir);
    }

    [Fact]
    public void userInfo_ShouldHaveCorrectPlatformSpecificFields()
    {
        var userInfo = os.userInfo();

        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            Assert.Equal(-1, userInfo.uid);
            Assert.Equal(-1, userInfo.gid);
            Assert.Null(userInfo.shell);
        }
        else
        {
            Assert.True(userInfo.uid >= 0);
            Assert.True(userInfo.gid >= 0);
        }
    }
}
