using Xunit;
using System.Runtime.InteropServices;

namespace Tsonic.CSharp.Node.Tests;

public class envTests
{
    [Fact]
    public void env_ShouldReturnValidObject()
    {
        var env = process.env;

        Assert.NotNull(env);
    }

    [Fact]
    public void env_ShouldContainSystemEnvironmentVariables()
    {
        var env = process.env;

        // PATH should exist on all platforms
        var hasPath = env.ContainsKey("PATH") || env.ContainsKey("Path");
        Assert.True(hasPath);
    }

    [Fact]
    public void env_ShouldAllowSettingValues()
    {
        var env = process.env;
        var testKey = "TSONIC_TEST_VAR";
        var testValue = "test-value";

        env[testKey] = testValue;

        Assert.Equal(testValue, env[testKey]);
        Assert.Equal(testValue, Environment.GetEnvironmentVariable(testKey));

        // Cleanup
        env.Remove(testKey);
    }

    [Fact]
    public void env_ShouldAllowRemovingValues()
    {
        var env = process.env;
        var testKey = "TSONIC_TEST_VAR_REMOVE";

        env[testKey] = "value";
        Assert.True(env.ContainsKey(testKey));

        env.Remove(testKey);

        Assert.False(env.ContainsKey(testKey));
        Assert.Null(Environment.GetEnvironmentVariable(testKey));
    }

    [Fact]
    public void env_ShouldBeCaseInsensitiveOnWindows()
    {
        if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            return; // Skip on non-Windows
        }

        var env = process.env;
        var testKey = "TSONIC_CASE_TEST";

        env[testKey] = "value";

        Assert.Equal("value", env["TSONIC_CASE_TEST"]);
        Assert.Equal("value", env["tsonic_case_test"]);
        Assert.Equal("value", env["Tsonic_Case_Test"]);

        // Cleanup
        env.Remove(testKey);
    }

    [Fact]
    public void env_ShouldBeCaseSensitiveOnUnix()
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            return; // Skip on Windows
        }

        var env = process.env;
        var testKey1 = "TSONIC_CASE_TEST";
        var testKey2 = "tsonic_case_test";

        env[testKey1] = "value1";
        env[testKey2] = "value2";

        Assert.Equal("value1", env[testKey1]);
        Assert.Equal("value2", env[testKey2]);

        // Cleanup
        env.Remove(testKey1);
        env.Remove(testKey2);
    }

    [Fact]
    public void env_ShouldRemoveKeyWhenSetToNull()
    {
        var env = process.env;
        var testKey = "TSONIC_NULL_TEST";

        // First set a value
        env[testKey] = "value";
        Assert.True(env.ContainsKey(testKey));

        // Setting to null should remove the key
        env[testKey] = null;

        Assert.False(env.ContainsKey(testKey));
        Assert.Null(env[testKey]);
    }
}
