using System.Text;
using Tsonic.CSharp.Js;
using NativeSpawnOptions = Tsonic.CSharp.Node.SpawnSyncOptions<Tsonic.CSharp.Node.Buffer, Tsonic.CSharp.Runtime.Union<double, string>?[]>;
using JsSpawnOptions = Tsonic.CSharp.Node.SpawnSyncOptions<Tsonic.CSharp.Runtime.Union<Tsonic.CSharp.Js.Uint8Array, Tsonic.CSharp.Node.Buffer>?, Tsonic.CSharp.Js.JSArray<Tsonic.CSharp.Runtime.Union<double, string>?>>;

namespace Tsonic.CSharp.Node;

public static partial class child_process
{
    /// <summary>Runs a child with exact Buffer results and .NET-supported options.</summary>
    [System.Runtime.CompilerServices.OverloadResolutionPriority(1)]
    public static SpawnSyncResult spawnSyncResult(string command, string[] args, NativeSpawnOptions? options = null) =>
        BufferResult(RunSpawnSync(command, args, SpawnSettings.From(options)));

    /// <summary>Runs native arguments with JavaScript-profile option carriers.</summary>
    [System.Runtime.CompilerServices.OverloadResolutionPriority(1)]
    public static SpawnSyncResult spawnSyncResult(string command, string[] args, JsSpawnOptions options) =>
        BufferResult(RunSpawnSync(command, args, SpawnSettings.From(options)));

    /// <summary>Runs JavaScript-profile arguments and options without copying their storage.</summary>
    public static SpawnSyncResult spawnSyncResult(string command, JSArray<string> args, JsSpawnOptions? options = null) =>
        BufferResult(RunSpawnSync(command, args, SpawnSettings.From(options)));

    /// <summary>Runs JavaScript arguments with native-profile option carriers.</summary>
    public static SpawnSyncResult spawnSyncResult(string command, JSArray<string> args, NativeSpawnOptions options) =>
        BufferResult(RunSpawnSync(command, args, SpawnSettings.From(options)));

    private static SpawnSyncResult BufferResult(SpawnSyncReturns<byte[]> result)
    {
        return new SpawnSyncResult
        {
            stdout = result.stdout is null ? null : Buffer.TakeOwnership(result.stdout),
            stderr = result.stderr is null ? null : Buffer.TakeOwnership(result.stderr),
            status = result.status,
            pid = result.pid,
            signal = result.signal,
            error = result.error
        };
    }

    /// <summary>Runs a child and returns native captured byte arrays.</summary>
    public static SpawnSyncReturns<byte[]> spawnSync(string command, string[]? args = null, ExecOptions? options = null) =>
        RunSpawnSync(command, args ?? [], SpawnSettings.From(options));

    /// <summary>Runs a child and decodes captured output as UTF-8.</summary>
    public static SpawnSyncReturns<string> spawnSyncString(string command, string[]? args = null, ExecOptions? options = null)
    {
        var result = spawnSync(command, args, options);
        var stdout = result.stdout is null ? null : Encoding.UTF8.GetString(result.stdout);
        var stderr = result.stderr is null ? null : Encoding.UTF8.GetString(result.stderr);
        return new SpawnSyncReturns<string>
        {
            pid = result.pid, stdout = stdout, stderr = stderr,
            output = [null, stdout, stderr], status = result.status,
            signal = result.signal, error = result.error
        };
    }

    /// <summary>Runs an executable directly; nonzero exits and launch failures throw.</summary>
    public static object? execFileSync(string file, string[]? args = null, ExecOptions? options = null)
    {
        var result = spawnSync(file, args, options);
        if (result.error is not null) throw result.error;
        if (result.status != 0)
            throw new InvalidOperationException($"Command failed with exit code {result.status}: {file}\n" +
                $"stderr: {(result.stderr is null ? "" : Encoding.UTF8.GetString(result.stderr))}");
        return options?.encoding is null or "buffer" || result.stdout is null
            ? result.stdout : Encoding.UTF8.GetString(result.stdout);
    }
}
