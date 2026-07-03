using System.Collections.Generic;
using System.Diagnostics;

namespace Tsonic.CSharp.Node;

#pragma warning disable CS1591
#pragma warning disable IDE1006

public sealed class MemoryUsage
{
    public long rss { get; set; }
    public long heapTotal { get; set; }
    public long heapUsed { get; set; }
    public long external { get; set; }
    public long arrayBuffers { get; set; }
}

public sealed class CpuUsage
{
    public long user { get; set; }
    public long system { get; set; }
}

public sealed class ResourceUsage
{
    public long userCPUTime { get; set; }
    public long systemCPUTime { get; set; }
    public long maxRSS { get; set; }
    public long sharedMemorySize { get; set; }
    public long unsharedDataSize { get; set; }
    public long unsharedStackSize { get; set; }
    public long minorPageFault { get; set; }
    public long majorPageFault { get; set; }
    public long swappedOut { get; set; }
    public long fsRead { get; set; }
    public long fsWrite { get; set; }
    public long ipcSent { get; set; }
    public long ipcReceived { get; set; }
    public long signalsCount { get; set; }
    public long voluntaryContextSwitches { get; set; }
    public long involuntaryContextSwitches { get; set; }
}

public sealed class ProcessRelease
{
    public string name { get; set; } = "tsonic";
    public string? sourceUrl { get; set; }
    public string? headersUrl { get; set; }
    public string? libUrl { get; set; }
    public string? lts { get; set; }
}

public sealed class ProcessFeatures
{
    public bool debug { get; set; }
    public bool inspector { get; set; }
    public bool ipv6 { get; set; } = true;
    public bool tls { get; set; } = true;
    public bool tlsAlpn { get; set; } = true;
    public bool tlsOcsp { get; set; } = true;
    public bool tlsSni { get; set; } = true;
    public bool uv { get; set; }
    public bool cachedBuiltins { get; set; }
    public bool requireModule { get; set; } = true;
    public bool typescript { get; set; } = true;
}

public sealed class ProcessConfig
{
    public string[] cflags { get; set; } = [];
    public string[] defines { get; set; } = [];
    public string[] includeDirs { get; set; } = [];
    public string[] libraries { get; set; } = [];
    public string defaultConfiguration { get; set; } = "Release";
    public string hostArch { get; set; } = process.arch;
    public string targetArch { get; set; } = process.arch;
    public bool nodeInstallNpm { get; set; }
    public bool nodeInstallWaf { get; set; }
    public string nodePrefix { get; set; } = string.Empty;
    public bool nodeSharedOpenSsl { get; set; }
    public bool nodeSharedV8 { get; set; }
    public bool nodeSharedZlib { get; set; }
    public bool nodeUseDtrace { get; set; }
    public bool nodeUseEtw { get; set; }
    public bool nodeUseOpenSsl { get; set; } = true;
    public bool v8NoStrictAliasing { get; set; }
    public bool v8UseSnapshot { get; set; }
    public string visibility { get; set; } = "default";
}

public sealed class EmitWarningOptions
{
    public string? type { get; set; }
    public string? code { get; set; }
    public string? detail { get; set; }
    public object? ctor { get; set; }
}

public sealed class ProcessWarning
{
    public string name { get; set; } = "Warning";
    public string message { get; set; } = string.Empty;
    public string? code { get; set; }
    public string? detail { get; set; }
}

public sealed class ProcessFinalization
{
    private readonly HashSet<object> _registered = new(ReferenceEqualityComparer.Instance);

    public void register(object target, Action<object>? callback = null)
    {
        _registered.Add(target);
        _ = callback;
    }

    public bool unregister(object target)
    {
        return _registered.Remove(target);
    }
}

public sealed class ProcessIpcState
{
    public bool connected { get; set; }
}

public static partial class process
{
    private static readonly Stopwatch StartTime = Stopwatch.StartNew();
    private static readonly Stopwatch Hrtime = Stopwatch.StartNew();
    private static readonly ProcessRelease ReleaseValue = new();
    private static readonly ProcessFeatures FeaturesValue = new();
    private static readonly ProcessConfig ConfigValue = new();
    private static readonly ProcessFinalization FinalizationValue = new();
    private static readonly List<ProcessWarning> WarningList = new();
    private static string _title = AppDomain.CurrentDomain.FriendlyName;
    private static Action<Exception>? _uncaughtExceptionCaptureCallback;

    public static string[] execArgv { get; set; } = [];
    public static int debugPort { get; set; } = 9229;
    public static bool sourceMapsEnabled { get; set; }
    public static bool connected { get; private set; }
    public static object? channel { get; private set; }
    public static object? mainModule { get; set; }
    public static bool noDeprecation { get; set; }
    public static bool throwDeprecation { get; set; }
    public static bool traceDeprecation { get; set; }
    public static bool traceProcessWarnings { get; set; }
    public static HashSet<string> allowedNodeEnvironmentFlags { get; } = new(StringComparer.Ordinal);
    public static Writable stdout { get; } = new();
    public static Writable stderr { get; } = new();
    public static Readable stdin { get; } = new();
    public static ProcessFinalization finalization => FinalizationValue;
    public static ProcessRelease release => ReleaseValue;
    public static ProcessFeatures features => FeaturesValue;
    public static ProcessConfig config => ConfigValue;
    public static IReadOnlyList<ProcessWarning> warnings => WarningList;

