using System.Text;
using System.Threading.Tasks;

namespace Tsonic.CSharp.Node;

public static partial class fs
{
    /// <summary>
    /// Asynchronously reads the entire contents of a file into a Buffer.
    /// </summary>
    /// <param name="path">Filename or file path.</param>
    /// <returns>A promise that resolves to the contents of the file as a Buffer.</returns>
    public static async Task<Buffer> readFile(string path)
    {
        return Buffer.from(await File.ReadAllBytesAsync(path));
    }

    /// <summary>
     /// Asynchronously reads the entire contents of a file.
     /// </summary>
     /// <param name="path">Filename or file path.</param>
    /// <param name="encoding">Character encoding (e.g., "utf-8").</param>
    /// <returns>A promise that resolves to the contents of the file as a string.</returns>
    public static async Task<string> readFile(string path, string encoding)
    {
        var enc = ParseEncoding(encoding);
        return await File.ReadAllTextAsync(path, enc);
    }
}
