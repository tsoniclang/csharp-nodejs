using System;

namespace Tsonic.CSharp.Node;

/// <summary>
/// Return type for spawnSync and related synchronous child process methods.
/// </summary>
/// <typeparam name="T">The type of output data (string or byte[])</typeparam>
public class SpawnSyncReturns<T>
{
    /// <summary>
    /// The process ID of the spawned child process.
    /// </summary>
    public int pid { get; set; }

    /// <summary>
    /// Array containing the results from stdio output.
    /// </summary>
    public T?[] output { get; set; } = Array.Empty<T?>();

    /// <summary>
    /// The contents of stdout.
    /// </summary>
    public T stdout { get; set; } = default!;

    /// <summary>
    /// The contents of stderr.
    /// </summary>
    public T stderr { get; set; } = default!;

    /// <summary>
    /// The exit code of the subprocess, or null if the subprocess terminated due to a signal.
    /// </summary>
    public int? status { get; set; }

    /// <summary>
    /// The signal used to kill the subprocess, or null.
    /// </summary>
    public string? signal { get; set; }

    /// <summary>
    /// Error object if the child process failed or timed out.
    /// </summary>
    public Exception? error { get; set; }
}
