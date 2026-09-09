namespace Tsonic.CSharp.Node;

#pragma warning disable CS1591

public sealed class FsWatchEvent
{
    public string eventType { get; set; } = string.Empty;
    public string? filename { get; set; }
}

public sealed class WatchOptions
{
    public bool? persistent { get; set; }
    public bool? recursive { get; set; }
}
