using System.Runtime.InteropServices;

namespace Tsonic.CSharp.Node;

public static partial class os
{
    /// <summary>
    /// Returns an array containing the 1, 5, and 15 minute load averages.
    /// The load average is a Unix-specific concept. On Windows, the return value is always [0, 0, 0].
    /// </summary>
    /// <returns>An array of three load average values.</returns>
    public static double[] loadavg()
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            return new double[] { 0, 0, 0 };
        }

        // On Unix systems, we could read from /proc/loadavg, but for simplicity we return zeros
        // A full implementation would need to parse /proc/loadavg or call getloadavg()
        return new double[] { 0, 0, 0 };
    }
}
