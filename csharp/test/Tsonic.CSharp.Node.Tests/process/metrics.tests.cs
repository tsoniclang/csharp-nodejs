using System.IO;
using System.Threading.Tasks;
using Xunit;

namespace Tsonic.CSharp.Node.Tests;

[Collection(JsEventLoopCollection.Name)]
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
    public void ProcessHrtime_PreservesNativeIntegerPrecisionAndBounds()
    {
        long[] before = process.hrtime();
        const long previous = 9_007_199_254_740_993;
        long[] delta = process.hrtime([previous, 0]);
        long[] after = process.hrtime();
        Assert.InRange(delta[0], before[0] - previous, after[0] - previous);
        Assert.InRange(delta[1], 0, 999_999_999);
        Assert.Throws<ArgumentOutOfRangeException>(() => process.hrtime([]));
        Assert.Throws<ArgumentOutOfRangeException>(() => process.hrtime([0, -1]));
        Assert.Throws<ArgumentOutOfRangeException>(() => process.hrtime([0, 1_000_000_000]));
        Assert.Throws<OverflowException>(() => process.hrtime([long.MinValue, 0]));
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
        int owner = Environment.CurrentManagedThreadId;
        int observed = 0;
        Tsonic.CSharp.Js.JsEventLoop.Run(() =>
        {
            process.nextTick(() =>
            {
                observed = Environment.CurrentManagedThreadId;
                tcs.SetResult();
            });
            Assert.False(tcs.Task.IsCompleted);
        });
        await tcs.Task.WaitAsync(System.TimeSpan.FromSeconds(2));
        Assert.Equal(owner, observed);
    }

    [Fact]
    public void ProcessNextTick_PreservesCallbackOrderAndFailureIdentity()
    {
        var observed = new System.Collections.Generic.List<int>();
        var failure = new InvalidOperationException("owned next tick");
        process.nextTick(() => observed.Add(1));
        process.nextTick(() => throw failure);
        process.nextTick(() => observed.Add(3));
        Assert.Same(failure, Assert.Throws<InvalidOperationException>(Tsonic.CSharp.Js.JsEventLoop.Run));
        Assert.True(Tsonic.CSharp.Js.ProcessKeepAlive.HasReferences);
        Tsonic.CSharp.Js.JsEventLoop.Run();
        Assert.Equal([1, 3], observed);
        Assert.False(Tsonic.CSharp.Js.ProcessKeepAlive.HasReferences);
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
