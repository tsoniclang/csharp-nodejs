using System;
using System.Diagnostics;

namespace Tsonic.CSharp.Node;

/// <summary>
/// Instances of ChildProcess represent spawned child processes.
/// Instances are not intended to be created directly.
/// Use spawn(), exec(), execFile(), or fork() to create instances.
/// </summary>
public class ChildProcess : EventEmitter
{
    private readonly Process _process;
    private bool _killed = false;

    internal ChildProcess(Process process)
    {
        _process = process ?? throw new ArgumentNullException(nameof(process));
    }

    /// <summary>
    /// A Writable Stream that represents the child process's stdin.
    /// If the child was spawned with stdio[0] set to anything other than 'pipe', this will be null.
    /// </summary>
    public Writable? stdin { get; internal set; }

    /// <summary>
    /// A Readable Stream that represents the child process's stdout.
    /// If the child was spawned with stdio[1] set to anything other than 'pipe', this will be null.
    /// </summary>
    public Readable? stdout { get; internal set; }

    /// <summary>
    /// A Readable Stream that represents the child process's stderr.
    /// If the child was spawned with stdio[2] set to anything other than 'pipe', this will be null.
    /// </summary>
    public Readable? stderr { get; internal set; }

    /// <summary>
    /// The process identifier (PID) of the child process.
    /// </summary>
    public int pid => _process.Id;

    /// <summary>
    /// Indicates whether it is still possible to send and receive messages from the child process.
    /// </summary>
    public bool connected { get; internal set; } = false;

    /// <summary>
    /// Indicates whether the child process successfully received a signal from kill().
    /// </summary>
    public bool killed => _killed;

    /// <summary>
    /// Indicates whether the child process is referenced (parent will wait for it).
    /// When true, the parent process will wait for this child to exit.
    /// When false (unreferenced), the parent can exit independently.
    /// </summary>
    public bool referenced { get; private set; } = true;

    /// <summary>
    /// The exit code of the child process. Returns null if the process has not yet exited.
    /// </summary>
    public int? exitCode { get; internal set; }

    /// <summary>
    /// The signal by which the child process was terminated. Returns null if not terminated by signal.
    /// </summary>
    public string? signalCode { get; internal set; }

    /// <summary>
    /// The full list of command-line arguments the child process was launched with.
    /// </summary>
    public string[] spawnargs { get; internal set; } = Array.Empty<string>();

    /// <summary>
    /// The executable file name of the child process that is launched.
    /// </summary>
    public string spawnfile { get; internal set; } = string.Empty;

    /// <summary>
    /// The subprocess.kill() method sends a signal to the child process.
    /// </summary>
    /// <param name="signal">The signal to send (default: 'SIGTERM')</param>
    /// <returns>True if the signal was sent successfully</returns>
    public bool kill(string? signal = null)
    {
        try
        {
            if (_process.HasExited)
                return false;

            // Map Node.js signal names to .NET Process.Kill()
            // Note: .NET doesn't support sending specific signals on all platforms
            // On Windows, this always terminates the process
            // On Unix, we can only terminate (similar to SIGKILL)

            if (signal == "SIGKILL" || signal == "SIGTERM" || signal == null)
            {
                _process.Kill(entireProcessTree: true);
                _killed = true;
                return true;
            }
            else
            {
                // For other signals, we'll just terminate
                // This is a limitation of .NET's Process class
                _process.Kill(entireProcessTree: true);
                _killed = true;
                signalCode = signal;
                return true;
            }
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Closes the IPC channel between parent and child.
    /// </summary>
    public void disconnect()
    {
        connected = false;
        // IPC implementation would go here
    }

    /// <summary>
    /// When an IPC channel exists, the send() method sends messages to the child process.
    /// </summary>
    /// <param name="message">The message to send</param>
    /// <param name="sendHandle">Optional handle to send</param>
    /// <param name="options">Optional send options</param>
    /// <param name="callback">Optional callback when message is sent</param>
    /// <returns>True if message was queued successfully</returns>
    public bool send(object message, object? sendHandle = null, object? options = null, Action<Exception?>? callback = null)
    {
        if (!connected)
        {
            callback?.Invoke(new InvalidOperationException("Channel closed"));
            return false;
        }

        // IPC implementation would go here
        callback?.Invoke(null);
        return true;
    }

    /// <summary>
    /// Calling ref() on a child process will prevent the parent process from exiting
    /// until the child exits. This is the default behavior.
    /// </summary>
    public void @ref()
    {
        referenced = true;
    }

    /// <summary>
    /// Calling unref() on a child process will allow the parent process to exit
    /// independently of the child. The child process will continue running in the background.
    /// </summary>
    public void @unref()
    {
        referenced = false;
    }

    internal Process GetProcess() => _process;
}
