namespace Tsonic.CSharp.Node;

public static partial class os
{
    /// <summary>
    /// Returns an estimate of the default amount of parallelism a program should use.
    /// Always returns a value greater than zero.
    /// </summary>
    /// <returns>The number of available parallel workers.</returns>
    public static int availableParallelism()
    {
        return Environment.ProcessorCount;
    }
}
