namespace Tsonic.CSharp.Node;

public static partial class fs
{
    /// <summary>
    /// Synchronously changes the permissions of a file.
    /// </summary>
    /// <param name="path">The file path.</param>
    /// <param name="mode">The permission mode (Unix-style permissions).</param>
    public static void chmodSync(string path, int mode)
    {
        // Limited implementation on Windows - can only set/unset readonly
        // On Unix, this would work with full permission bits
        if (File.Exists(path))
        {
            var fileInfo = new FileInfo(path);
            // Check if write permission is being removed (mode & 0200 == 0)
            if ((mode & 0x80) == 0) // Owner write bit
            {
                fileInfo.IsReadOnly = true;
            }
            else
            {
                fileInfo.IsReadOnly = false;
            }
        }
        else if (Directory.Exists(path))
        {
            // Directory permissions are more complex on Windows
            // For now, just ensure it exists
        }
        else
        {
            throw new FileNotFoundException($"No such file or directory: {path}");
        }
    }
}
