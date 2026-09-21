namespace Tsonic.CSharp.Node;

public static partial class fs
{
    private static Stats ReadStats(string path, bool followLink)
    {
        FileSystemInfo info = new FileInfo(path);
        var attributes = info.Attributes;
        if (attributes == (FileAttributes)(-1))
            throw new FileNotFoundException($"No such file or directory: {path}", path);

        var symbolicLink = (attributes & FileAttributes.ReparsePoint) != 0;
        if (symbolicLink && followLink)
        {
            info = info.ResolveLinkTarget(returnFinalTarget: true)
                ?? throw new FileNotFoundException($"No such link target: {path}", path);
            attributes = info.Attributes;
            if (attributes == (FileAttributes)(-1))
                throw new FileNotFoundException($"No such link target: {path}", path);
            symbolicLink = false;
        }

        var directory = (attributes & FileAttributes.Directory) != 0;
        var size = symbolicLink
            ? System.Text.Encoding.UTF8.GetByteCount(info.LinkTarget ?? string.Empty)
            : directory ? 0L : ((FileInfo)info).Length;
        return new Stats(info.LastAccessTimeUtc, info.LastWriteTimeUtc, info.CreationTimeUtc, info.CreationTimeUtc)
        {
            size = size,
            mode = 0,
            isFile = !symbolicLink && !directory,
            isDirectory = !symbolicLink && directory,
            isSymbolicLink = symbolicLink
        };
    }
}
