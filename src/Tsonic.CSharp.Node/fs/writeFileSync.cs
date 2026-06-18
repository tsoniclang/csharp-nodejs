using System.Text;

namespace Tsonic.CSharp.Node;

public static partial class fs
{
    /// <summary>
    /// Synchronously writes data to a file, replacing the file if it already exists.
    /// </summary>
    /// <param name="path">Filename or file path.</param>
    /// <param name="data">The data to write.</param>
    /// <param name="encoding">Character encoding (default: "utf-8").</param>
    public static void writeFileSync(string path, string data, string? encoding = "utf-8")
    {
        var enc = ParseEncoding(encoding ?? "utf-8");
        File.WriteAllText(path, data, enc);
    }
}
