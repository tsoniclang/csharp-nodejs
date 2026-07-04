namespace Tsonic.CSharp.Node;

#pragma warning disable CS1591

public sealed class FsConstants
{
    public int F_OK { get; set; } = 0;
    public int R_OK { get; set; } = 4;
    public int W_OK { get; set; } = 2;
    public int X_OK { get; set; } = 1;
    public int COPYFILE_EXCL { get; set; } = 1;
    public int COPYFILE_FICLONE { get; set; } = 2;
    public int COPYFILE_FICLONE_FORCE { get; set; } = 4;
    public int O_RDONLY { get; set; } = 0;
    public int O_WRONLY { get; set; } = 1;
    public int O_RDWR { get; set; } = 2;
    public int O_CREAT { get; set; } = 0x40;
    public int O_EXCL { get; set; } = 0x80;
    public int O_TRUNC { get; set; } = 0x200;
    public int O_APPEND { get; set; } = 0x400;
    public int O_DIRECTORY { get; set; } = 0x10000;
    public int O_NOFOLLOW { get; set; } = 0x20000;
    public int O_SYNC { get; set; } = 0x101000;
    public int S_IFMT { get; set; } = 0xF000;
    public int S_IFREG { get; set; } = 0x8000;
    public int S_IFDIR { get; set; } = 0x4000;
    public int S_IFCHR { get; set; } = 0x2000;
    public int S_IFBLK { get; set; } = 0x6000;
    public int S_IFIFO { get; set; } = 0x1000;
    public int S_IFLNK { get; set; } = 0xA000;
    public int S_IFSOCK { get; set; } = 0xC000;
    public int S_IRWXU { get; set; } = 0x1C0;
    public int S_IRUSR { get; set; } = 0x100;
    public int S_IWUSR { get; set; } = 0x80;
    public int S_IXUSR { get; set; } = 0x40;
    public int S_IRWXG { get; set; } = 0x38;
    public int S_IRGRP { get; set; } = 0x20;
    public int S_IWGRP { get; set; } = 0x10;
    public int S_IXGRP { get; set; } = 0x8;
    public int S_IRWXO { get; set; } = 0x7;
    public int S_IROTH { get; set; } = 0x4;
    public int S_IWOTH { get; set; } = 0x2;
    public int S_IXOTH { get; set; } = 0x1;
    public int UV_FS_O_FILEMAP { get; set; }
}

public static partial class fs
{
    private static readonly FsConstants ConstantsValue = new();
    public static FsConstants constants => ConstantsValue;
}
