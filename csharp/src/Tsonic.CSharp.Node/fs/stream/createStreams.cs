namespace Tsonic.CSharp.Node;

#pragma warning disable CS1591

public static partial class fs
{
    public static ReadStream createReadStream(string path, ReadStreamOptions? options = null) => new(path, options);
    public static WriteStream createWriteStream(string path, WriteStreamOptions? options = null) => new(path, options);
}
