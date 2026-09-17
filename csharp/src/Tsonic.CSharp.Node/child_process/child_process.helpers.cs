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
    private static (string? shell, string? cwd, IReadOnlyDictionary<string, string?>? env, string? encoding, int timeout, int maxBuffer)
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

    private static void ApplyEnvironment(ProcessStartInfo startInfo, IReadOnlyDictionary<string, string?> env)
    {
        startInfo.Environment.Clear();
        foreach (var pair in env)
        {
            if (pair.Value != null)
            {
                if (pair.Key.Length == 0 || pair.Key.Contains('=') || pair.Key.Contains('\0') || pair.Value.Contains('\0'))
                    throw new ArgumentException("A child environment requires nonempty names without '=' or NUL, and values without NUL.");
                startInfo.Environment[pair.Key] = pair.Value;
            }
        }
    }
}
