using System.Threading.Tasks;

namespace Tsonic.CSharp.Node;

public static partial class fs
{
    /// <summary>
    /// Asynchronously copies files and directories (modern API).
    /// </summary>
    /// <param name="src">The source path.</param>
    /// <param name="dest">The destination path.</param>
    /// <param name="recursive">If true, copies directories recursively.</param>
    /// <returns>A promise that resolves when the copy is complete.</returns>
    public static Task cp(string src, string dest, bool recursive = false)
    {
        return Task.Run(() => cpSync(src, dest, recursive));
    }
}
