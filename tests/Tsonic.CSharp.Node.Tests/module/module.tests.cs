using System;
using Tsonic.CSharp.Node;
using Xunit;

namespace Tsonic.CSharp.Node.Tests;

public class ModuleTests
{
    [Fact]
    public void IsBuiltin_AcceptsNodePrefix()
    {
        Assert.True(module.isBuiltin("node:fs"));
        Assert.True(module.isBuiltin("https"));
        Assert.False(module.isBuiltin("acme"));
    }

    [Fact]
    public void CreateRequire_ResolvesBuiltinsOnly()
    {
        var require = module.createRequire("/src/app.js");

        Assert.Equal("node:fs", require.resolve("fs"));
        Assert.Throws<NotSupportedException>(() => require.resolve("./local.js"));
    }

    [Fact]
    public void CreateRequire_RejectsEmptyFilename()
    {
        Assert.Throws<ArgumentException>(() => module.createRequire(""));
    }

    [Fact]
    public void Require_ReturnsClosedBuiltinAndRejectsUserland()
    {
        var require = module.createRequire("/src/app.js");

        var builtin = Assert.IsType<string[]>(require.require("node:module"));

        Assert.Contains("fs", builtin);
        Assert.Throws<NotSupportedException>(() => require.require("left-pad"));
    }
}
