namespace Tsonic.CSharp.Node;

/// <summary>The result contract for a V8 heap-statistics observation.</summary>
public sealed class HeapInfo
{
    /// <summary>Total V8 heap bytes.</summary>
    public double total_heap_size { get; set; }
    /// <summary>Executable V8 heap bytes.</summary>
    public double total_heap_size_executable { get; set; }
    /// <summary>Committed V8 heap bytes.</summary>
    public double total_physical_size { get; set; }
    /// <summary>Available V8 heap bytes.</summary>
    public double total_available_size { get; set; }
    /// <summary>Used V8 heap bytes.</summary>
    public double used_heap_size { get; set; }
    /// <summary>V8 heap limit in bytes.</summary>
    public double heap_size_limit { get; set; }
    /// <summary>Bytes allocated through V8 malloc.</summary>
    public double malloced_memory { get; set; }
    /// <summary>Peak bytes allocated through V8 malloc.</summary>
    public double peak_malloced_memory { get; set; }
    /// <summary>Whether V8 overwrites freed memory.</summary>
    public double does_zap_garbage { get; set; }
    /// <summary>Active native V8 contexts.</summary>
    public double number_of_native_contexts { get; set; }
    /// <summary>Detached native V8 contexts.</summary>
    public double number_of_detached_contexts { get; set; }
    /// <summary>Allocated V8 global-handle bytes.</summary>
    public double total_global_handles_size { get; set; }
    /// <summary>Used V8 global-handle bytes.</summary>
    public double used_global_handles_size { get; set; }
    /// <summary>External bytes retained by V8.</summary>
    public double external_memory { get; set; }
    /// <summary>Cumulative V8 allocation bytes.</summary>
    public double total_allocated_bytes { get; set; }
}
