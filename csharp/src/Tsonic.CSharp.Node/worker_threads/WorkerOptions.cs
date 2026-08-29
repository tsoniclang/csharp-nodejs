using Tsonic.CSharp.Js;
using Tsonic.CSharp.Runtime;

namespace Tsonic.CSharp.Node;

/// <summary>Configures an isolated worker process.</summary>
public sealed class WorkerOptions
{
    /// <summary>Gets or sets the worker thread name.</summary>
    public string? name { get; set; }
    /// <summary>Gets or sets arguments appended to the worker entry arguments.</summary>
    public JSArray<string>? argv { get; set; }
    /// <summary>Gets or sets the worker's closed string-valued environment.</summary>
    public TsValue env { get; set; } = TsValue.undefined();
    /// <summary>Gets or sets the structured-cloned startup value.</summary>
    public TsValue workerData { get; set; } = TsValue.undefined();
}
