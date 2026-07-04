using System;
using System.IO;
using Xunit;

namespace Tsonic.CSharp.Node.Tests;

public class ModuleSourceMapsTests
{
    [Fact]
    public void SourceMap_ShouldFindDecodedEntryAndOrigin()
    {
        var payload = new SourceMapPayload
        {
            file = "out.js",
            sources = ["in.ts"],
            sourcesContent = ["const x = 1;\n"]
        };
        var map = SourceMap.withDecodedMappings(payload,
        [
            new SourceMapping
            {
                generatedLine = 1,
                generatedColumn = 0,
                source = "in.ts",
                originalLine = 1,
                originalColumn = 0,
                name = "x"
            }
        ]);

        var entry = map.findEntry(1, 4);
        var origin = map.findOrigin(1, 4);

        Assert.NotNull(entry);
        Assert.NotNull(origin);
        Assert.Equal("in.ts", origin!.source);
        Assert.Contains(12, map.lineLengths);
    }

    [Fact]
    public void SourceMapsSupport_ShouldRoundTripState()
    {
        module.setSourceMapsSupport(true, new SetSourceMapsSupportOptions
        {
            nodeModules = true,
            generatedCode = false
        });

        var state = module.getSourceMapsSupport();

        Assert.True(state.enabled);
        Assert.True(state.nodeModules);
        Assert.False(state.generatedCode);
    }

    [Fact]
    public void FindPackageJson_ShouldWalkParents()
    {
        var root = Path.Combine(Path.GetTempPath(), $"module-test-{Guid.NewGuid():N}");
        var child = Path.Combine(root, "src", "lib");
        Directory.CreateDirectory(child);
        var packageJson = Path.Combine(root, "package.json");
        File.WriteAllText(packageJson, "{}");
        try
        {
            Assert.Equal(packageJson, module.findPackageJSON(child));
        }
        finally
        {
            Directory.Delete(root, true);
        }
    }

    [Fact]
    public void StripTypeScriptTypes_ShouldRemoveSimpleAnnotations()
    {
        var output = module.stripTypeScriptTypes("function add(a: number, b: number): number { return a + b; }");

        Assert.Equal("function add(a, b) { return a + b; }", output);
    }
}
