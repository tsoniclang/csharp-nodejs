namespace Tsonic.CSharp.Node;

#pragma warning disable CS1591

public sealed class ReadVResult
{
    public int bytesRead { get; set; }
    public byte[][] buffers { get; set; } = [];
}

public sealed class WriteVResult
{
    public int bytesWritten { get; set; }
    public byte[][] buffers { get; set; } = [];
}
