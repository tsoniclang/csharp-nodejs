namespace Tsonic.CSharp.Node;

#pragma warning disable CS1591

public sealed class ReadStreamOptions
{
    public string? flags { get; set; }
    public string? encoding { get; set; }
    public long? start { get; set; }
    public long? end { get; set; }
    public int? highWaterMark { get; set; }
}

public sealed class WriteStreamOptions
{
    public string? flags { get; set; }
    public string? encoding { get; set; }
    public long? start { get; set; }
    public int? highWaterMark { get; set; }
    public bool? flush { get; set; }
}
