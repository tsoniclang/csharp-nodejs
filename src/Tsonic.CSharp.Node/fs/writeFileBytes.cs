using System.Threading.Tasks;

namespace Tsonic.CSharp.Node;

public static partial class fs
{
    /// <summary>
    /// Asynchronously writes buffer data to a file, replacing the file if it already exists.
    /// </summary>
    /// <param name="path">Filename or file path.</param>
    /// <param name="data">The buffer data to write.</param>
    /// <returns>A promise that resolves when the write is complete.</returns>
    public static Task writeFileBytes(string path, Buffer data) => writeFile(path, data);
}
