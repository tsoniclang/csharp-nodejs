namespace Tsonic.CSharp.Node;

public static partial class fs
{
    /// <summary>
    /// Synchronously writes a byte array to a file, replacing the file if it already exists.
    /// </summary>
    /// <param name="path">Filename or file path.</param>
    /// <param name="data">The byte array to write.</param>
    public static void writeFileSyncBytes(string path, byte[] data)
    {
        File.WriteAllBytes(path, data);
    }
}
