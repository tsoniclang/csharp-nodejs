namespace Tsonic.CSharp.Node;

/// <summary>
/// Implements Node.js path module functionality using .NET path APIs.
/// All methods follow JavaScript naming conventions (lowercase).
/// </summary>
public static partial class path
{
    /// <summary>
    /// Platform-specific path segment separator.
    /// </summary>
    public static readonly string sep = Path.DirectorySeparatorChar.ToString();

    /// <summary>
    /// Platform-specific path delimiter for environment variables.
    /// </summary>
    public static readonly string delimiter = Path.PathSeparator.ToString();

    /// <summary>
    /// POSIX-specific path operations. Returns the same path module instance.
    /// </summary>
    public static PathModule posix => PathModule.Instance;

    /// <summary>
    /// Windows-specific path operations. Returns the same path module instance.
    /// </summary>
    public static PathModule win32 => PathModule.Instance;
}
