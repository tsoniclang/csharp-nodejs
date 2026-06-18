namespace Tsonic.CSharp.Node;

public static partial class fs
{
    /// <summary>
    /// Synchronously copies src to dest. By default, dest is overwritten if it already exists.
    /// </summary>
    /// <param name="src">Source filename to copy.</param>
    /// <param name="dest">Destination filename.</param>
    /// <param name="mode">Optional flags (not yet implemented).</param>
    public static void copyFileSync(string src, string dest, int mode = 0)
    {
        File.Copy(src, dest, overwrite: true);
    }
}
