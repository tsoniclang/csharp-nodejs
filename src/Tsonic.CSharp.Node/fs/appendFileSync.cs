using System;
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

    /// <summary>
    /// Synchronously appends buffer data to a file, creating the file if it does not yet exist.
    /// </summary>
    /// <param name="path">Filename or file path.</param>
    /// <param name="data">The buffer data to append.</param>
    public static void appendFileSync(string path, Buffer data)
    {
        if (data == null)
            throw new ArgumentNullException(nameof(data));

        using var stream = new FileStream(path, FileMode.Append, FileAccess.Write, FileShare.Read);
        stream.Write(data.InternalData, 0, data.length);
    }
}
