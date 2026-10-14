using Tsonic.CSharp.Js;

namespace Tsonic.CSharp.Node;

internal static class NodePhases
{
    internal static readonly PhaseRegistration Background = new();
    internal static readonly PhaseRegistration RuntimeTasks = new();
    internal static readonly PhaseRegistration Signals = new();
    internal static readonly PhaseRegistration Timers = new();
    internal static readonly PhaseRegistration Http = new();
    internal static readonly PhaseRegistration Net = new();
    internal static readonly PhaseRegistration Tls = new();
    internal static readonly PhaseRegistration Watchers = new();
    internal static readonly PhaseRegistration Workers = new();
    internal static readonly PhaseRegistration Ports = new();
    internal static readonly DispatchPhase[] Order = [
        JsEventLoop.TimerPhase, Background.Phase, RuntimeTasks.Phase, Signals.Phase,
        Timers.Phase, Http.Phase, Net.Phase, Tls.Phase, Watchers.Phase, Workers.Phase, Ports.Phase,
    ];
}
