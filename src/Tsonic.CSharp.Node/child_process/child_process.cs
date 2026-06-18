using System;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Runtime.InteropServices;
using System.Linq;

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
    public object? env { get; set; }

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
public static class child_process
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
            process.StartInfo.Environment.Clear();
            var envType = env.GetType();
            foreach (var prop in envType.GetProperties())
            {
                var value = prop.GetValue(env)?.ToString();
                if (value != null)
                    process.StartInfo.Environment[prop.Name] = value;
            }
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
            process.StartInfo.Environment.Clear();
            var envType = env.GetType();
            foreach (var prop in envType.GetProperties())
            {
                var value = prop.GetValue(env)?.ToString();
                if (value != null)
                    process.StartInfo.Environment[prop.Name] = value;
            }
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
            process.StartInfo.Environment.Clear();
            var envType = env.GetType();
            foreach (var prop in envType.GetProperties())
            {
                var value = prop.GetValue(env)?.ToString();
                if (value != null)
                    process.StartInfo.Environment[prop.Name] = value;
            }
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

    private static (string? shell, string? cwd, object? env, string? encoding, int timeout, int maxBuffer)
        ParseExecOptions(ExecOptions? options)
    {
        if (options == null)
            return (null, null, null, null, 0, 1024 * 1024);

        return (
            options.shell,
            options.cwd,
            options.env,
            options.encoding,
            options.timeout,
            options.maxBuffer
        );
    }
}

#pragma warning restore CS8981
#pragma warning restore IDE1006
