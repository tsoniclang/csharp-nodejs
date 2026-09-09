using System.Threading.Tasks;

namespace Tsonic.CSharp.Node;

public static partial class fs
{
    /// <summary>
    /// Asynchronously renames (moves) a file or directory.
    /// </summary>
    /// <param name="oldPath">The old path.</param>
    /// <param name="newPath">The new path.</param>
    /// <returns>A promise that resolves when the rename is complete.</returns>
    public static Task rename(string oldPath, string newPath)
    {
        return Task.Run(() =>
        {
            if (File.Exists(oldPath))
            {
                File.Move(oldPath, newPath);
            }
            else if (Directory.Exists(oldPath))
            {
                Directory.Move(oldPath, newPath);
            }
            else
            {
                throw new FileNotFoundException($"No such file or directory: {oldPath}");
            }
        });
    }
}
