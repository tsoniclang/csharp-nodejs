using System.Threading.Tasks;

namespace Tsonic.CSharp.Node;

#pragma warning disable CS1591
#pragma warning disable IDE1006

public static class readline_promises
{
    public static ReadlinePromisesInterface createInterface()
    {
        return new ReadlinePromisesInterface(readline.createInterface(new InterfaceOptions
        {
            input = new Readable(),
            output = new Writable()
        }));
    }

    public static ReadlinePromisesInterface createInterface(InterfaceOptions options)
    {
        return new ReadlinePromisesInterface(readline.createInterface(options));
    }

    public static Task clearLine(Writable stream, int dir = 0)
    {
        readline.clearLine(stream, dir);
        return Task.CompletedTask;
    }

    public static Task clearScreenDown(Writable stream)
    {
        readline.clearScreenDown(stream);
        return Task.CompletedTask;
    }

    public static Task cursorTo(Writable stream, int x, int? y = null)
    {
        readline.cursorTo(stream, x, y);
        return Task.CompletedTask;
    }

    public static Task moveCursor(Writable stream, int dx, int dy)
    {
        readline.moveCursor(stream, dx, dy);
        return Task.CompletedTask;
    }
}

public sealed class ReadlinePromisesInterface
{
    private readonly Interface _inner;

    internal ReadlinePromisesInterface(Interface inner)
    {
        _inner = inner;
    }

    public Task<string> question(string query)
    {
        return _inner.questionAsync(query);
    }

    public void close()
    {
        _inner.close();
    }
}
