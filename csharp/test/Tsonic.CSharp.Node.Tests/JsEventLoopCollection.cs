using Xunit;

namespace Tsonic.CSharp.Node.Tests;

[CollectionDefinition(Name, DisableParallelization = true)]
public sealed class JsEventLoopCollection
{
    public const string Name = "JavaScript event loop";
}
