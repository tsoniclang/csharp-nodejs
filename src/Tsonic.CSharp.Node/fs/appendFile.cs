using System.Text;
using System.Threading.Tasks;

namespace Tsonic.CSharp.Node;

public static partial class fs
{
    /// <summary>
    /// Asynchronously appends data to a file, creating the file if it does not yet exist.
    /// </summary>
    /// <param name="path">Filename or file path.</param>
    /// <param name="data">The data to append.</param>
    /// <param name="encoding">Character encoding (default: "utf-8").</param>
    /// <returns>A promise that resolves when the append is complete.</returns>
    public static async Task appendFile(string path, string data, string? encoding = "utf-8")
    {
        var enc = ParseEncoding(encoding ?? "utf-8");
        await File.AppendAllTextAsync(path, data, enc);
    }
}
