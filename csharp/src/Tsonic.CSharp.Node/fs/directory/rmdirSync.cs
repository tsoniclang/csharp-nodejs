namespace Tsonic.CSharp.Node;

public static partial class fs
{
    /// <summary>
    /// Synchronously removes a directory.
    /// </summary>
    /// <param name="path">The directory path to remove.</param>
    /// <param name="recursive">If true, removes directory and all contents (default: false).</param>
    public static void rmdirSync(string path, bool recursive = false)
    {
        Directory.Delete(path, recursive);
    }
}
