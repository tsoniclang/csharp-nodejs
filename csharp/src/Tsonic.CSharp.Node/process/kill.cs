using System.Diagnostics;

namespace Tsonic.CSharp.Node;

public static partial class process
{
    /// <summary>
    /// The process.kill() method sends the signal to the process identified by pid.
    /// Signal names are strings such as 'SIGINT' or 'SIGHUP'. If omitted, the signal defaults to 'SIGTERM'.
    /// </summary>
    /// <param name="pid">The process ID to send the signal to.</param>
    /// <param name="signal">The signal to send. Can be a string ('SIGTERM', 'SIGKILL', etc.) or a number. Defaults to 'SIGTERM'.</param>
    /// <returns>true if the signal was sent successfully.</returns>
    public static bool kill(int pid, object? signal = null)
    {
        try
        {
            var targetProcess = Process.GetProcessById(pid);

            // Parse signal - default is SIGTERM (terminate)
            var signalName = signal switch
            {
                null => "SIGTERM",
                string s => s.ToUpperInvariant(),
                int i when i == 0 => "0",           // 0 - check if process exists
                int i when i == 1 => "SIGHUP",      // 1 - hangup
                int i when i == 2 => "SIGINT",      // 2 - interrupt
                int i when i == 9 => "SIGKILL",     // 9 - kill
                int i when i == 15 => "SIGTERM",    // 15 - terminate
                _ => "SIGTERM"
            };

            // Handle different signals
            switch (signalName)
            {
                case "SIGKILL":
                case "SIGTERM":
                    targetProcess.Kill();
                    return true;

                case "SIGINT":
                case "SIGHUP":
                    // For SIGINT and SIGHUP, try CloseMainWindow first (graceful)
                    if (targetProcess.CloseMainWindow())
                    {
                        return true;
                    }
                    // If that fails, kill it
                    targetProcess.Kill();
                    return true;

                case "0":
                case "SIGNULL":
                    // Signal 0 - just check if process exists
                    return !targetProcess.HasExited;

                default:
                    // For unknown signals, try to terminate gracefully
                    targetProcess.Kill();
                    return true;
            }
        }
        catch (ArgumentException)
        {
            // Process doesn't exist
            throw new Exception($"kill ESRCH: No such process {pid}");
        }
        catch (Exception ex)
        {
            throw new Exception($"kill failed: {ex.Message}", ex);
        }
    }
}
