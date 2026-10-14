using System.Diagnostics;
using System.Reflection;
using System.Text;
using System.Text.Json;
using Xunit;

namespace Tsonic.CSharp.Node.Tests;

public sealed class WorkerBootstrapProcessTests
{
    [Fact]
    public async Task PublicNativeBootstrapWorksWithoutAnImplicitLoopAndRetiresOnlyCallbacks()
    {
        var relativePath = typeof(WorkerBootstrapProcessTests).Assembly
            .GetCustomAttributes<AssemblyMetadataAttribute>()
            .Single(attribute => attribute.Key == "WorkerBootstrapProbePath").Value;
        Assert.False(string.IsNullOrEmpty(relativePath));
        var probePath = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, relativePath));
        Assert.True(File.Exists(probePath), "ordinary native build produced bootstrap fixture");
        Assert.True(File.Exists(Path.ChangeExtension(probePath, ".runtimeconfig.json")), "probe runtime configuration");
        Assert.True(File.Exists(Path.ChangeExtension(probePath, ".deps.json")), "probe dependency manifest");
        var start = new ProcessStartInfo("dotnet")
        {
            UseShellExecute = false,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            WorkingDirectory = Path.GetDirectoryName(probePath),
        };
        start.ArgumentList.Add(probePath);
        using var child = new System.Diagnostics.Process { StartInfo = start };
        Assert.True(child.Start(), "native bootstrap fixture started");
        using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(30));
        try
        {
            var output = ReadBounded(child.StandardOutput, child, timeout.Token);
            var error = ReadBounded(child.StandardError, child, timeout.Token);
            await child.WaitForExitAsync(timeout.Token);
            var standardOutput = await output;
            var standardError = await error;
            Assert.True(child.ExitCode == 0, $"native bootstrap exit {child.ExitCode}: {standardError}");
            Assert.Equal(string.Empty, standardError);
            using var result = JsonDocument.Parse(standardOutput);
            Assert.Equal(5, result.RootElement.EnumerateObject().Count());
            Assert.True(result.RootElement.GetProperty("public_native_bootstrap").GetBoolean());
            Assert.True(result.RootElement.GetProperty("exact_int64").GetBoolean());
            Assert.True(result.RootElement.GetProperty("preloop_transport").GetBoolean());
            Assert.True(result.RootElement.GetProperty("retained_transport").GetBoolean());
            Assert.True(result.RootElement.GetProperty("fresh_epoch").GetBoolean());
        }
        finally
        {
            if (!child.HasExited)
            {
                try { child.Kill(entireProcessTree: true); }
                catch (InvalidOperationException) when (child.HasExited) { }
                await child.WaitForExitAsync().WaitAsync(TimeSpan.FromSeconds(5));
            }
        }
    }

    private static async Task<string> ReadBounded(StreamReader reader, System.Diagnostics.Process child,
        CancellationToken cancellation)
    {
        var collected = new StringBuilder();
        var buffer = new char[1024];
        while (true)
        {
            var count = await reader.ReadAsync(buffer.AsMemory(), cancellation);
            if (count == 0) return collected.ToString();
            if (collected.Length + count > 16384)
            {
                if (!child.HasExited)
                {
                    try { child.Kill(entireProcessTree: true); }
                    catch (InvalidOperationException) when (child.HasExited) { }
                }
                throw new InvalidOperationException("Native bootstrap fixture exceeded its finite output limit.");
            }
            collected.Append(buffer, 0, count);
        }
    }
}
