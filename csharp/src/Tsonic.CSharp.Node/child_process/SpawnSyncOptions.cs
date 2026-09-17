namespace Tsonic.CSharp.Node;

/// <summary>Closed options for synchronous Buffer-based process execution.</summary>
public sealed class SpawnSyncOptions<TInput, TStdio> where TInput : class where TStdio : class
{
    /// <summary>The binary output encoding, when explicitly selected.</summary>
    public string? encoding { get; set; }
    /// <summary>The child's working directory.</summary>
    public string? cwd { get; set; }
    /// <summary>The complete child environment; omission inherits the parent's environment.</summary>
    public ProcessEnv? env { get; set; }
    /// <summary>The maximum captured bytes on each output stream.</summary>
    public double? maxBuffer { get; set; }
    /// <summary>The timeout in milliseconds.</summary>
    public double? timeout { get; set; }
    /// <summary>The requested termination signal, unsupported by the .NET launcher.</summary>
    public string? killSignal { get; set; }
    /// <summary>The requested numeric user identity, unsupported by the .NET launcher.</summary>
    public double? uid { get; set; }
    /// <summary>The requested numeric group identity, unsupported by the .NET launcher.</summary>
    public double? gid { get; set; }
    /// <summary>The exact binary input view, retained until invocation.</summary>
    public TInput? input { get; set; }
    /// <summary>The selected standard descriptors, retained by reference until invocation.</summary>
    public TStdio? stdio { get; set; }
}
