using System.Runtime.InteropServices;

namespace Tsonic.CSharp.Node;

public static partial class os
{
    /// <summary>
    /// Returns a string identifying the operating system platform.
    /// Possible values are 'aix', 'darwin', 'freebsd', 'linux', 'openbsd', 'sunos', and 'win32'.
    /// The return value is equivalent to process.platform.
    /// </summary>
    /// <returns>The operating system platform.</returns>
    public static string platform()
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            return "win32";
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            return "linux";
        if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            return "darwin";
        if (RuntimeInformation.IsOSPlatform(OSPlatform.FreeBSD))
            return "freebsd";

        return RuntimeInformation.OSDescription.ToLowerInvariant();
    }
}
