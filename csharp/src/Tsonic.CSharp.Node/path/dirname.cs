namespace Tsonic.CSharp.Node;

public static partial class path
{
    /// <summary>
    /// Returns the directory name of a path, similar to the Unix dirname command.
    /// </summary>
    /// <param name="path">The path to process.</param>
    /// <returns>The directory name.</returns>
    public static string dirname(string path)
    {
        if (string.IsNullOrEmpty(path))
            return ".";

        var result = Path.GetDirectoryName(path);
        return string.IsNullOrEmpty(result) ? "." : result;
    }
}
