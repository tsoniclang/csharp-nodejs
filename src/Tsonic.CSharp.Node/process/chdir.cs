namespace Tsonic.CSharp.Node;

public static partial class process
{
    /// <summary>
    /// The process.chdir() method changes the current working directory of the Node.js process
    /// or throws an exception if doing so fails (for instance, if the specified directory does not exist).
    /// </summary>
    /// <param name="directory">The directory to change to.</param>
    public static void chdir(string directory)
    {
        if (string.IsNullOrEmpty(directory))
        {
            throw new ArgumentException("Directory path cannot be null or empty.", nameof(directory));
        }

        if (!Directory.Exists(directory))
        {
            throw new DirectoryNotFoundException($"Directory not found: {directory}");
        }

        Directory.SetCurrentDirectory(directory);
    }
}
