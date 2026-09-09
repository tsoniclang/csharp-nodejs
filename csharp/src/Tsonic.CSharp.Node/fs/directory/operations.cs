namespace Tsonic.CSharp.Node;

#pragma warning disable CS1591

public static partial class fs
{
    public static string mkdtempSync(string prefix)
    {
        var path = prefix + Path.GetRandomFileName();
        Directory.CreateDirectory(path);
        return path;
    }

    public static Task<string> mkdtemp(string prefix, Action<Exception?, string>? callback = null)
    {
        try
        {
            var value = mkdtempSync(prefix);
            callback?.Invoke(null, value);
            return Task.FromResult(value);
        }
        catch (Exception ex)
        {
            callback?.Invoke(ex, string.Empty);
            return Task.FromException<string>(ex);
        }
    }

    public static DisposableTempDir mkdtempDisposableSync(string prefix) => new() { path = mkdtempSync(prefix) };
    public static Task<DisposableTempDir> mkdtempDisposable(string prefix) => Task.FromResult(mkdtempDisposableSync(prefix));

    public static Dirent[] readdirDirentsSync(string path)
    {
        return Directory.GetFileSystemEntries(path)
            .Select(entry => new Dirent
            {
                name = Path.GetFileName(entry),
                parentPath = path,
                fileType = Directory.Exists(entry) ? "directory" : File.GetAttributes(entry).HasFlag(FileAttributes.ReparsePoint) ? "symlink" : "file"
            })
            .ToArray();
    }

    public static Dir opendirSync(string path) => new(path);
    public static Task<Dir> opendir(string path) => Task.FromResult(opendirSync(path));
    public static string[] globSync(string pattern) => Directory.GetFiles(Environment.CurrentDirectory, pattern, SearchOption.AllDirectories);
    public static Task<string[]> glob(string pattern) => Task.FromResult(globSync(pattern));
}
