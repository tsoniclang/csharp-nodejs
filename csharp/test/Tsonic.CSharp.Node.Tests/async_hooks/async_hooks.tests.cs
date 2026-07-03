using Tsonic.CSharp.Node;
using Xunit;

namespace Tsonic.CSharp.Node.Tests;

public class AsyncHooksTests
{
    [Fact]
    public void AsyncLocalStorage_RestoresPreviousStore()
    {
        var storage = new AsyncLocalStorage<string>();
        storage.enterWith("outer");

        var inner = storage.run("inner", () => storage.getStore());

        Assert.Equal("inner", inner);
        Assert.Equal("outer", storage.getStore());
    }

    [Fact]
    public void AsyncLocalStorage_DisableClearsStore()
    {
        var storage = new AsyncLocalStorage<string>();

        storage.enterWith("value");
        storage.disable();

        Assert.Null(storage.getStore());
    }

    [Fact]
    public void AsyncLocalStorage_ActionRunRestoresPreviousStore()
    {
        var storage = new AsyncLocalStorage<string>();
        storage.enterWith("outer");
        string? observed = null;

        storage.run("inner", () => observed = storage.getStore());

        Assert.Equal("inner", observed);
        Assert.Equal("outer", storage.getStore());
    }

    [Fact]
    public void AsyncResource_RunsInScopedAsyncId()
    {
        var resource = new AsyncResource("TEST");
        var observed = resource.runInAsyncScope(() => async_hooks.executionAsyncId());

        Assert.Equal(resource.asyncId(), observed);
    }

    [Fact]
    public void AsyncResource_ActionScopeAndDestroy_AreObservable()
    {
        var resource = new AsyncResource("TEST");
        long observed = 0;

        resource.runInAsyncScope(() => observed = async_hooks.executionAsyncId());
        resource.emitDestroy();

        Assert.Equal(resource.asyncId(), observed);
        Assert.True(resource.destroyed);
    }

    [Fact]
    public void AsyncResource_RejectsEmptyType()
    {
        Assert.Throws<System.ArgumentException>(() => new AsyncResource(""));
    }

    [Fact]
    public void AsyncHook_CanBeEnabledAndDisabled()
    {
        var hook = async_hooks.createHook(new AsyncHookCallbacks
        {
            before = _ => { }
        });

        hook.enable();
        Assert.True(hook.enabled);
        hook.disable();
        Assert.False(hook.enabled);
    }
}
