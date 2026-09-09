namespace Tsonic.CSharp.Node;

public static partial class fs
{
    /// <summary>
    /// Synchronously tests a user's permissions for the file or directory specified by path.
    /// </summary>
    /// <param name="path">The path to check.</param>
    /// <param name="mode">Optional mode (not fully implemented on all platforms).</param>
    public static void accessSync(string path, int mode = 0)
    {
        // Basic implementation: check if file/directory exists and is readable
        if (!File.Exists(path) && !Directory.Exists(path))
        {
            throw new FileNotFoundException($"No such file or directory: {path}");
        }

        // On Windows, permission checks are limited
        // Mode 0 (F_OK) - just existence check (already done above)
        // Mode 4 (R_OK) - readable (assume yes if we can stat it)
        // Mode 2 (W_OK) - writable (check attributes)
        // Mode 1 (X_OK) - executable (not easily checkable on Windows)

        if (mode == 2 || mode == 6) // W_OK or R_OK | W_OK
        {
            if (File.Exists(path))
            {
                var fileInfo = new FileInfo(path);
                if (fileInfo.IsReadOnly)
                {
                    throw new UnauthorizedAccessException($"Permission denied: {path}");
                }
            }
        }
    }
}
