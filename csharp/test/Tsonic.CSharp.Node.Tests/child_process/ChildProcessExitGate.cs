using System;
using System.Globalization;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;

namespace Tsonic.CSharp.Node.Tests;

internal sealed class ChildProcessExitGate : IDisposable
{
    private readonly string _gatePath;
    private int _released;

    private ChildProcessExitGate(string gatePath, ChildProcess child)
    {
        _gatePath = gatePath;
        Child = child;
    }

    public ChildProcess Child { get; }

    public static ChildProcessExitGate Spawn(int exitCode)
    {
        var gateDirectory = Path.Combine(AppContext.BaseDirectory, ".child-process-exit-gates");
        Directory.CreateDirectory(gateDirectory);
        var gatePath = Path.Combine(gateDirectory, Guid.NewGuid().ToString("N", CultureInfo.InvariantCulture));
        File.WriteAllText(gatePath, string.Empty);

        try
        {
            var child = RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
                ? child_process.spawn(
                    "powershell.exe",
                    new[]
                    {
                        "-NoLogo",
                        "-NoProfile",
                        "-NonInteractive",
                        "-Command",
                        "while (Test-Path -LiteralPath $args[0]) { Start-Sleep -Milliseconds 10 }; exit [int]$args[1]",
                        gatePath,
                        exitCode.ToString(CultureInfo.InvariantCulture),
                    })
                : child_process.spawn(
                    "sh",
                    new[]
                    {
                        "-c",
                        "while [ -e \"$1\" ]; do sleep 0.01; done; exit \"$2\"",
                        "tsonic-child-process-exit-gate",
                        gatePath,
                        exitCode.ToString(CultureInfo.InvariantCulture),
                    });

            return new ChildProcessExitGate(gatePath, child);
        }
        catch
        {
            File.Delete(gatePath);
            throw;
        }
    }

    public void Release()
    {
        if (Interlocked.Exchange(ref _released, 1) == 0)
        {
            File.Delete(_gatePath);
        }
    }

    public void Dispose()
    {
        Release();
        if (Child.exitCode is null)
        {
            Child.kill();
        }
    }
}
