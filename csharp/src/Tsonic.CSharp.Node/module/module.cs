using System;
using System.Collections.Generic;

namespace Tsonic.CSharp.Node;

#pragma warning disable CS1591

#pragma warning disable IDE1006

public static partial class module
{
    public static readonly string[] builtinModules =
    [
        "assert", "async_hooks", "buffer", "child_process", "console", "crypto", "dgram",
        "diagnostics_channel", "dns", "events", "fs", "http", "http2", "https", "module",
        "net", "os", "path", "perf_hooks", "process", "querystring", "readline", "stream",
        "string_decoder", "timers", "tls", "tty", "url", "util", "worker_threads", "zlib"
    ];

    public static RequireFunction createRequire(string filename)
    {
        if (string.IsNullOrWhiteSpace(filename))
            throw new ArgumentException("Filename is required.", nameof(filename));

        return new RequireFunction(filename);
    }

    public static bool isBuiltin(string moduleName)
    {
        var normalized = moduleName.StartsWith("node:", StringComparison.Ordinal) ? moduleName[5..] : moduleName;
        return Array.Exists(builtinModules, name => string.Equals(name, normalized, StringComparison.Ordinal));
    }
}

public sealed class RequireFunction
{
    private static readonly Dictionary<string, object> Builtins = new(StringComparer.Ordinal)
    {
        ["node:module"] = module.builtinModules,
        ["module"] = module.builtinModules,
        ["node:assert"] = "node:assert",
        ["assert"] = "node:assert",
        ["node:async_hooks"] = "node:async_hooks",
        ["async_hooks"] = "node:async_hooks",
        ["node:buffer"] = "node:buffer",
        ["buffer"] = "node:buffer",
        ["node:child_process"] = "node:child_process",
        ["child_process"] = "node:child_process",
        ["node:console"] = "node:console",
        ["console"] = "node:console",
        ["node:crypto"] = "node:crypto",
        ["crypto"] = "node:crypto",
        ["node:dgram"] = "node:dgram",
        ["dgram"] = "node:dgram",
        ["node:diagnostics_channel"] = "node:diagnostics_channel",
        ["diagnostics_channel"] = "node:diagnostics_channel",
        ["node:dns"] = "node:dns",
        ["dns"] = "node:dns",
        ["node:events"] = "node:events",
        ["events"] = "node:events",
        ["node:fs"] = "node:fs",
        ["fs"] = "node:fs",
        ["node:http"] = "node:http",
        ["http"] = "node:http",
        ["node:http2"] = "node:http2",
        ["http2"] = "node:http2",
        ["node:https"] = "node:https",
        ["https"] = "node:https",
        ["node:net"] = "node:net",
        ["net"] = "node:net",
        ["node:os"] = "node:os",
        ["os"] = "node:os",
        ["node:path"] = "node:path",
        ["path"] = "node:path",
        ["node:perf_hooks"] = "node:perf_hooks",
        ["perf_hooks"] = "node:perf_hooks",
        ["node:process"] = "node:process",
        ["process"] = "node:process",
        ["node:querystring"] = "node:querystring",
        ["querystring"] = "node:querystring",
        ["node:readline"] = "node:readline",
        ["readline"] = "node:readline",
        ["node:stream"] = "node:stream",
        ["stream"] = "node:stream",
        ["node:string_decoder"] = "node:string_decoder",
        ["string_decoder"] = "node:string_decoder",
        ["node:timers"] = "node:timers",
        ["timers"] = "node:timers",
        ["node:tls"] = "node:tls",
        ["tls"] = "node:tls",
        ["node:tty"] = "node:tty",
        ["tty"] = "node:tty",
        ["node:url"] = "node:url",
        ["url"] = "node:url",
        ["node:util"] = "node:util",
        ["util"] = "node:util",
        ["node:worker_threads"] = "node:worker_threads",
        ["worker_threads"] = "node:worker_threads",
        ["node:zlib"] = "node:zlib",
        ["zlib"] = "node:zlib"
    };

    public RequireFunction(string filename)
    {
        this.filename = filename;
    }

    public string filename { get; }

    public object require(string specifier)
    {
        if (Builtins.TryGetValue(specifier, out var value))
            return value;

        throw new NotSupportedException($"Runtime require is available only for closed built-in bindings. Unsupported specifier: {specifier}");
    }

    public string resolve(string specifier)
    {
        if (module.isBuiltin(specifier))
            return specifier.StartsWith("node:", StringComparison.Ordinal) ? specifier : $"node:{specifier}";

        throw new NotSupportedException($"Runtime module resolution is not supported for non-built-in specifier: {specifier}");
    }
}
