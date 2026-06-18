namespace Tsonic.CSharp.Node;

public static partial class os
{
    /// <summary>
    /// Returns the operating system's default directory for temporary files as a string.
    /// </summary>
    /// <returns>The temporary directory path.</returns>
    public static string tmpdir()
    {
        return Path.GetTempPath().TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
    }
}
