using System.Collections.Generic;
using Xunit;

namespace Tsonic.CSharp.Node.Tests;

public class UtilParseArgsTests
{
    [Fact]
    public void ParseArgs_ShouldParseLongShortBooleanStringPositionalsAndTokens()
    {
        var result = util.parseArgs(new ParseArgsConfig
        {
            args = ["--name=app", "-v", "src/index.ts"],
            allowPositionals = true,
            tokens = true,
            options = new Dictionary<string, ParseArgsOptionDescriptor>
            {
                ["name"] = new() { type = "string" },
                ["verbose"] = new() { type = "boolean", @short = "v" }
            }
        });

        Assert.Equal("app", result.values["name"]);
        Assert.Equal(true, result.values["verbose"]);
        Assert.Equal(["src/index.ts"], result.positionals);
        Assert.Equal(3, result.tokens.Length);
    }

    [Fact]
    public void ParseArgs_ShouldRejectUnknownStrictOptions()
    {
        Assert.Throws<ArgumentException>(() => util.parseArgs(new ParseArgsConfig
        {
            args = ["--bad"]
        }));
    }

    [Fact]
    public void ParseEnv_ShouldParseQuotedValuesAndComments()
    {
        var values = util.parseEnv("A=1\n#x\nB=\"two words\"");

        Assert.Equal("1", values["A"]);
        Assert.Equal("two words", values["B"]);
    }
}
