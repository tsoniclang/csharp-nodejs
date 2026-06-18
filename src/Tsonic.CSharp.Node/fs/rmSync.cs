namespace Tsonic.CSharp.Node;

public static partial class fs
{
    /// <summary>
    /// Synchronously removes files and directories (modern API).
    /// </summary>
    /// <param name="path">The path to remove.</param>
    /// <param name="recursive">If true, removes directories and contents recursively.</param>
    public static void rmSync(string path, bool recursive = false)
    {
        if (File.Exists(path))
        {
            File.Delete(path);
        }
        else if (Directory.Exists(path))
        {
            Directory.Delete(path, recursive);
        }
        // Unlike unlink/rmdir, rm doesn't throw if path doesn't exist
    }
}
