using System.Runtime.InteropServices;

namespace Tsonic.CSharp.Node;

public static partial class os
{
    /// <summary>
    /// Returns the operating system release version as a string.
    /// On POSIX systems, the operating system release is determined by calling uname(3).
    /// On Windows, GetVersionExW() is used.
    /// </summary>
    /// <returns>The operating system release version.</returns>
    public static string release()
    {
        return Environment.OSVersion.Version.ToString();
    }
}
