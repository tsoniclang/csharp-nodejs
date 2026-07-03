namespace Tsonic.CSharp.Node;

public static partial class fs
{
    /// <summary>
    /// Returns true if the path exists, false otherwise.
    /// </summary>
    /// <param name="path">The path to check.</param>
    /// <returns>True if the path exists.</returns>
    public static bool existsSync(string path)
    {
        return File.Exists(path) || Directory.Exists(path);
    }
}
