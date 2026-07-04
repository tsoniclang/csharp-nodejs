namespace Tsonic.CSharp.Node;

public static partial class os
{
    /// <summary>
    /// The operating system-specific end-of-line marker.
    /// \n on POSIX
    /// \r\n on Windows
    /// </summary>
    public static readonly string EOL = Environment.NewLine;
}
