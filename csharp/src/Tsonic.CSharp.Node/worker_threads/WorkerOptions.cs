using Tsonic.CSharp.Js;
using Tsonic.CSharp.Runtime;

namespace Tsonic.CSharp.Node;

public sealed class WorkerOptions
{
    public string? name { get; set; }
    public JSArray<string>? argv { get; set; }
    public TsValue env { get; set; } = TsValue.undefined();
    public TsValue workerData { get; set; } = TsValue.undefined();
}
