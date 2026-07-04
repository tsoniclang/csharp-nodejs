using System.Runtime.InteropServices;

namespace Tsonic.CSharp.Node;

public static partial class os
{
    /// <summary>
    /// Returns a human-readable operating system version string.
    /// </summary>
    public static string version()
    {
        return RuntimeInformation.OSDescription;
    }
}
