#pragma warning disable CS1591
#pragma warning disable IDE1006

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace Tsonic.CSharp.Node;

public sealed class StyleTextOptions
{
    public bool validateStream { get; set; } = true;
}

public sealed class CallSiteObject
{
    public string? functionName { get; set; }

    public string? fileName { get; set; }

    public int? lineNumber { get; set; }

    public int? columnNumber { get; set; }
}

public sealed class CallSite
{
    public CallSite(string? functionName, string? fileName, int? lineNumber, int? columnNumber)
    {
        _functionName = functionName;
        _fileName = fileName;
        _lineNumber = lineNumber;
        _columnNumber = columnNumber;
    }

    private readonly string? _functionName;
    private readonly string? _fileName;
    private readonly int? _lineNumber;
    private readonly int? _columnNumber;

    public static CallSite withPosition(string? functionName, string? fileName, int? lineNumber, int? columnNumber)
    {
        return new CallSite(functionName, fileName, lineNumber, columnNumber);
    }

    public string? getFunctionName() => _functionName;

    public string? getFileName() => _fileName;

    public string? getScriptNameOrSourceURL() => _fileName;

    public int? getLineNumber() => _lineNumber;

    public int? getColumnNumber() => _columnNumber;

    public bool isEval() => false;

    public bool isNative() => false;

    public bool isConstructor() => false;

    public bool isAsync() => false;
}

public sealed class GetCallSitesOptions
{
    public bool sourceMap { get; set; }
}

public sealed class DiffEntry
{
    public string type { get; set; } = "equal";

    public string value { get; set; } = string.Empty;
}

public sealed class SystemErrorEntry
{
    public SystemErrorEntry(int code, string name, string message)
    {
        this.code = code;
        this.name = name;
        this.message = message;
    }

    public int code { get; }

    public string name { get; }

    public string message { get; }
}

public sealed class DebugLogger
{
    private readonly DebugLogFunction _logger;

    public DebugLogger(string section)
    {
        _logger = util.debuglog(section);
        enabled = Environment.GetEnvironmentVariable("NODE_DEBUG")?.Contains(section, StringComparison.OrdinalIgnoreCase) == true;
    }

    public bool enabled { get; }

    public void log(string message, params object[] args) => _logger(message, args);
}

public static partial class util
{
    public static readonly Dictionary<string, object?> defaultOptions = new(StringComparer.Ordinal)
    {
        ["colors"] = false,
        ["depth"] = 2
    };

    public static string styleText(string format, string text, StyleTextOptions? options = null)
    {
        _ = options;
        return format switch
        {
            "bold" => "\u001b[1m" + text + "\u001b[22m",
            "red" => "\u001b[31m" + text + "\u001b[39m",
            "green" => "\u001b[32m" + text + "\u001b[39m",
            "yellow" => "\u001b[33m" + text + "\u001b[39m",
            "blue" => "\u001b[34m" + text + "\u001b[39m",
            _ => text
        };
    }

    public static string getSystemErrorName(int errorNumber)
    {
        return errorNumber switch
        {
            2 => "ENOENT",
            13 => "EACCES",
            17 => "EEXIST",
            20 => "ENOTDIR",
            21 => "EISDIR",
            22 => "EINVAL",
            _ => $"ERRNO_{errorNumber}"
        };
    }

    public static string getSystemErrorMessage(int errorNumber)
    {
        return errorNumber switch
        {
            2 => "no such file or directory",
            13 => "permission denied",
            17 => "file already exists",
            20 => "not a directory",
            21 => "is a directory",
            22 => "invalid argument",
            _ => $"system error {errorNumber}"
        };
    }

    public static Dictionary<int, SystemErrorEntry> getSystemErrorMap()
    {
        var result = new Dictionary<int, SystemErrorEntry>();
        foreach (var code in new[] { 2, 13, 17, 20, 21, 22 })
            result[code] = new SystemErrorEntry(code, getSystemErrorName(code), getSystemErrorMessage(code));
        return result;
    }

    public static Func<Task<TResult>> promisify<TResult>(Func<TResult> callbackStyle)
    {
        if (callbackStyle == null)
            throw new ArgumentNullException(nameof(callbackStyle));

        return () => Task.FromResult(callbackStyle());
    }

    public static Func<T, Task<TResult>> promisify<T, TResult>(Func<T, TResult> callbackStyle)
    {
        if (callbackStyle == null)
            throw new ArgumentNullException(nameof(callbackStyle));

        return arg => Task.FromResult(callbackStyle(arg));
    }

    public static Action<Task> callbackify(Func<Task> asyncFunction)
    {
        if (asyncFunction == null)
            throw new ArgumentNullException(nameof(asyncFunction));

        return _ => asyncFunction().GetAwaiter().GetResult();
    }

    public static async Task<bool> aborted(CancellationToken cancellationToken)
    {
        if (cancellationToken.IsCancellationRequested)
            return true;

        var completion = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        using var registration = cancellationToken.Register(static state => ((TaskCompletionSource)state!).TrySetResult(), completion);
        await completion.Task.ConfigureAwait(false);
        return true;
    }

    public static CancellationToken transferableAbortSignal(CancellationToken signal) => signal;

    public static CancellationTokenSource transferableAbortController() => new();

    public static CallSite[] getCallSites(GetCallSitesOptions? options = null)
    {
        _ = options;
        var trace = new StackTrace(1, true);
        var frames = trace.GetFrames() ?? [];
        var result = new List<CallSite>(frames.Length);
        foreach (var frame in frames)
        {
            result.Add(new CallSite(null, frame.GetFileName(), frame.GetFileLineNumber(), frame.GetFileColumnNumber()));
        }

        return result.ToArray();
    }

    public static DiffEntry[] diff(string actual, string expected)
    {
        if (actual == expected)
            return [new DiffEntry { type = "equal", value = actual }];

        return
        [
            new DiffEntry { type = "delete", value = actual },
            new DiffEntry { type = "insert", value = expected }
        ];
    }

    public static int convertProcessSignalToExitCode(string signal)
    {
        return signal switch
        {
            "SIGHUP" => 129,
            "SIGINT" => 130,
            "SIGQUIT" => 131,
            "SIGTERM" => 143,
            _ => 128
        };
    }

    public static void setTraceSigInt(bool enable)
    {
        _ = enable;
    }
}
