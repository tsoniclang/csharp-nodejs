using System.Text;
using Tsonic.CSharp.Runtime;
using NativeSpawnOptions = Tsonic.CSharp.Node.SpawnSyncOptions<Tsonic.CSharp.Node.Buffer, Tsonic.CSharp.Runtime.Union<double, string>?[]>;
using JsSpawnOptions = Tsonic.CSharp.Node.SpawnSyncOptions<Tsonic.CSharp.Runtime.Union<Tsonic.CSharp.Js.Uint8Array, Tsonic.CSharp.Node.Buffer>?, Tsonic.CSharp.Js.JSArray<Tsonic.CSharp.Runtime.Union<double, string>?>>;

namespace Tsonic.CSharp.Node;

public static partial class child_process
{
    private sealed record SpawnSettings(
        string? Directory,
        IReadOnlyDictionary<string, string?>? Environment,
        ReadOnlyMemory<byte> Input,
        int MaximumBuffer,
        bool StdinPipe,
        bool StdoutPipe,
        bool StderrPipe,
        bool HideWindow)
    {
        public static SpawnSettings From(NativeSpawnOptions? options)
        {
            var descriptors = options?.stdio;
            return From(options, options?.input?.InternalMemory ?? ReadOnlyMemory<byte>.Empty, options?.input is not null,
                descriptors?.Length ?? 0,
                descriptors is { Length: > 0 } ? descriptors[0] : (Union<double, string>?)null,
                descriptors is { Length: > 1 } ? descriptors[1] : (Union<double, string>?)null,
                descriptors is { Length: > 2 } ? descriptors[2] : (Union<double, string>?)null);
        }

        public static SpawnSettings From(JsSpawnOptions? options)
        {
            var input = options?.input;
            var bytes = input is null ? ReadOnlyMemory<byte>.Empty
                : input.Value.Is1() ? input.Value.As1().AsMemory() : input.Value.As2().InternalMemory;
            var descriptors = options?.stdio;
            return From(options, bytes, input is not null, descriptors?.length ?? 0,
                descriptors is not null && descriptors.length > 0 ? descriptors[0] : (Union<double, string>?)null,
                descriptors is not null && descriptors.length > 1 ? descriptors[1] : (Union<double, string>?)null,
                descriptors is not null && descriptors.length > 2 ? descriptors[2] : (Union<double, string>?)null);
        }

        private static SpawnSettings From<TInput, TStdio>(
            SpawnSyncOptions<TInput, TStdio>? options, ReadOnlyMemory<byte> bytes, bool hasInput, int descriptorCount,
            Union<double, string>? stdinDescriptor,
            Union<double, string>? stdoutDescriptor,
            Union<double, string>? stderrDescriptor)
            where TStdio : class
        {
            if (options?.encoding is not null and not "buffer")
                throw new ArgumentException("spawnSync Buffer options require encoding: 'buffer'.");
            ValidateControls(options?.uid, options?.gid, options?.timeout, options?.killSignal);
            var maximumBuffer = BufferLimit(options?.maxBuffer);
            if (descriptorCount > 3)
                throw new PlatformNotSupportedException("The .NET launcher cannot map extra file descriptors.");
            var stdin = Pipe(stdinDescriptor, 0);
            if (hasInput && !stdin)
                throw new ArgumentException("spawnSync input requires piped stdin.");
            return new(options?.cwd, options?.env, bytes,
                maximumBuffer, stdin, Pipe(stdoutDescriptor, 1), Pipe(stderrDescriptor, 2), false);
        }

        private static bool Pipe(Union<double, string>? entry, int index) =>
            entry is null ||
            (entry.Value.Is1() ? DescriptorPipe(entry.Value.As1(), index) : ModePipe(entry.Value.As2()));

        public static SpawnSettings From(ExecOptions? options)
        {
            ValidateControls(options?.uid, options?.gid, options?.timeout, options?.killSignal);
            if (options?.shell is not null || options?.argv0 is not null || options?.detached == true || options?.windowsVerbatimArguments == true)
                throw new PlatformNotSupportedException("The .NET synchronous launcher does not support shell, argv0, detached or verbatim-argument options.");
            if (options?.encoding is not null and not "buffer" and not "utf8" and not "utf-8")
                throw new PlatformNotSupportedException("The native synchronous launcher supports Buffer and UTF-8 output.");
            var pipe = ModePipe(options?.stdio ?? "pipe");
            if (options?.input is not null && !pipe)
                throw new ArgumentException("spawnSync input requires piped stdin.");
            return new(options?.cwd, options?.env,
                options?.input is null ? ReadOnlyMemory<byte>.Empty : Encoding.UTF8.GetBytes(options.input),
                BufferLimit(options?.maxBuffer), pipe, pipe, pipe, options?.windowsHide ?? false);
        }

        private static void ValidateControls(int? uid, int? gid, int? timeout, string? signal)
        {
            if (uid is not null || gid is not null)
                throw new PlatformNotSupportedException("System.Diagnostics.Process cannot independently select numeric uid or gid.");
            if (timeout < 0) throw new ArgumentOutOfRangeException(nameof(timeout));
            if (timeout.GetValueOrDefault() != 0 || signal is not null)
                throw new PlatformNotSupportedException("System.Diagnostics.Process cannot preserve Node timeout/signal termination evidence.");
        }

        private static int BufferLimit(int? value)
        {
            if (value < 0) throw new ArgumentOutOfRangeException(nameof(value), "maxBuffer must be nonnegative.");
            return value ?? 1024 * 1024;
        }

        private static bool DescriptorPipe(double descriptor, int index)
        {
            if (!double.IsFinite(descriptor) || descriptor < 0 || Math.Truncate(descriptor) != descriptor)
                throw new ArgumentOutOfRangeException(nameof(descriptor));
            if (descriptor == index) return false;
            throw new PlatformNotSupportedException("System.Diagnostics.Process cannot remap arbitrary numeric file descriptors.");
        }

        private static bool ModePipe(string mode) => mode switch
        {
            "pipe" => true,
            "inherit" => false,
            "ignore" => throw new PlatformNotSupportedException("System.Diagnostics.Process cannot redirect a child descriptor to the null device."),
            _ => throw new ArgumentException($"Unsupported stdio mode '{mode}'.")
        };
    }
}
