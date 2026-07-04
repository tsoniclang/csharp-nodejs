namespace Tsonic.CSharp.Node;

public static partial class os
{
    /// <summary>
    /// Returns the amount of free system memory in bytes as an integer.
    /// </summary>
    /// <returns>The free system memory in bytes.</returns>
    public static long freemem()
    {
        var gcInfo = GC.GetGCMemoryInfo();
        // This returns an approximation of free memory
        return gcInfo.TotalAvailableMemoryBytes - gcInfo.MemoryLoadBytes;
    }
}
