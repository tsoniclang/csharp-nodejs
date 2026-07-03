using System.Threading.Tasks;
using Xunit;

namespace Tsonic.CSharp.Node.Tests;

public class ReadlinePromisesModuleTests
{
    [Fact]
    public void CreateInterface_ReturnsPromiseInterface()
    {
        var rl = readline_promises.createInterface();

        Assert.NotNull(rl);
        rl.close();
    }

    [Fact]
    public async Task CursorHelpers_Complete()
    {
        var writable = new Writable();

        await readline_promises.clearLine(writable);
        await readline_promises.clearScreenDown(writable);
        await readline_promises.cursorTo(writable, 0);
        await readline_promises.moveCursor(writable, 1, 1);
    }
}
