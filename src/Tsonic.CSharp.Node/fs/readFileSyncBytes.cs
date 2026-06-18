namespace Tsonic.CSharp.Node;

public static partial class fs
{
    /// <summary>
    /// Synchronously reads the entire contents of a file as a byte array.
    /// </summary>
    /// <param name="path">Filename or file path.</param>
    /// <returns>The contents of the file as a byte array.</returns>
    public static byte[] readFileSyncBytes(string path)
    {
        return File.ReadAllBytes(path);
    }
}
