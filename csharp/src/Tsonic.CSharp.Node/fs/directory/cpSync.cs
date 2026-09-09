namespace Tsonic.CSharp.Node;

public static partial class fs
{
    /// <summary>
    /// Synchronously copies files and directories (modern API).
    /// </summary>
    /// <param name="src">The source path.</param>
    /// <param name="dest">The destination path.</param>
    /// <param name="recursive">If true, copies directories recursively.</param>
    public static void cpSync(string src, string dest, bool recursive = false)
    {
        if (File.Exists(src))
        {
            File.Copy(src, dest, overwrite: true);
        }
        else if (Directory.Exists(src))
        {
            if (!recursive)
            {
                throw new IOException($"Cannot copy directory without recursive flag: {src}");
            }

            // Recursively copy directory
            CopyDirectory(src, dest);
        }
        else
        {
            throw new FileNotFoundException($"No such file or directory: {src}");
        }
    }
}
