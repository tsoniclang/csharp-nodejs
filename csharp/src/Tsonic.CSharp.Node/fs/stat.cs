using System.Threading.Tasks;

namespace Tsonic.CSharp.Node;

public static partial class fs
{
    /// <summary>
    /// Asynchronously retrieves statistics for the file/directory at the given path.
    /// </summary>
    /// <param name="path">The file or directory path.</param>
    /// <returns>A promise that resolves to a Stats object.</returns>
    public static Task<Stats> stat(string path)
    {
        return Task.Run(() =>
        {
            var fileInfo = new FileInfo(path);
            var dirInfo = new DirectoryInfo(path);

            var isFile = fileInfo.Exists;
            var isDir = dirInfo.Exists;

            if (!isFile && !isDir)
            {
                throw new FileNotFoundException($"No such file or directory: {path}");
            }

            if (isFile)
            {
                return new Stats
                {
                    size = fileInfo.Length,
                    mode = 0,
                    atime = StatTime.ToJsDate(fileInfo.LastAccessTime),
                    atimeMs = StatTime.ToUnixMilliseconds(fileInfo.LastAccessTime),
                    mtime = StatTime.ToJsDate(fileInfo.LastWriteTime),
                    mtimeMs = StatTime.ToUnixMilliseconds(fileInfo.LastWriteTime),
                    ctime = StatTime.ToJsDate(fileInfo.CreationTime),
                    ctimeMs = StatTime.ToUnixMilliseconds(fileInfo.CreationTime),
                    birthtime = StatTime.ToJsDate(fileInfo.CreationTime),
                    birthtimeMs = StatTime.ToUnixMilliseconds(fileInfo.CreationTime),
                    isFile = true,
                    isDirectory = false
                };
            }
            else
            {
                return new Stats
                {
                    size = 0,
                    mode = 0,
                    atime = StatTime.ToJsDate(dirInfo.LastAccessTime),
                    atimeMs = StatTime.ToUnixMilliseconds(dirInfo.LastAccessTime),
                    mtime = StatTime.ToJsDate(dirInfo.LastWriteTime),
                    mtimeMs = StatTime.ToUnixMilliseconds(dirInfo.LastWriteTime),
                    ctime = StatTime.ToJsDate(dirInfo.CreationTime),
                    ctimeMs = StatTime.ToUnixMilliseconds(dirInfo.CreationTime),
                    birthtime = StatTime.ToJsDate(dirInfo.CreationTime),
                    birthtimeMs = StatTime.ToUnixMilliseconds(dirInfo.CreationTime),
                    isFile = false,
                    isDirectory = true
                };
            }
        });
    }
}
