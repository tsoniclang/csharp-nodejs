namespace Tsonic.CSharp.Node;

public static partial class fs
{
    /// <summary>
    /// Synchronously copies src to dest. By default, dest is overwritten if it already exists.
    /// </summary>
    /// <param name="src">Source filename to copy.</param>
    /// <param name="dest">Destination filename.</param>
    /// <param name="mode">Optional COPYFILE_* flags.</param>
    public static void copyFileSync(string src, string dest, int mode = 0)
    {
        if ((mode & constants.COPYFILE_FICLONE_FORCE) != 0)
        {
            throw new NotSupportedException("COPYFILE_FICLONE_FORCE requires copy-on-write clone support, which is not exposed by the portable .NET file API.");
        }

        var overwrite = (mode & constants.COPYFILE_EXCL) == 0;
        File.Copy(src, dest, overwrite);
    }
}
