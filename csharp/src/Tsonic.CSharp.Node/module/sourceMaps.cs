#pragma warning disable CS1591
#pragma warning disable IDE1006

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Tsonic.CSharp.Node;

public sealed class SourceMapping
{
    public int generatedLine { get; set; }

    public int generatedColumn { get; set; }

    public string? source { get; set; }

    public int originalLine { get; set; }

    public int originalColumn { get; set; }

    public string? name { get; set; }
}

public sealed class SourceOrigin
{
    public string? source { get; set; }

    public int line { get; set; }

    public int column { get; set; }

    public string? name { get; set; }
}

public sealed class SourceMapPayload
{
    public int version { get; set; } = 3;

    public string? file { get; set; }

    public string? sourceRoot { get; set; }

    public string[] sources { get; set; } = [];

    public string[]? sourcesContent { get; set; }

    public string[] names { get; set; } = [];

    public string mappings { get; set; } = string.Empty;
}

public sealed class SourceMapConstructorOptions
{
    public SourceMapping[]? decodedMappings { get; set; }
}

public sealed class SourceMap
{
    private readonly SourceMapping[] _decodedMappings;

    public SourceMap(SourceMapPayload payload, SourceMapConstructorOptions? options = null)
    {
        this.payload = payload ?? throw new ArgumentNullException(nameof(payload));
        _decodedMappings = options?.decodedMappings ?? [];
    }

    public SourceMapPayload payload { get; }

    public int[] lineLengths => (payload.sourcesContent ?? []).SelectMany(content => content.Split('\n')).Select(line => line.Length).ToArray();

    public static SourceMap withDecodedMappings(SourceMapPayload payload, SourceMapping[] decodedMappings)
    {
        return new SourceMap(payload, new SourceMapConstructorOptions { decodedMappings = decodedMappings });
    }

    public SourceMapping? findEntry(int generatedLine, int generatedColumn)
    {
        return _decodedMappings
            .Where(mapping => mapping.generatedLine <= generatedLine)
            .OrderByDescending(mapping => mapping.generatedLine)
            .ThenByDescending(mapping => mapping.generatedColumn)
            .FirstOrDefault(mapping => mapping.generatedLine < generatedLine || mapping.generatedColumn <= generatedColumn);
    }

    public SourceOrigin? findOrigin(int generatedLine, int generatedColumn)
    {
        var mapping = findEntry(generatedLine, generatedColumn);
        if (mapping == null)
            return null;

        return new SourceOrigin
        {
            source = mapping.source,
            line = mapping.originalLine,
            column = mapping.originalColumn,
            name = mapping.name
        };
    }
}

public sealed class SourceMapsSupport
{
    public bool enabled { get; set; }

    public bool nodeModules { get; set; }

    public bool generatedCode { get; set; }
}

public sealed class SetSourceMapsSupportOptions
{
    public bool nodeModules { get; set; }

    public bool generatedCode { get; set; } = true;
}

public sealed class StripTypeScriptTypesOptions
{
    public string mode { get; set; } = "strip";

    public string? sourceUrl { get; set; }

    public bool sourceMap { get; set; }
}

public static partial class module
{
    private static SourceMapsSupport _sourceMapsSupport = new()
    {
        enabled = false,
        nodeModules = false,
        generatedCode = true
    };

    public static void syncBuiltinESMExports()
    {
    }

    public static string? findPackageJSON(string specifier, string? basePath = null)
    {
        var start = Path.GetFullPath(basePath ?? specifier);
        var directory = File.Exists(start) ? Path.GetDirectoryName(start) : start;
        while (!string.IsNullOrEmpty(directory))
        {
            var candidate = Path.Combine(directory, "package.json");
            if (File.Exists(candidate))
                return candidate;

            directory = Directory.GetParent(directory)?.FullName;
        }

        return null;
    }

    public static SourceMapsSupport getSourceMapsSupport()
    {
        return new SourceMapsSupport
        {
            enabled = _sourceMapsSupport.enabled,
            nodeModules = _sourceMapsSupport.nodeModules,
            generatedCode = _sourceMapsSupport.generatedCode
        };
    }

    public static void setSourceMapsSupport(bool enabled)
    {
        _sourceMapsSupport.enabled = enabled;
    }

    public static void setSourceMapsSupport(bool enabled, SetSourceMapsSupportOptions options)
    {
        _sourceMapsSupport.enabled = enabled;
        _sourceMapsSupport.nodeModules = options.nodeModules;
        _sourceMapsSupport.generatedCode = options.generatedCode;
    }

    public static string stripTypeScriptTypes(string code, StripTypeScriptTypesOptions? options = null)
    {
        if (code == null)
            throw new ArgumentNullException(nameof(code));

        options ??= new StripTypeScriptTypesOptions();
        if (options.mode is not ("strip" or "transform"))
            throw new ArgumentException("Unsupported stripTypeScriptTypes mode.", nameof(options));

        return StripTypes(code);
    }

    private static string StripTypes(string code)
    {
        var result = new StringBuilder(code.Length);
        var index = 0;
        while (index < code.Length)
        {
            if (code[index] == ':' && IsTypeAnnotationStart(code, index))
            {
                index++;
                while (index < code.Length && !IsTypeAnnotationEnd(code[index]))
                    index++;
                if (index < code.Length && code[index] == '{' && result.Length > 0 && result[^1] != ' ')
                    result.Append(' ');
                continue;
            }

            if (code[index] == '<' && IsGenericParameterStart(code, index))
            {
                var depth = 1;
                index++;
                while (index < code.Length && depth > 0)
                {
                    if (code[index] == '<') depth++;
                    else if (code[index] == '>') depth--;
                    index++;
                }
                continue;
            }

            result.Append(code[index++]);
        }

        return result.ToString();
    }

    private static bool IsTypeAnnotationStart(string code, int colonIndex)
    {
        var previous = colonIndex - 1;
        while (previous >= 0 && char.IsWhiteSpace(code[previous]))
            previous--;
        if (previous < 0 || !(char.IsLetterOrDigit(code[previous]) || code[previous] == '_' || code[previous] == ')' || code[previous] == ']'))
            return false;

        var next = colonIndex + 1;
        while (next < code.Length && char.IsWhiteSpace(code[next]))
            next++;
        return next < code.Length && (char.IsLetter(code[next]) || code[next] == '_' || code[next] == '{' || code[next] == '[');
    }

    private static bool IsTypeAnnotationEnd(char value)
    {
        return value is ',' or ')' or '=' or ';' or '{' or '\n' or '\r';
    }

    private static bool IsGenericParameterStart(string code, int index)
    {
        var previous = index - 1;
        while (previous >= 0 && char.IsWhiteSpace(code[previous]))
            previous--;
        return previous >= 0 && char.IsLetterOrDigit(code[previous]);
    }
}
