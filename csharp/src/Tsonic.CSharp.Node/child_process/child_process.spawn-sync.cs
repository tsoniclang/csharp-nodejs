using System.Text;
using Tsonic.CSharp.Js;
using NativeSpawnOptions = Tsonic.CSharp.Node.SpawnSyncOptions<Tsonic.CSharp.Node.Buffer, Tsonic.CSharp.Runtime.Union<double, string>?[]>;
using JsSpawnOptions = Tsonic.CSharp.Node.SpawnSyncOptions<Tsonic.CSharp.Runtime.Union<Tsonic.CSharp.Js.Uint8Array, Tsonic.CSharp.Node.Buffer>?, Tsonic.CSharp.Js.JSArray<Tsonic.CSharp.Runtime.Union<double, string>?>>;

namespace Tsonic.CSharp.Node;

public static partial class child_process
{
    /// <summary>Runs a child with exact Buffer results and .NET-supported options.</summary>
    [System.Runtime.CompilerServices.OverloadResolutionPriority(1)]
    public static SpawnSyncReturns<Buffer> spawnSyncResult(string command, string[] args, NativeSpawnOptions? options = null) =>
        BufferResult(RunSpawnSync(command, args, SpawnSettings.From(options)));

    /// <summary>Runs native arguments with JavaScript-profile option carriers.</summary>
    [System.Runtime.CompilerServices.OverloadResolutionPriority(1)]
    public static SpawnSyncReturns<Buffer> spawnSyncResult(string command, string[] args, JsSpawnOptions options) =>
        BufferResult(RunSpawnSync(command, args, SpawnSettings.From(options)));

    /// <summary>Runs JavaScript-profile arguments and options without copying their storage.</summary>
    public static SpawnSyncReturns<Buffer> spawnSyncResult(string command, JSArray<string> args, JsSpawnOptions? options = null) =>
        BufferResult(RunSpawnSync(command, args, SpawnSettings.From(options)));

    /// <summary>Runs JavaScript arguments with native-profile option carriers.</summary>
    public static SpawnSyncReturns<Buffer> spawnSyncResult(string command, JSArray<string> args, NativeSpawnOptions options) =>
        BufferResult(RunSpawnSync(command, args, SpawnSettings.From(options)));

    private static SpawnSyncReturns<Buffer> BufferResult(SpawnOutcome result)
    {
        var stdout = result.Stdout is null ? null : Buffer.TakeOwnership(result.Stdout);
        var stderr = result.Stderr is null ? null : Buffer.TakeOwnership(result.Stderr);
        return new SpawnSyncReturns<Buffer>
        {
            stdout = stdout, stderr = stderr, output = [null, stdout, stderr],
            status = result.Status, pid = result.Pid, error = result.Error
        };
    }

    /// <summary>Runs a child and returns native captured byte arrays.</summary>
    public static SpawnSyncReturns<byte[]> spawnSync(string command, string[]? args = null, ExecOptions? options = null)
    {
        var result = RunSpawnSync(command, args ?? [], SpawnSettings.From(options));
        return new SpawnSyncReturns<byte[]>
        {
            pid = result.Pid, stdout = result.Stdout, stderr = result.Stderr,
            output = [null, result.Stdout, result.Stderr], status = result.Status, error = result.Error
        };
    }

    /// <summary>Runs a child and decodes captured output as UTF-8.</summary>
    public static SpawnSyncReturns<string> spawnSyncString(string command, string[]? args = null, ExecOptions? options = null)
    {
        var result = RunSpawnSync(command, args ?? [], SpawnSettings.From(options));
        var stdout = result.Stdout is null ? null : Encoding.UTF8.GetString(result.Stdout);
        var stderr = result.Stderr is null ? null : Encoding.UTF8.GetString(result.Stderr);
        return new SpawnSyncReturns<string>
        {
            pid = result.Pid, stdout = stdout, stderr = stderr,
            output = [null, stdout, stderr], status = result.Status, error = result.Error
        };
    }

    /// <summary>Runs an executable directly; nonzero exits and launch failures throw.</summary>
    public static object? execFileSync(string file, string[]? args = null, ExecOptions? options = null)
    {
        var result = RunSpawnSync(file, args ?? [], SpawnSettings.From(options));
        if (result.Error is not null) throw result.Error;
        if (result.Status != 0)
            throw new InvalidOperationException($"Command failed with exit code {result.Status}: {file}\n" +
                $"stderr: {(result.Stderr is null ? "" : Encoding.UTF8.GetString(result.Stderr))}");
        return options?.encoding is null or "buffer" || result.Stdout is null
            ? result.Stdout : Encoding.UTF8.GetString(result.Stdout);
    }
}
