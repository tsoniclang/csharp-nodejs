using System.Text;

namespace Tsonic.CSharp.Node;

public static partial class fs
{
    /// <summary>
    /// Synchronously appends data to a file, creating the file if it does not yet exist.
    /// </summary>
    /// <param name="path">Filename or file path.</param>
    /// <param name="data">The data to append.</param>
    /// <param name="encoding">Character encoding (default: "utf-8").</param>
    public static void appendFileSync(string path, string data, string? encoding = "utf-8")
    {
        var enc = ParseEncoding(encoding ?? "utf-8");
        File.AppendAllText(path, data, enc);
    }
}
