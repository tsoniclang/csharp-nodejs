namespace Tsonic.CSharp.Node;

/// <summary>
/// Information about a logical CPU core.
/// </summary>
public class CpuInfo
{
    /// <summary>
    /// The CPU model name.
    /// </summary>
    public string model { get; set; } = string.Empty;

    /// <summary>
    /// The CPU speed in MHz.
    /// </summary>
    public int speed { get; set; }

    /// <summary>
    /// CPU time statistics.
    /// </summary>
    public CpuTimes times { get; set; } = new CpuTimes();
}

/// <summary>
/// CPU time statistics in milliseconds.
/// </summary>
public class CpuTimes
{
    /// <summary>
    /// The number of milliseconds the CPU has spent in user mode.
    /// </summary>
    public long user { get; set; }

    /// <summary>
    /// The number of milliseconds the CPU has spent in nice mode (POSIX only, always 0 on Windows).
    /// </summary>
    public long nice { get; set; }

    /// <summary>
    /// The number of milliseconds the CPU has spent in sys mode.
    /// </summary>
    public long sys { get; set; }

    /// <summary>
    /// The number of milliseconds the CPU has spent in idle mode.
    /// </summary>
    public long idle { get; set; }

    /// <summary>
    /// The number of milliseconds the CPU has spent in irq mode.
    /// </summary>
    public long irq { get; set; }
}

public static partial class os
{
    /// <summary>
    /// Returns an array of objects containing information about each logical CPU core.
    /// </summary>
    /// <returns>An array of CpuInfo objects.</returns>
    public static CpuInfo[] cpus()
    {
        var processorCount = Environment.ProcessorCount;
        var cpuInfos = new CpuInfo[processorCount];

        for (int i = 0; i < processorCount; i++)
        {
            cpuInfos[i] = new CpuInfo
            {
                model = "Unknown CPU",
                speed = 0,
                times = new CpuTimes
                {
                    user = 0,
                    nice = 0,
                    sys = 0,
                    idle = 0,
                    irq = 0
                }
            };
        }

        return cpuInfos;
    }
}
