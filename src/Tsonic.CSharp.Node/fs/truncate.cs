using System.Threading.Tasks;

namespace Tsonic.CSharp.Node;

public static partial class fs
{
    /// <summary>
    /// Asynchronously truncates a file to a specified length.
    /// </summary>
    /// <param name="path">The file path.</param>
    /// <param name="len">The desired length in bytes (default: 0).</param>
    /// <returns>A promise that resolves when the file is truncated.</returns>
    public static async Task truncate(string path, long len = 0)
    {
        using var fileStream = new FileStream(path, FileMode.Open, FileAccess.Write);
        fileStream.SetLength(len);
        await Task.CompletedTask;
    }
}
