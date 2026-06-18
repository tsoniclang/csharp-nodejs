namespace Tsonic.CSharp.Node;

public static partial class fs
{
    /// <summary>
    /// Synchronously deletes a file.
    /// </summary>
    /// <param name="path">The file path to delete.</param>
    public static void unlinkSync(string path)
    {
        File.Delete(path);
    }
}
