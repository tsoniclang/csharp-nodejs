using System;
using System.Threading.Tasks;
using Xunit;

namespace Tsonic.CSharp.Node.Tests;

public class assertAsyncTests
{
    [Fact]
    public void strict_ShouldAliasStrictEqual()
    {
        assert.strict(42, 42);
        Assert.Throws<AssertionError>(() => assert.strict(42, "42"));
    }

    [Fact]
    public async Task rejects_ShouldPassWhenTaskThrows()
    {
        await assert.rejects(async () =>
        {
            await Task.Yield();
            throw new InvalidOperationException("boom");
        });
    }

    [Fact]
    public async Task rejects_ShouldThrowWhenTaskDoesNotThrow()
    {
        await Assert.ThrowsAsync<AssertionError>(() => assert.rejects(() => Task.CompletedTask));
    }

    [Fact]
    public async Task doesNotReject_ShouldPassWhenTaskCompletes()
    {
        await assert.doesNotReject(() => Task.CompletedTask);
    }

    [Fact]
    public async Task doesNotReject_ShouldThrowWhenTaskThrows()
    {
        await Assert.ThrowsAsync<AssertionError>(() => assert.doesNotReject(() => Task.FromException(new Exception("nope"))));
    }
}
