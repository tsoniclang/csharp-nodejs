using System.Threading.Tasks;

namespace Tsonic.CSharp.Node;

public static partial class fs
{
    /// <summary>
    /// Asynchronously retrieves statistics for the file/directory at the given path.
    /// </summary>
    /// <param name="path">The file or directory path.</param>
    /// <returns>A promise that resolves to a Stats object.</returns>
    public static Task<Stats> stat(string path)
    {
        return Task.Run(() => statSync(path));
    }
}
