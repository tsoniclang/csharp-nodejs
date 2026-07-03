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
    /// Async version of execSync(). Spawns a shell and runs a command within that shell.
    /// </summary>
    /// <param name="command">The command to run</param>
    /// <param name="options">Options object</param>
    /// <param name="callback">Callback function (error, stdout, stderr)</param>
    public static void exec(string command, ExecOptions? options, Action<Exception?, string, string> callback)
    {
        RunBackground("Tsonic.CSharp.Node.exec", () =>
        {
            try
            {
                var result = execSync(command, options);
                var stdout = result is string str ? str : Encoding.UTF8.GetString((byte[])result);
                callback(null, stdout, "");
            }
            catch (Exception ex)
            {
                callback(ex, "", ex.Message);
            }
        });
    }

    /// <summary>
    /// Async version of execSync() with default options.
    /// </summary>
    public static void exec(string command, Action<Exception?, string, string> callback)
    {
        exec(command, null, callback);
    }

    /// <summary>
    /// Spawns a new process asynchronously using the given command.
    /// </summary>
    /// <param name="command">The command to run</param>
    /// <param name="args">List of string arguments</param>
    /// <param name="options">Options object</param>
    /// <returns>ChildProcess instance</returns>
    public static ChildProcess spawn(string command, string[]? args = null, ExecOptions? options = null)
    {
        var process = new Process();
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

        var (shell, cwd, env, encoding, timeout, maxBuffer) = ParseExecOptions(options);

        if (cwd != null)
            process.StartInfo.WorkingDirectory = cwd;

        if (env != null)
        {
            ApplyEnvironment(process.StartInfo, env);
        }

        var childProcess = new ChildProcess(process);

        // Set spawn properties
        childProcess.spawnfile = command;
        var spawnArgsList = new System.Collections.Generic.List<string> { command };
        if (args != null)
            spawnArgsList.AddRange(args);
        childProcess.spawnargs = spawnArgsList.ToArray();

        // Create stream wrappers for stdin/stdout/stderr
        // These would need proper Readable/Writable stream implementations
        // For now, we'll set them to null
        childProcess.stdin = null;
        childProcess.stdout = null;
        childProcess.stderr = null;

        // Enable the Exited event to fire
        process.EnableRaisingEvents = true;

        process.Exited += (sender, e) =>
        {
            childProcess.exitCode = process.ExitCode;
            childProcess.emit("exit", process.ExitCode, childProcess.signalCode);
            childProcess.emit("close", process.ExitCode, childProcess.signalCode);
        };

        try
        {
            process.Start();
            childProcess.emit("spawn");
        }
        catch (Exception ex)
        {
            childProcess.emit("error", ex);
        }

        return childProcess;
    }

    /// <summary>
    /// Async version of execFileSync().
    /// </summary>
    public static void execFile(string file, string[]? args, ExecOptions? options, Action<Exception?, string, string> callback)
    {
        RunBackground("Tsonic.CSharp.Node.execFile", () =>
        {
            try
            {
                var result = execFileSync(file, args, options);
                var stdout = result is string str ? str : Encoding.UTF8.GetString((byte[])result);
                callback(null, stdout, "");
            }
            catch (Exception ex)
            {
                callback(ex, "", ex.Message);
            }
        });
    }

    /// <summary>
    /// Fork a new Node.js process.
    /// Note: This is a simplified implementation that spawns a new process.
    /// Full IPC channel support would require additional implementation.
    /// </summary>
    /// <param name="modulePath">The module to run in the child process</param>
    /// <param name="args">List of string arguments</param>
    /// <param name="options">Options object</param>
    /// <returns>ChildProcess instance with IPC channel</returns>
    public static ChildProcess fork(string modulePath, string[]? args = null, ExecOptions? options = null)
    {
        // Determine Node.js/dotnet executable path
        var execPath = Process.GetCurrentProcess().MainModule?.FileName ?? "dotnet";

        var allArgs = new System.Collections.Generic.List<string> { modulePath };
        if (args != null)
            allArgs.AddRange(args);

        var childProcess = spawn(execPath, allArgs.ToArray(), options);
        childProcess.connected = true;

        return childProcess;
    }

    // ==================== Helper Methods ====================
}
