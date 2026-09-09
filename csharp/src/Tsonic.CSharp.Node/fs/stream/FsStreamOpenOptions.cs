namespace Tsonic.CSharp.Node;

internal readonly record struct FsStreamOpenOptions(
    FileMode Mode,
    FileAccess Access,
    FileOptions Options,
    bool Append)
{
    public static FsStreamOpenOptions ForRead(string flags) => flags switch
    {
        "r" => new(FileMode.Open, FileAccess.Read, FileOptions.SequentialScan, false),
        "r+" => new(FileMode.Open, FileAccess.ReadWrite, FileOptions.RandomAccess, false),
        "rs+" => new(
            FileMode.Open,
            FileAccess.ReadWrite,
            FileOptions.RandomAccess | FileOptions.WriteThrough,
            false),
        _ => throw new ArgumentException($"Unsupported ReadStream flag: {flags}", nameof(flags)),
    };

    public static FsStreamOpenOptions ForWrite(string flags) => flags switch
    {
        "w" => new(FileMode.Create, FileAccess.Write, FileOptions.SequentialScan, false),
        "wx" => new(FileMode.CreateNew, FileAccess.Write, FileOptions.SequentialScan, false),
        "w+" => new(FileMode.Create, FileAccess.ReadWrite, FileOptions.RandomAccess, false),
        "wx+" => new(FileMode.CreateNew, FileAccess.ReadWrite, FileOptions.RandomAccess, false),
        "a" => new(FileMode.OpenOrCreate, FileAccess.Write, FileOptions.SequentialScan, true),
        "ax" => new(FileMode.CreateNew, FileAccess.Write, FileOptions.SequentialScan, true),
        "a+" => new(FileMode.OpenOrCreate, FileAccess.ReadWrite, FileOptions.RandomAccess, true),
        "ax+" => new(FileMode.CreateNew, FileAccess.ReadWrite, FileOptions.RandomAccess, true),
        "as" => new(
            FileMode.OpenOrCreate,
            FileAccess.Write,
            FileOptions.SequentialScan | FileOptions.WriteThrough,
            true),
        "as+" => new(
            FileMode.OpenOrCreate,
            FileAccess.ReadWrite,
            FileOptions.RandomAccess | FileOptions.WriteThrough,
            true),
        _ => throw new ArgumentException($"Unsupported WriteStream flag: {flags}", nameof(flags)),
    };
}
