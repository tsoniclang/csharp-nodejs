using System.Threading.Tasks;

namespace Tsonic.CSharp.Node;

public static partial class fs
{
    /// <summary>
    /// Asynchronously creates a symbolic link.
    /// </summary>
    /// <param name="target">The target path.</param>
    /// <param name="path">The symbolic link path.</param>
    /// <param name="type">The type of symbolic link (file or directory).</param>
    /// <returns>A promise that resolves when the symlink is created.</returns>
    public static Task symlink(string target, string path, string? type = null)
    {
        return Task.Run(() => symlinkSync(target, path, type));
    }
}
