using System.ComponentModel;
using System.Numerics;
using System.Runtime.InteropServices;
using Tsonic.CSharp.Runtime;

namespace Tsonic.CSharp.Node;

public static partial class process
{
    /// <summary>Sends the exact native signal, or checks existence and permission for signal zero.</summary>
    public static bool kill(int pid, int? signal = null)
    {
        var signalNumber = signal ?? 15;
        if (signalNumber < 0)
            throw new ArgumentException("ERR_UNKNOWN_SIGNAL: signal must be nonnegative.", nameof(signal));
        if (!OperatingSystem.IsLinux() && !OperatingSystem.IsMacOS())
            throw new PlatformNotSupportedException("ERR_FEATURE_UNAVAILABLE: native process signals require Linux or macOS.");
        if (killNative(pid, signalNumber) == 0)
            return true;

        var error = Marshal.GetLastPInvokeError();
        if (error == 22)
            throw new ArgumentException($"ERR_UNKNOWN_SIGNAL: unknown signal number {signalNumber}.", nameof(signal));
        var code = error switch
        {
            1 => "EPERM",
            3 => "ESRCH",
            _ => $"errno {error}"
        };
        throw new Win32Exception(error, $"kill {code}: {new Win32Exception(error).Message}; pid {pid}, signal {signalNumber}.");
    }

    /// <summary>Sends the default signal after an exact native process-ID conversion.</summary>
    public static bool kill<TPid>(TPid pid) where TPid : INumberBase<TPid>
        => kill(processSignalInteger(pid, nameof(pid)));

    /// <summary>Sends a named native signal after an exact process-ID conversion.</summary>
    public static bool killNamed<TPid>(TPid pid, string signal) where TPid : INumberBase<TPid>
        => kill(processSignalInteger(pid, nameof(pid)), processSignalNumber(signal));

    /// <summary>Sends a numeric native signal after exact process-ID and signal conversions.</summary>
    public static bool killNumber<TPid, TSignal>(TPid pid, TSignal signal)
        where TPid : INumberBase<TPid>
        where TSignal : INumberBase<TSignal>
        => kill(processSignalInteger(pid, nameof(pid)), processSignalInteger(signal, nameof(signal)));

    private static int processSignalInteger<TNumber>(TNumber value, string parameterName)
        where TNumber : INumberBase<TNumber>
    {
        try
        {
            return IntegerConversions.Checked<TNumber, int>(value);
        }
        catch (OverflowException exception)
        {
            throw new ArgumentException($"ERR_INVALID_ARG_TYPE: {parameterName} must be a finite signed 32-bit integer.", parameterName, exception);
        }
    }

    private static int processSignalNumber(string signal)
    {
        var linux = OperatingSystem.IsLinux();
        var macOS = OperatingSystem.IsMacOS();
        if (!linux && !macOS)
            throw new PlatformNotSupportedException("ERR_FEATURE_UNAVAILABLE: native process signals require Linux or macOS.");
        return signal switch
        {
            "SIGHUP" => 1,
            "SIGINT" => 2,
            "SIGQUIT" => 3,
            "SIGILL" => 4,
            "SIGTRAP" => 5,
            "SIGABRT" or "SIGIOT" => 6,
            "SIGBUS" => linux ? 7 : 10,
            "SIGFPE" => 8,
            "SIGKILL" => 9,
            "SIGUSR1" => linux ? 10 : 30,
            "SIGSEGV" => 11,
            "SIGUSR2" => linux ? 12 : 31,
            "SIGPIPE" => 13,
            "SIGALRM" => 14,
            "SIGTERM" => 15,
            "SIGSTKFLT" when linux => 16,
            "SIGCHLD" => linux ? 17 : 20,
            "SIGCONT" => linux ? 18 : 19,
            "SIGSTOP" => linux ? 19 : 17,
            "SIGTSTP" => linux ? 20 : 18,
            "SIGTTIN" => 21,
            "SIGTTOU" => 22,
            "SIGURG" => linux ? 23 : 16,
            "SIGXCPU" => 24,
            "SIGXFSZ" => 25,
            "SIGVTALRM" => 26,
            "SIGPROF" => 27,
            "SIGWINCH" => 28,
            "SIGIO" or "SIGPOLL" => linux ? 29 : 23,
            "SIGPWR" when linux => 30,
            "SIGSYS" => linux ? 31 : 12,
            "SIGUNUSED" when linux => 31,
            "SIGINFO" when macOS => 29,
            _ => throw new ArgumentException($"ERR_UNKNOWN_SIGNAL: unknown signal '{signal}'.", nameof(signal))
        };
    }

    [DllImport("libc", EntryPoint = "kill", SetLastError = true)]
    private static extern int killNative(int pid, int signal);
}
