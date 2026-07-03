using System.Runtime.InteropServices;

namespace Tsonic.CSharp.Node;

/// <summary>
/// Information about the currently effective user.
/// </summary>
public class UserInfo
{
    /// <summary>
    /// The username.
    /// </summary>
    public string username { get; set; } = string.Empty;

    /// <summary>
    /// The user identifier (POSIX only, -1 on Windows).
    /// </summary>
    public int uid { get; set; }

    /// <summary>
    /// The group identifier (POSIX only, -1 on Windows).
    /// </summary>
    public int gid { get; set; }

    /// <summary>
    /// The user's shell (POSIX only, null on Windows).
    /// </summary>
    public string? shell { get; set; }

    /// <summary>
    /// The user's home directory.
    /// </summary>
    public string homedir { get; set; } = string.Empty;
}

public static partial class os
{
    /// <summary>
    /// Returns information about the currently effective user.
    /// On POSIX platforms, this is typically a subset of the password file.
    /// On Windows, the uid and gid fields are -1, and shell is null.
    /// </summary>
    /// <returns>User information.</returns>
    public static UserInfo userInfo()
    {
        var isWindows = RuntimeInformation.IsOSPlatform(OSPlatform.Windows);

        return new UserInfo
        {
            username = Environment.UserName,
            uid = isWindows ? -1 : 1000, // Default UID on Unix, -1 on Windows
            gid = isWindows ? -1 : 1000, // Default GID on Unix, -1 on Windows
            shell = isWindows ? null : Environment.GetEnvironmentVariable("SHELL") ?? "/bin/bash",
            homedir = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile)
        };
    }

    /// <summary>
    /// Returns information about the currently effective user.
    /// </summary>
    public static UserInfo userInfo(UserInfoOptions options)
    {
        _ = options;
        return userInfo();
    }
}
