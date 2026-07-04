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
public class ExecOptions
{
    /// <summary>
    /// Current working directory of the child process.
    /// </summary>
    public string? cwd { get; set; }

    /// <summary>
    /// Environment variables to pass to the child process.
    /// </summary>
    public IReadOnlyDictionary<string, string?>? env { get; set; }

    /// <summary>
    /// Encoding to use for string output ('utf8', 'buffer', etc). Default is 'buffer' (returns byte[]).
    /// </summary>
    public string? encoding { get; set; }

    /// <summary>
    /// Shell to execute the command with (default: '/bin/sh' on Unix, 'cmd.exe' on Windows).
    /// </summary>
    public string? shell { get; set; }

    /// <summary>
    /// Timeout in milliseconds (default: 0 = no timeout).
    /// </summary>
    public int timeout { get; set; }

    /// <summary>
    /// Largest amount of data in bytes allowed on stdout or stderr (default: 1024*1024).
    /// </summary>
    public int maxBuffer { get; set; } = 1024 * 1024;

    /// <summary>
    /// Signal to use to kill the process (default: 'SIGTERM').
    /// </summary>
    public string? killSignal { get; set; }

    /// <summary>
    /// Hide the subprocess console window on Windows (default: false).
    /// </summary>
    public bool windowsHide { get; set; }

    /// <summary>
    /// No quoting or escaping of arguments on Windows (default: false).
    /// </summary>
    public bool windowsVerbatimArguments { get; set; }

    /// <summary>
    /// Prepare child to run independently of its parent process (Unix only).
    /// </summary>
    public bool detached { get; set; }

    /// <summary>
    /// User identity of the process (Unix only).
    /// </summary>
    public int? uid { get; set; }

    /// <summary>
    /// Group identity of the process (Unix only).
    /// </summary>
    public int? gid { get; set; }

    /// <summary>
    /// Explicitly set the value of argv[0] sent to the child process.
    /// </summary>
    public string? argv0 { get; set; }

    /// <summary>
    /// stdio configuration ('pipe', 'inherit', 'ignore').
    /// </summary>
    public string? stdio { get; set; }

    /// <summary>
    /// Input to be sent to stdin (for sync methods).
    /// </summary>
    public string? input { get; set; }
}

/// <summary>
/// The child_process module provides the ability to spawn subprocesses.
/// </summary>
public static partial class child_process
{
    private static void RunBackground(string name, Action action)
    {
        var thread = new System.Threading.Thread(() => action())
        {
            IsBackground = true,
            Name = name,
        };
        thread.Start();
    }

    // ==================== execSync ====================

    /// <summary>
    /// Synchronous version of exec() that will block until the child process exits.
    /// Returns the stdout from the command as a byte array.
    /// </summary>
    /// <param name="command">The command to run, with space-separated arguments</param>
    /// <returns>The stdout from the command</returns>
    public static byte[] execSync(string command)
    {
        var result = execSync(command, (ExecOptions?)null);
        return result is byte[] bytes ? bytes : Encoding.UTF8.GetBytes((string)result);
    }

    /// <summary>
    /// Synchronous version of exec() that will block until the child process exits.
    /// </summary>
    /// <param name="command">The command to run, with space-separated arguments</param>
    /// <param name="options">Options object (with encoding, cwd, env, shell, etc.)</param>
    /// <returns>The stdout from the command (byte[] or string depending on encoding option)</returns>
    public static object execSync(string command, ExecOptions? options)
    {
        var (shell, cwd, env, encoding, timeout, maxBuffer) = ParseExecOptions(options);

        using var process = new Process();

        // Use shell to execute the command
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            process.StartInfo.FileName = shell ?? "cmd.exe";
            process.StartInfo.Arguments = $"/c {command}";
        }
        else
        {
            process.StartInfo.FileName = shell ?? "/bin/sh";
            process.StartInfo.Arguments = $"-c \"{command.Replace("\"", "\\\"")}\"";
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

        var stdoutData = new StringBuilder();
        var stderrData = new StringBuilder();

        process.OutputDataReceived += (sender, e) =>
        {
            if (e.Data != null)
                stdoutData.AppendLine(e.Data);
        };

        process.ErrorDataReceived += (sender, e) =>
        {
            if (e.Data != null)
                stderrData.AppendLine(e.Data);
        };

        try
        {
            process.Start();
            process.BeginOutputReadLine();
            process.BeginErrorReadLine();

            bool exited;
            if (timeout > 0)
            {
                exited = process.WaitForExit((int)timeout);
                if (!exited)
                {
                    process.Kill(entireProcessTree: true);
                    throw new TimeoutException($"Command timed out after {timeout}ms: {command}");
                }
            }
            else
            {
                process.WaitForExit();
                exited = true;
            }

            if (process.ExitCode != 0)
            {
                throw new InvalidOperationException(
                    $"Command failed with exit code {process.ExitCode}: {command}\n" +
                    $"stderr: {stderrData}"
                );
            }

            var output = stdoutData.ToString();

            if (encoding != null && encoding != "buffer")
            {
                return output;
            }
            else
            {
                return Encoding.UTF8.GetBytes(output);
            }
        }
        catch (Exception ex) when (ex is not TimeoutException and not InvalidOperationException)
        {
            throw new InvalidOperationException($"Failed to execute command: {command}", ex);
        }
    }

    // ==================== spawnSync ====================



}

#pragma warning restore CS8981
#pragma warning restore IDE1006