    public static string title
    {
        get => _title;
        set => _title = value ?? string.Empty;
    }

    public static int getuid() => OperatingSystem.IsWindows() ? throw new PlatformNotSupportedException("getuid is not available on Windows.") : 0;
    public static int geteuid() => getuid();
    public static int getgid() => OperatingSystem.IsWindows() ? throw new PlatformNotSupportedException("getgid is not available on Windows.") : 0;
    public static int getegid() => getgid();
    public static int[] getgroups() => [getgid()];
    public static double uptime() => StartTime.Elapsed.TotalSeconds;
    public static long constrainedMemory() => GC.GetGCMemoryInfo().TotalAvailableMemoryBytes;
    public static long availableMemory() => GC.GetGCMemoryInfo().TotalAvailableMemoryBytes - GC.GetTotalMemory(false);

    public static double[] hrtime(double[]? previous = null)
    {
        var elapsedNs = Hrtime.ElapsedTicks * 1_000_000_000L / Stopwatch.Frequency;
        if (previous is { Length: >= 2 })
        {
            var previousNs = (long)previous[0] * 1_000_000_000L + (long)previous[1];
            elapsedNs -= previousNs;
        }

        return [elapsedNs / 1_000_000_000L, elapsedNs % 1_000_000_000L];
    }

    public static long hrtime_bigint()
    {
        return Hrtime.ElapsedTicks * 1_000_000_000L / Stopwatch.Frequency;
    }

    public static MemoryUsage memoryUsage()
    {
        using var proc = Process.GetCurrentProcess();
        var managed = GC.GetTotalMemory(false);
        return new MemoryUsage
        {
            rss = proc.WorkingSet64,
            heapTotal = GC.GetGCMemoryInfo().HeapSizeBytes,
            heapUsed = managed,
            external = 0,
            arrayBuffers = 0
        };
    }

    public static long memoryUsage_rss()
    {
        using var proc = Process.GetCurrentProcess();
        return proc.WorkingSet64;
    }

    public static CpuUsage cpuUsage(CpuUsage? previous = null)
    {
        using var proc = Process.GetCurrentProcess();
        var current = new CpuUsage
        {
            user = (long)proc.UserProcessorTime.TotalMilliseconds * 1000,
            system = (long)proc.PrivilegedProcessorTime.TotalMilliseconds * 1000
        };

        if (previous == null)
            return current;

        current.user -= previous.user;
        current.system -= previous.system;
        return current;
    }

    public static CpuUsage threadCpuUsage(CpuUsage? previous = null)
    {
        return cpuUsage(previous);
    }

    public static ResourceUsage resourceUsage()
    {
        var cpu = cpuUsage();
        return new ResourceUsage
        {
            userCPUTime = cpu.user,
            systemCPUTime = cpu.system,
            maxRSS = memoryUsage_rss()
        };
    }

    public static object? getBuiltinModule(string name)
    {
        return module.isBuiltin(name) ? module.createRequire(execPath).require(name.StartsWith("node:", StringComparison.Ordinal) ? name : $"node:{name}") : null;
    }

    public static bool hasUncaughtExceptionCaptureCallback()
    {
        return _uncaughtExceptionCaptureCallback != null;
    }

    public static void setUncaughtExceptionCaptureCallback(Action<Exception>? callback)
    {
        _uncaughtExceptionCaptureCallback = callback;
    }

    public static void addUncaughtExceptionCaptureCallback(Action<Exception> callback)
    {
        _uncaughtExceptionCaptureCallback += callback;
    }

    public static void abort()
    {
        Environment.FailFast("process.abort()");
    }

    public static void disconnect()
    {
        connected = false;
        channel = null;
    }

    public static bool send(object? message)
    {
        _ = message;
        return connected;
    }

    public static void emitWarning(string warning, string? type = null, string? code = null)
    {
        emitWarning(warning, new EmitWarningOptions { type = type, code = code });
    }

    public static void emitWarning(string warning, EmitWarningOptions? options)
    {
        var item = new ProcessWarning
        {
            name = options?.type ?? "Warning",
            message = warning,
            code = options?.code,
            detail = options?.detail
        };
        WarningList.Add(item);
        Console.Error.WriteLine(item.code == null ? $"{item.name}: {item.message}" : $"{item.name} [{item.code}]: {item.message}");
    }

    public static string[] getActiveResourcesInfo()
    {
        return [];
    }

    public static void loadEnvFile(string path = ".env")
    {
        if (!File.Exists(path))
            return;

        foreach (var line in File.ReadAllLines(path))
        {
            var trimmed = line.Trim();
            if (trimmed.Length == 0 || trimmed.StartsWith('#'))
                continue;
            var index = trimmed.IndexOf('=');
            if (index <= 0)
                continue;
            env[trimmed[..index]] = trimmed[(index + 1)..];
        }
    }

    public static int umask(int? mask = null)
    {
        _ = mask;
        return 0;
    }

    public static void nextTick(Action callback)
    {
        if (callback == null)
            throw new ArgumentNullException(nameof(callback));

        Task.Run(callback);
    }

    public static void @ref(object? handle)
    {
        _ = handle;
    }

    public static void unref(object? handle)
    {
        _ = handle;
    }
}
