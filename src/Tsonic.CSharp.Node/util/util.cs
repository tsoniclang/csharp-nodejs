namespace Tsonic.CSharp.Node;

/// <summary>
/// The util module supports the needs of Node.js internal APIs.
/// Many of the utilities are useful for application and module developers as well.
/// </summary>
public static partial class util
{
    private static NotSupportedException UnsupportedOpenCarrierOperation(string apiName)
    {
        return new NotSupportedException($"{apiName} requires closed provider/runtime carrier semantics and is not available in the current C# Node runtime package.");
    }
}
