using System.Diagnostics;
using System.Runtime.InteropServices;

namespace Tsonic.CSharp.Node;

public static partial class process
{
    /// <summary>
    /// The PID of the current process.
    /// </summary>
    public static int pid => Environment.ProcessId;

    /// <summary>
    /// The PID of the parent process.
    /// </summary>
    public static int ppid
    {
        get
        {
            try
            {
                using var currentProcess = Process.GetCurrentProcess();
                // Try to get parent process ID
                // This is platform-specific and may not always be available
                return GetParentProcessId(currentProcess);
            }
            catch
            {
                return 0;
            }
        }
    }

    private static int GetParentProcessId(Process process)
    {
        try
        {
            // On Windows, we can use WMI or native methods
            // On Unix, we can read from /proc
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                // For Windows, we'd need P/Invoke or WMI
                // For now, return 0 (not implemented)
                return 0;
            }
            else
            {
                // On Unix-like systems, read from /proc/[pid]/stat
                var statPath = $"/proc/{process.Id}/stat";
                if (File.Exists(statPath))
                {
                    var stat = File.ReadAllText(statPath);
                    // The format is: pid (name) state ppid ...
                    // We need to find the ppid which is the 4th field
                    var parts = stat.Split(' ');
                    if (parts.Length > 3 && int.TryParse(parts[3], out var ppid))
                    {
                        return ppid;
                    }
                }
            }
        }
        catch
        {
            // If we can't determine parent PID, return 0
        }

        return 0;
    }
}
