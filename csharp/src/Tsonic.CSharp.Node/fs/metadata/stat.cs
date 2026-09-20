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
                return new Stats(fileInfo.LastAccessTime, fileInfo.LastWriteTime, fileInfo.CreationTime, fileInfo.CreationTime)
                {
                    size = fileInfo.Length,
                    mode = 0,
                    isFile = true,
                    isDirectory = false
                };
            }
            else
            {
                return new Stats(dirInfo.LastAccessTime, dirInfo.LastWriteTime, dirInfo.CreationTime, dirInfo.CreationTime)
                {
                    size = 0,
                    mode = 0,
                    isFile = false,
                    isDirectory = true
                };
            }
        });
    }
}
