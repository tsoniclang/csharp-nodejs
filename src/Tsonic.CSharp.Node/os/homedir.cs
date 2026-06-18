namespace Tsonic.CSharp.Node;

public static partial class os
{
    /// <summary>
    /// Returns the string path of the current user's home directory.
    /// On POSIX, it uses the $HOME environment variable if defined.
    /// On Windows, it uses the USERPROFILE environment variable if defined.
    /// </summary>
    /// <returns>The home directory path.</returns>
    public static string homedir()
    {
        return Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
    }
}
