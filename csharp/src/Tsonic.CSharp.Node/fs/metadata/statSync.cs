namespace Tsonic.CSharp.Node;

public static partial class fs
{
    /// <summary>
    /// Synchronously retrieves statistics for the file/directory at the given path.
    /// </summary>
    /// <param name="path">The file or directory path.</param>
    /// <returns>A Stats object.</returns>
    public static Stats statSync(string path)
    {
        return ReadStats(path, followLink: true);
    }
}
