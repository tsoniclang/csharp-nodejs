using System.Text;

namespace Tsonic.CSharp.Node;

public static partial class fs
{
    /// <summary>
    /// Synchronously reads the entire contents of a file into a Buffer.
    /// </summary>
    /// <param name="path">Filename or file path.</param>
    /// <returns>The contents of the file as a Buffer.</returns>
    public static Buffer readFileSync(string path)
    {
        return Buffer.from(File.ReadAllBytes(path));
    }

    /// <summary>
     /// Synchronously reads the entire contents of a file.
     /// </summary>
     /// <param name="path">Filename or file path.</param>
    /// <param name="encoding">Character encoding (e.g., "utf-8").</param>
    /// <returns>The contents of the file as a string.</returns>
    public static string readFileSync(string path, string encoding)
    {
        var enc = ParseEncoding(encoding);
        return File.ReadAllText(path, enc);
    }
}
