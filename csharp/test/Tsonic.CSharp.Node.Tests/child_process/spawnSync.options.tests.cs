using Tsonic.CSharp.Js;
using Tsonic.CSharp.Runtime;
using Xunit;
using SpawnSyncOptions = Tsonic.CSharp.Node.SpawnSyncOptions<Tsonic.CSharp.Runtime.Union<Tsonic.CSharp.Js.Uint8Array, Tsonic.CSharp.Node.Buffer>?, Tsonic.CSharp.Js.JSArray<Tsonic.CSharp.Runtime.Union<double, string>?>>;

namespace Tsonic.CSharp.Node.Tests;

public class ChildProcessSpawnOptionsTests
{
    [Fact]
    public void BinaryInputRetainsViewAndStdioAliases()
    {
        var backing = new Uint8Array(new double[] { 9, 0, 255, 42, 9 });
        var view = new Uint8Array(backing.buffer, 1, 3);
        var descriptors = new JSArray<Union<double, string>?>(new Union<double, string>?[] { "pipe", "ignore", "pipe" });
        var options = new SpawnSyncOptions { encoding = "buffer", input = view, stdio = descriptors, maxBuffer = 4096 };
        descriptors[1] = "pipe";
        backing[3] = 43;
        var command = OperatingSystem.IsWindows() ? "cmd" : "/bin/cat";
        var args = OperatingSystem.IsWindows() ? new[] { "/c", "more" } : System.Array.Empty<string>();
        if (OperatingSystem.IsWindows())
        {
            options.input = Buffer.from("binary input\r\n");
            var text = child_process.spawnSyncResult(command, args, options);
            Assert.Equal(0, text.status);
            Assert.Contains("binary input", Assert.IsType<Buffer>(text.stdout).toString());
        }
        else
        {
            var result = child_process.spawnSyncResult(command, args, options);
            Assert.Equal(0, result.status);
            Assert.True(result.pid > 0);
            Assert.Null(result.error);
            Assert.Null(result.signal);
            var output = Assert.IsType<Buffer>(result.stdout);
            Assert.Equal(3, output.length);
            Assert.Equal(0, output[0]);
            Assert.Equal(255, output[1]);
            Assert.Equal(43, output[2]);
            Assert.Equal(0, Assert.IsType<Buffer>(result.stderr).length);
        }
    }

    [Fact]
    public void PlainEnvironmentDoesNotReadOrWriteParentState()
    {
        var key = "TSONIC_CHILD_" + Guid.NewGuid().ToString("N");
        var environment = new ProcessEnv();
        Assert.Empty(environment);
        environment[key] = "child-only";
        Assert.Null(Environment.GetEnvironmentVariable(key));
        var options = new SpawnSyncOptions { env = environment, cwd = Path.GetTempPath() };
        var command = OperatingSystem.IsWindows() ? "cmd" : "/bin/sh";
        var args = OperatingSystem.IsWindows()
            ? new[] { "/c", $"echo %{key}%" }
            : new[] { "-c", $"printf '%s' \"${key}\"" };
        var result = child_process.spawnSyncResult(command, args, options);
        Assert.Equal(0, result.status);
        Assert.Equal("child-only", Assert.IsType<Buffer>(result.stdout).toString().Trim());
        environment.Clear();
        Assert.Null(Environment.GetEnvironmentVariable(key));
    }

    [Fact]
    public void BufferBoundMustBeNonnegative()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => child_process.spawnSyncResult(
            "must-not-be-started", System.Array.Empty<string>(), new SpawnSyncOptions { maxBuffer = -1 }));
    }

    [Fact]
    public void NativeLimitationsAreRejectedBeforeStartingAChild()
    {
        var unsupported = new SpawnSyncOptions[]
        {
            new() { uid = 1 }, new() { gid = 1 }, new() { timeout = 1 }, new() { killSignal = "SIGTERM" },
            new() { stdio = new JSArray<Union<double, string>?>(new Union<double, string>?[] { "pipe", "ignore", "pipe" }) },
            new() { stdio = new JSArray<Union<double, string>?>(new Union<double, string>?[] { 4, "pipe", "pipe" }) },
            new() { stdio = new JSArray<Union<double, string>?>(new Union<double, string>?[] { "pipe", "pipe", "pipe", "pipe" }) }
        };
        foreach (var options in unsupported)
            Assert.Throws<PlatformNotSupportedException>(() => child_process.spawnSyncResult("must-not-be-started", System.Array.Empty<string>(), options));
    }

    [Fact]
    public void LaunchStringsCannotBeSilentlyTruncatedAtTheNativeBoundary()
    {
        Assert.Throws<ArgumentException>(() => child_process.spawnSyncResult("bad\0command", System.Array.Empty<string>()));
        Assert.Throws<ArgumentException>(() => child_process.spawnSyncResult("unused", new[] { "bad\0argument" }));
        Assert.Throws<ArgumentException>(() => child_process.spawnSyncResult("unused", System.Array.Empty<string>(), new SpawnSyncOptions { cwd = "bad\0path" }));
        foreach (var pair in new[] { ("BAD=KEY", "value"), ("BAD\0KEY", "value"), ("KEY", "bad\0value"), ("", "value") })
        {
            var environment = new ProcessEnv { [pair.Item1] = pair.Item2 };
            Assert.Throws<ArgumentException>(() => child_process.spawnSyncResult("unused", System.Array.Empty<string>(), new SpawnSyncOptions { env = environment }));
        }
    }

    [Fact]
    public void EmptyInputClosesThePipeAndNonzeroExitIsNotALaunchError()
    {
        var command = OperatingSystem.IsWindows() ? "cmd" : "/bin/sh";
        var args = OperatingSystem.IsWindows() ? new[] { "/c", "exit 7" } : new[] { "-c", "cat >/dev/null; exit 7" };
        var result = child_process.spawnSyncResult(command, args);
        Assert.Equal(7, result.status);
        Assert.Null(result.error);
        Assert.Null(result.signal);
        Assert.Equal(0, Assert.IsType<Buffer>(result.stdout).length);
    }

    [Fact]
    public void BufferLimitStopsTheChildWithoutInventingASignalResult()
    {
        var command = OperatingSystem.IsWindows() ? "cmd" : "/bin/sh";
        var args = OperatingSystem.IsWindows() ? new[] { "/c", "echo overflow" } : new[] { "-c", "printf overflow" };
        var error = Assert.Throws<PlatformNotSupportedException>(() => child_process.spawnSyncResult(
            command, args, new SpawnSyncOptions { maxBuffer = 2 }));
        Assert.Contains("maxBuffer", error.Message);
    }

    [Fact]
    public void AmbiguousUnixExitStatusCannotBeClaimedAsOrdinaryOrSignalled()
    {
        if (OperatingSystem.IsWindows())
        {
            Assert.Equal(200, child_process.spawnSyncResult("cmd", ["/c", "exit 200"]).status);
        }
        else
        {
            Assert.Throws<PlatformNotSupportedException>(() => child_process.spawnSyncResult("/bin/sh", ["-c", "exit 200"]));
            Assert.Throws<PlatformNotSupportedException>(() => child_process.spawnSyncResult("/bin/sh", ["-c", "kill -TERM $$"]));
        }
    }
}
