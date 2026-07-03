using System.Runtime.InteropServices;

namespace Tsonic.CSharp.Node;

public static partial class process
{
    /// <summary>
    /// The operating system platform on which the Node.js process is running.
    /// Possible values are: 'aix', 'darwin', 'freebsd', 'linux', 'openbsd', 'sunos', 'win32'.
    /// </summary>
    public static string platform
    {
        get
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                return "win32";
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                return "linux";
            if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                return "darwin";
            if (RuntimeInformation.IsOSPlatform(OSPlatform.FreeBSD))
                return "freebsd";

            // Fallback to OS description
            return RuntimeInformation.OSDescription.ToLowerInvariant();
        }
    }
}
