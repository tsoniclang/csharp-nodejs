using System;
using System.IO;

namespace Tsonic.CSharp.Node;

public static partial class fs
{
    /// <summary>
    /// Synchronously retrieves statistics for a file descriptor.
    /// </summary>
    /// <param name="fd">The file descriptor.</param>
    /// <returns>A Stats object.</returns>
    public static Stats fstatSync(int fd)
    {
        var stream = FileDescriptorManager.Get(fd);
        if (stream == null)
            throw new ArgumentException($"Bad file descriptor: {fd}", nameof(fd));

        // Get FileInfo from the stream's name
        var fileInfo = new FileInfo(stream.Name);

        if (!fileInfo.Exists)
            throw new FileNotFoundException($"File not found", stream.Name);

        return new Stats
        {
            size = fileInfo.Length,
            mode = Convert.ToInt32("666", 8), // 0o666 in octal = 438 decimal
            atime = StatTime.ToJsDate(fileInfo.LastAccessTime),
            atimeMs = StatTime.ToUnixMilliseconds(fileInfo.LastAccessTime),
            mtime = StatTime.ToJsDate(fileInfo.LastWriteTime),
            mtimeMs = StatTime.ToUnixMilliseconds(fileInfo.LastWriteTime),
            ctime = StatTime.ToJsDate(fileInfo.LastWriteTime),
            ctimeMs = StatTime.ToUnixMilliseconds(fileInfo.LastWriteTime),
            birthtime = StatTime.ToJsDate(fileInfo.CreationTime),
            birthtimeMs = StatTime.ToUnixMilliseconds(fileInfo.CreationTime),
            isFile = !fileInfo.Attributes.HasFlag(FileAttributes.Directory),
            isDirectory = fileInfo.Attributes.HasFlag(FileAttributes.Directory)
        };
    }
}
