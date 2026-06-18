namespace Tsonic.CSharp.Node;

public static partial class fs
{
    /// <summary>
    /// Synchronously truncates a file to a specified length.
    /// </summary>
    /// <param name="path">The file path.</param>
    /// <param name="len">The desired length in bytes (default: 0).</param>
    public static void truncateSync(string path, long len = 0)
    {
        using var fileStream = new FileStream(path, FileMode.Open, FileAccess.Write);
        fileStream.SetLength(len);
    }
}
