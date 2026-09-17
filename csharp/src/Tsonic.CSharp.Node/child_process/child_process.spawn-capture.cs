using System.Buffers;
using System.ComponentModel;
using System.Diagnostics;

namespace Tsonic.CSharp.Node;

public static partial class child_process
{
    private static SpawnSyncReturns<byte[]> RunSpawnSync(string command, IReadOnlyList<string> arguments, SpawnSettings settings)
    {
        ArgumentException.ThrowIfNullOrEmpty(command);
        ArgumentNullException.ThrowIfNull(arguments);
        if (command.Contains('\0')) throw new ArgumentException("A command cannot contain NUL.", nameof(command));
        using var child = new Process();
        var start = child.StartInfo;
        start.FileName = command;
        start.UseShellExecute = false;
        start.RedirectStandardInput = settings.StdinPipe;
        start.RedirectStandardOutput = settings.StdoutPipe;
        start.RedirectStandardError = settings.StderrPipe;
        start.CreateNoWindow = settings.HideWindow;
        for (var index = 0; index < arguments.Count; index++)
        {
            var argument = arguments[index] ?? throw new ArgumentException($"spawnSync argument array contains null at index {index}.", nameof(arguments));
            if (argument.Contains('\0')) throw new ArgumentException("An argument cannot contain NUL.", nameof(arguments));
            start.ArgumentList.Add(argument);
        }
        if (settings.Directory is not null)
        {
            if (settings.Directory.Contains('\0')) throw new ArgumentException("A working directory cannot contain NUL.");
            start.WorkingDirectory = settings.Directory.Length == 0 ? "" : Path.GetFullPath(settings.Directory);
        }
        if (settings.Environment is not null) ApplyEnvironment(start, settings.Environment);
        try
        {
            child.Start();
        }
        catch (Win32Exception error)
        {
            return new SpawnSyncReturns<byte[]> { output = [null, null, null], error = SpawnStartError(error) };
        }
        var pid = child.Id;
        var capture = new SpawnCapture(child, settings.MaximumBuffer);
        var stdout = settings.StdoutPipe ? capture.Read(child.StandardOutput.BaseStream) : Task.FromResult<byte[]?>(null);
        var stderr = settings.StderrPipe ? capture.Read(child.StandardError.BaseStream) : Task.FromResult<byte[]?>(null);
        var input = settings.StdinPipe ? capture.Write(child.StandardInput.BaseStream, settings.Input) : Task.CompletedTask;
        try
        {
            child.WaitForExit();
            Task.WhenAll(stdout, stderr, input).GetAwaiter().GetResult();
        }
        finally
        {
            if (!child.HasExited) child.Kill();
            child.WaitForExit();
        }
        if (!OperatingSystem.IsWindows() && child.ExitCode >= 128)
            throw new PlatformNotSupportedException(".NET does not distinguish this Unix exit code from signal termination; no signal or status is invented.");
        return new SpawnSyncReturns<byte[]>
        {
            pid = pid, stdout = stdout.Result, stderr = stderr.Result,
            output = [null, stdout.Result, stderr.Result], status = child.ExitCode
        };
    }

    private static SpawnSyncError SpawnStartError(Win32Exception error)
    {
        var code = OperatingSystem.IsWindows() ? error.NativeErrorCode switch
        {
            2 or 3 => "ENOENT", 5 => "EACCES", 8 or 14 => "ENOMEM", 193 => "ENOEXEC", _ => null
        } : OperatingSystem.IsLinux() || OperatingSystem.IsMacOS() ? error.NativeErrorCode switch
        {
            1 => "EPERM", 2 => "ENOENT", 8 => "ENOEXEC", 11 when OperatingSystem.IsLinux() => "EAGAIN",
            35 when OperatingSystem.IsMacOS() => "EAGAIN", 12 => "ENOMEM",
            13 => "EACCES", 20 => "ENOTDIR", 23 => "ENFILE", 24 => "EMFILE", _ => null
        } : null;
        return code is null
            ? throw new PlatformNotSupportedException($"No exact Node error mapping is defined for native launch error {error.NativeErrorCode}.", error)
            : new SpawnSyncError(code, error.Message);
    }

    private sealed class SpawnCapture(Process child, int maximumBytes)
    {
        private int _stopping;

        public async Task<byte[]?> Read(System.IO.Stream stream)
        {
            using var output = new MemoryStream();
            var buffer = ArrayPool<byte>.Shared.Rent(8192);
            try
            {
                while (true)
                {
                    var available = (int)Math.Min(buffer.Length, (long)maximumBytes - output.Length + 1);
                    var count = await stream.ReadAsync(buffer.AsMemory(0, available)).ConfigureAwait(false);
                    if (count == 0) return output.ToArray();
                    if (output.Length + count > maximumBytes)
                        throw new PlatformNotSupportedException("spawnSync maxBuffer exceeded; .NET cannot report exact Node signal termination. The child was stopped and no result is fabricated.");
                    output.Write(buffer, 0, count);
                }
            }
            catch
            {
                Stop();
                throw;
            }
            finally
            {
                ArrayPool<byte>.Shared.Return(buffer);
            }
        }

        public async Task Write(System.IO.Stream stream, ReadOnlyMemory<byte> input)
        {
            try
            {
                await stream.WriteAsync(input).ConfigureAwait(false);
            }
            catch
            {
                Stop();
                throw;
            }
            finally
            {
                stream.Dispose();
            }
        }

        private void Stop()
        {
            if (Interlocked.Exchange(ref _stopping, 1) != 0) return;
            try
            {
                if (!child.HasExited) child.Kill();
            }
            catch (InvalidOperationException) when (child.HasExited) { }
        }
    }
}
