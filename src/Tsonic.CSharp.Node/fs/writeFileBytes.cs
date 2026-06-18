using System.Threading.Tasks;

namespace Tsonic.CSharp.Node;

public static partial class fs
{
    /// <summary>
    /// Asynchronously writes a byte array to a file, replacing the file if it already exists.
    /// </summary>
    /// <param name="path">Filename or file path.</param>
    /// <param name="data">The byte array to write.</param>
    /// <returns>A promise that resolves when the write is complete.</returns>
    public static async Task writeFileBytes(string path, byte[] data)
    {
        await File.WriteAllBytesAsync(path, data);
    }
}
