using Xunit;

namespace Tsonic.CSharp.Node.Tests;

/// <summary>
/// Collection definition for perf_hooks tests to ensure they run sequentially.
/// This is necessary because performance.cs uses static global state that is shared
/// across tests. Running tests in parallel causes interference.
/// </summary>
[CollectionDefinition("PerfHooks", DisableParallelization = true)]
public class PerfHooksCollection
{
    // This class is never instantiated, it's just used to define the collection
}
