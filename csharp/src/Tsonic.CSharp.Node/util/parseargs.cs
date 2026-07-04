#pragma warning disable CS1591
#pragma warning disable IDE1006

using System;
using System.Collections.Generic;

namespace Tsonic.CSharp.Node;

public sealed class ParseArgsOptionDescriptor
{
    public string type { get; set; } = "string";

    public bool multiple { get; set; }

    public string? @short { get; set; }

    public object? @default { get; set; }
}

public sealed class ParseArgsConfig
{
    public string[] args { get; set; } = [];

    public Dictionary<string, ParseArgsOptionDescriptor> options { get; set; } = [];

    public bool strict { get; set; } = true;

    public bool allowPositionals { get; set; }

    public bool tokens { get; set; }
}

public sealed class ParseArgsToken
{
    public string kind { get; set; } = string.Empty;

    public int index { get; set; }

    public string? name { get; set; }

    public string? rawName { get; set; }

    public string? value { get; set; }

    public bool inlineValue { get; set; }
}

public sealed class ParseArgsResult
{
    public Dictionary<string, object?> values { get; set; } = [];

    public string[] positionals { get; set; } = [];

    public ParseArgsToken[] tokens { get; set; } = [];
}

public static partial class util
{
    public static ParseArgsResult parseArgs(ParseArgsConfig config)
    {
        if (config == null)
            throw new ArgumentNullException(nameof(config));

        var values = new Dictionary<string, object?>(StringComparer.Ordinal);
        var positionals = new List<string>();
        var tokens = new List<ParseArgsToken>();
        var shortToLong = BuildShortMap(config.options);

        foreach (var (key, descriptor) in config.options)
        {
            if (descriptor.@default != null)
                values[key] = descriptor.multiple && descriptor.@default is not Array ? new[] { descriptor.@default } : descriptor.@default;
        }

        for (var index = 0; index < config.args.Length; index++)
        {
            var arg = config.args[index];
            if (arg == "--")
            {
                for (var positionalIndex = index + 1; positionalIndex < config.args.Length; positionalIndex++)
                    positionals.Add(config.args[positionalIndex]);
                break;
            }

            if (arg.StartsWith("--", StringComparison.Ordinal) && arg.Length > 2)
            {
                var raw = arg[2..];
                var equals = raw.IndexOf('=');
                var name = equals >= 0 ? raw[..equals] : raw;
                var inlineValue = equals >= 0;
                var inlineText = equals >= 0 ? raw[(equals + 1)..] : null;
                var descriptor = ResolveDescriptor(name, config);
                var value = ParseOptionValue(name, descriptor, inlineText, inlineValue, config.args, ref index);
                StoreValue(values, name, descriptor, value);
                tokens.Add(new ParseArgsToken { kind = "option", index = index, name = name, rawName = $"--{name}", value = value?.ToString(), inlineValue = inlineValue });
                continue;
            }

            if (arg.StartsWith("-", StringComparison.Ordinal) && arg.Length > 1)
            {
                var shortName = arg[1..];
                if (!shortToLong.TryGetValue(shortName, out var name))
                {
                    if (config.strict)
                        throw new ArgumentException($"Unknown option '-{shortName}'.");

                    positionals.Add(arg);
                    continue;
                }

                var descriptor = ResolveDescriptor(name, config);
                var value = ParseOptionValue(name, descriptor, null, false, config.args, ref index);
                StoreValue(values, name, descriptor, value);
                tokens.Add(new ParseArgsToken { kind = "option", index = index, name = name, rawName = $"-{shortName}", value = value?.ToString(), inlineValue = false });
                continue;
            }

            if (!config.allowPositionals && config.strict)
                throw new ArgumentException($"Unexpected positional argument '{arg}'.");

            positionals.Add(arg);
            tokens.Add(new ParseArgsToken { kind = "positional", index = index, value = arg });
        }

        return new ParseArgsResult
        {
            values = values,
            positionals = positionals.ToArray(),
            tokens = config.tokens ? tokens.ToArray() : []
        };
    }

    public static Dictionary<string, string> parseEnv(string content)
    {
        var result = new Dictionary<string, string>(StringComparer.Ordinal);
        foreach (var rawLine in content.Split(['\r', '\n'], StringSplitOptions.RemoveEmptyEntries))
        {
            var line = rawLine.Trim();
            if (line.Length == 0 || line.StartsWith("#", StringComparison.Ordinal))
                continue;

            var equals = line.IndexOf('=');
            if (equals <= 0)
                continue;

            var key = line[..equals].Trim();
            var value = line[(equals + 1)..].Trim();
            if (value.Length >= 2 && ((value[0] == '"' && value[^1] == '"') || (value[0] == '\'' && value[^1] == '\'')))
                value = value[1..^1];

            result[key] = value;
        }

        return result;
    }

    private static Dictionary<string, string> BuildShortMap(Dictionary<string, ParseArgsOptionDescriptor> options)
    {
        var map = new Dictionary<string, string>(StringComparer.Ordinal);
        foreach (var (name, descriptor) in options)
        {
            if (!string.IsNullOrEmpty(descriptor.@short))
                map[descriptor.@short] = name;
        }

        return map;
    }

    private static ParseArgsOptionDescriptor ResolveDescriptor(string name, ParseArgsConfig config)
    {
        if (config.options.TryGetValue(name, out var descriptor))
            return descriptor;

        if (config.strict)
            throw new ArgumentException($"Unknown option '--{name}'.");

        return new ParseArgsOptionDescriptor { type = "boolean" };
    }

    private static object? ParseOptionValue(string name, ParseArgsOptionDescriptor descriptor, string? inlineText, bool inlineValue, string[] args, ref int index)
    {
        return descriptor.type switch
        {
            "boolean" => inlineValue ? bool.Parse(inlineText ?? "false") : true,
            "string" => inlineValue ? inlineText ?? string.Empty : TakeNextValue(name, args, ref index),
            _ => throw new ArgumentException($"Unsupported option type for '{name}': {descriptor.type}.")
        };
    }

    private static string TakeNextValue(string name, string[] args, ref int index)
    {
        if (index + 1 >= args.Length)
            throw new ArgumentException($"Option '--{name}' requires a value.");

        index++;
        return args[index];
    }

    private static void StoreValue(Dictionary<string, object?> values, string name, ParseArgsOptionDescriptor descriptor, object? value)
    {
        if (!descriptor.multiple)
        {
            values[name] = value;
            return;
        }

        if (!values.TryGetValue(name, out var existing) || existing is not List<object?> list)
        {
            list = [];
            values[name] = list;
        }

        list.Add(value);
    }
}
