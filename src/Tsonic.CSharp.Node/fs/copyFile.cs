using System.Threading.Tasks;

namespace Tsonic.CSharp.Node;

public static partial class fs
{
    /// <summary>
    /// Asynchronously copies src to dest. By default, dest is overwritten if it already exists.
    /// </summary>
    /// <param name="src">Source filename to copy.</param>
    /// <param name="dest">Destination filename.</param>
    /// <param name="mode">Optional flags (not yet implemented).</param>
    /// <returns>A promise that resolves when the copy is complete.</returns>
    public static Task copyFile(string src, string dest, int mode = 0)
    {
        return Task.Run(() => File.Copy(src, dest, overwrite: true));
    }
}
