using System.Threading.Tasks;

namespace Tsonic.CSharp.Node;

public static partial class fs
{
    /// <summary>
    /// Asynchronously reads the entire contents of a file as a byte array.
    /// </summary>
    /// <param name="path">Filename or file path.</param>
    /// <returns>A promise that resolves to the contents of the file as a byte array.</returns>
    public static async Task<byte[]> readFileBytes(string path)
    {
        return await File.ReadAllBytesAsync(path);
    }
}
