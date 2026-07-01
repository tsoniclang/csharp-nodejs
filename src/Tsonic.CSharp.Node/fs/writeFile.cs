using System.Text;
using System.Threading.Tasks;

namespace Tsonic.CSharp.Node;

public static partial class fs
{
    /// <summary>
    /// Asynchronously writes data to a file, replacing the file if it already exists.
    /// </summary>
    /// <param name="path">Filename or file path.</param>
    /// <param name="data">The data to write.</param>
    /// <param name="encoding">Character encoding (default: "utf-8").</param>
    /// <returns>A promise that resolves when the write is complete.</returns>
    public static async Task writeFile(string path, string data, string? encoding = "utf-8")
    {
        var enc = ParseEncoding(encoding ?? "utf-8");
        await File.WriteAllTextAsync(path, data, enc);
    }

    /// <summary>
    /// Asynchronously writes buffer data to a file, replacing the file if it already exists.
    /// </summary>
    /// <param name="path">Filename or file path.</param>
    /// <param name="data">The buffer data to write.</param>
    /// <returns>A promise that resolves when the write is complete.</returns>
    public static async Task writeFile(string path, Buffer data)
    {
        if (data == null)
            throw new ArgumentNullException(nameof(data));

        await File.WriteAllBytesAsync(path, data.InternalData);
    }
}
