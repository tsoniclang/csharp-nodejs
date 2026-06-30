using System;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Runtime.InteropServices;
using System.Linq;
using System.Collections.Generic;

namespace Tsonic.CSharp.Node;

#pragma warning disable CS8981 // Lowercase type names
#pragma warning disable IDE1006 // Naming rule violation

/// <summary>
/// Options for exec, spawn, and related methods.
/// </summary>
public static partial class child_process
{
    /// <summary>
    /// Synchronous version of spawn() that will block until the child process exits.
    /// </summary>
    /// <param name="command">The command to run</param>
    /// <param name="args">List of string arguments</param>
    /// <param name="options">Options object</param>
    /// <returns>SpawnSyncReturns object containing pid, output, stdout, stderr, status, signal</returns>
    public static SpawnSyncReturns<byte[]> spawnSync(string command, string[]? args = null, ExecOptions? options = null)
    {
        var (shell, cwd, env, encoding, timeout, maxBuffer) = ParseExecOptions(options);

        using var process = new Process();
        process.StartInfo.FileName = command;

        if (args != null)
        {
            foreach (var arg in args)
            {
                process.StartInfo.ArgumentList.Add(arg);
            }
        }

        process.StartInfo.UseShellExecute = false;
        process.StartInfo.RedirectStandardOutput = true;
        process.StartInfo.RedirectStandardError = true;
        process.StartInfo.RedirectStandardInput = true;
        process.StartInfo.CreateNoWindow = true;

        if (cwd != null)
            process.StartInfo.WorkingDirectory = cwd;

        if (env != null)
        {
            ApplyEnvironment(process.StartInfo, env);
        }

        var result = new SpawnSyncReturns<byte[]>();
        var stdoutData = new MemoryStream();
        var stderrData = new MemoryStream();

        try
        {
            process.Start();
            result.pid = process.Id;

            var stdoutTask = Task.Run(() => process.StandardOutput.BaseStream.CopyToAsync(stdoutData));
            var stderrTask = Task.Run(() => process.StandardError.BaseStream.CopyToAsync(stderrData));

            bool exited;
            if (timeout > 0)
            {
                exited = process.WaitForExit((int)timeout);
                if (!exited)
                {
                    process.Kill(entireProcessTree: true);
                    result.signal = "SIGTERM";
                    result.error = new TimeoutException($"Process timed out after {timeout}ms");
                }
            }
            else
            {
                process.WaitForExit();
                exited = true;
            }

            Task.WaitAll(stdoutTask, stderrTask);

            result.stdout = stdoutData.ToArray();
            result.stderr = stderrData.ToArray();
            result.output = new byte[]?[] { null, result.stdout, result.stderr };

            if (exited)
            {
                result.status = process.ExitCode;
            }
        }
        catch (Exception ex)
        {
            result.error = ex;
            result.status = null;
        }

        return result;
    }

    /// <summary>
    /// Synchronous version of spawn() that will block until the child process exits.
    /// Returns string output when encoding is specified.
    /// </summary>
    public static SpawnSyncReturns<string> spawnSyncString(string command, string[]? args = null, ExecOptions? options = null)
    {
        var byteResult = spawnSync(command, args, options);

        return new SpawnSyncReturns<string>
        {
            pid = byteResult.pid,
            stdout = Encoding.UTF8.GetString(byteResult.stdout),
            stderr = Encoding.UTF8.GetString(byteResult.stderr),
            output = byteResult.output?.Select(b => b != null ? Encoding.UTF8.GetString(b) : null).ToArray() ?? Array.Empty<string?>(),
            status = byteResult.status,
            signal = byteResult.signal,
            error = byteResult.error
        };
    }

    // ==================== execFileSync ====================

    /// <summary>
    /// Synchronous version of execFile() that spawns the command directly without a shell.
    /// </summary>
    /// <param name="file">The name or path of the executable file to run</param>
    /// <param name="args">List of string arguments</param>
    /// <param name="options">Options object</param>
    /// <returns>The stdout from the command (byte[] or string depending on encoding option)</returns>
    public static object execFileSync(string file, string[]? args = null, ExecOptions? options = null)
    {
        var result = spawnSync(file, args, options);

        if (result.error != null)
            throw result.error;

        if (result.status != 0)
        {
            throw new InvalidOperationException(
                $"Command failed with exit code {result.status}: {file}\n" +
                $"stderr: {Encoding.UTF8.GetString(result.stderr)}"
            );
        }

        var (shell, cwd, env, encoding, timeout, maxBuffer) = ParseExecOptions(options);

        if (encoding != null && encoding != "buffer")
        {
            return Encoding.UTF8.GetString(result.stdout);
        }
        else
        {
            return result.stdout;
        }
    }

    // ==================== Async Methods ====================
}
