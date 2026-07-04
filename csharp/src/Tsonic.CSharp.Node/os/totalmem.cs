namespace Tsonic.CSharp.Node;

public static partial class os
{
    /// <summary>
    /// Returns the total amount of system memory in bytes as an integer.
    /// </summary>
    /// <returns>The total system memory in bytes.</returns>
    public static long totalmem()
    {
        return GC.GetGCMemoryInfo().TotalAvailableMemoryBytes;
    }
}
