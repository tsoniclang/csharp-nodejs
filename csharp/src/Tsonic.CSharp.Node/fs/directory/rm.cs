using System.Threading.Tasks;

namespace Tsonic.CSharp.Node;

public static partial class fs
{
    /// <summary>
    /// Asynchronously removes files and directories (modern API).
    /// </summary>
    /// <param name="path">The path to remove.</param>
    /// <returns>A promise that resolves when the removal is complete.</returns>
    public static Task rm(string path)
    {
        return Task.Run(() => rmSync(path));
    }

    /// <summary>
    /// Asynchronously removes a file or directory using the exact Node options contract.
    /// </summary>
    /// <param name="path">The path to remove.</param>
    /// <param name="options">Removal options.</param>
    /// <returns>A promise that resolves when the removal is complete.</returns>
    public static Task rm(string path, RmOptions options)
    {
        return Task.Run(() => rmSync(path, options));
    }
}
