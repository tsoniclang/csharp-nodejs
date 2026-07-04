using System.Threading.Tasks;

namespace Tsonic.CSharp.Node;

public static partial class fs
{
    /// <summary>
    /// Asynchronously deletes a file.
    /// </summary>
    /// <param name="path">The file path to delete.</param>
    /// <returns>A promise that resolves when the file is deleted.</returns>
    public static Task unlink(string path)
    {
        return Task.Run(() => File.Delete(path));
    }
}
