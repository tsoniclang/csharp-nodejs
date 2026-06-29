using System.IO;
using System.Threading.Tasks;
using Xunit;

namespace Tsonic.CSharp.Node.Tests;

public class ProcessMetricsTests : FsTestBase
{
    [Fact]
    public void ProcessMetrics_ReturnRuntimeValues()
    {
        var memory = process.memoryUsage();
        var cpu = process.cpuUsage();
        var resource = process.resourceUsage();

        Assert.True(memory.rss > 0);
        Assert.True(memory.heapUsed >= 0);
        Assert.True(cpu.user >= 0);
        Assert.True(cpu.system >= 0);
        Assert.True(resource.maxRSS > 0);
    }

    [Fact]
    public void ProcessHrtime_ReturnsTupleAndBigint()
    {
        var value = process.hrtime();
        var bigint = process.hrtime_bigint();

        Assert.Equal(2, value.Length);
        Assert.True(bigint >= 0);
    }

    [Fact]
    public void ProcessFeaturesAndConfig_AreClosedObjects()
    {
        Assert.True(process.features.tls);
        Assert.True(process.features.typescript);
        Assert.Equal(process.arch, process.config.hostArch);
        Assert.Equal("tsonic", process.release.name);
    }

    [Fact]
    public void ProcessWarnings_AreRecorded()
    {
        var before = process.warnings.Count;

        process.emitWarning("careful", new EmitWarningOptions { type = "CustomWarning", code = "TSTEST" });

        Assert.True(process.warnings.Count > before);
        Assert.Contains(process.warnings, warning => warning.code == "TSTEST");
    }

    [Fact]
    public void ProcessEnvFile_LoadsValues()
    {
        var path = GetTestPath(".env");
        File.WriteAllText(path, "TSONIC_ENV_FILE_TEST=ok\n# ignored\n");

        process.loadEnvFile(path);

        Assert.Equal("ok", process.env["TSONIC_ENV_FILE_TEST"]);
    }

    [Fact]
    public async Task ProcessNextTick_RunsCallback()
    {
        var tcs = new TaskCompletionSource();

        process.nextTick(() => tcs.SetResult());

        await tcs.Task.WaitAsync(System.TimeSpan.FromSeconds(2));
    }

    [Fact]
    public void ProcessUncaughtCapture_CanBeSetAndCleared()
    {
        process.setUncaughtExceptionCaptureCallback(_ => { });
        Assert.True(process.hasUncaughtExceptionCaptureCallback());

        process.setUncaughtExceptionCaptureCallback(null);
        Assert.False(process.hasUncaughtExceptionCaptureCallback());
    }
}
