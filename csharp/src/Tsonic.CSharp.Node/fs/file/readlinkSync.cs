namespace Tsonic.CSharp.Node;

public static partial class fs
{
    /// <summary>
    /// Synchronously reads the contents of a symbolic link.
    /// </summary>
    /// <param name="path">The symbolic link path.</param>
    /// <returns>The target of the symbolic link.</returns>
    public static string readlinkSync(string path)
    {
        if (File.Exists(path))
        {
            var linkInfo = new FileInfo(path);
            return linkInfo.LinkTarget ?? throw new IOException($"Not a symbolic link: {path}");
        }
        else if (Directory.Exists(path))
        {
            var linkInfo = new DirectoryInfo(path);
            return linkInfo.LinkTarget ?? throw new IOException($"Not a symbolic link: {path}");
        }
        throw new FileNotFoundException($"No such file or directory: {path}");
    }
}
