using System;

namespace Tsonic.CSharp.Node;

#pragma warning disable CS1591

#pragma warning disable IDE1006

public static class tty
{
    public static bool isatty(int fd)
    {
        return fd switch
        {
            0 => !Console.IsInputRedirected,
            1 => !Console.IsOutputRedirected,
            2 => !Console.IsErrorRedirected,
            _ => false
        };
    }
}

public class TtyReadStream : EventEmitter
{
    public TtyReadStream(int fd)
    {
        this.fd = fd;
        isRaw = false;
    }

    public int fd { get; }
    public bool isRaw { get; private set; }
    public bool isTTY => tty.isatty(fd);

    public TtyReadStream setRawMode(bool mode)
    {
        isRaw = mode;
        return this;
    }
}

public class TtyWriteStream : EventEmitter
{
    public TtyWriteStream(int fd)
    {
        this.fd = fd;
    }

    public int fd { get; }
    public bool isTTY => tty.isatty(fd);
    public int columns => Console.IsOutputRedirected ? 0 : Console.WindowWidth;
    public int rows => Console.IsOutputRedirected ? 0 : Console.WindowHeight;

    public bool clearLine(int dir = 0)
    {
        _ = dir;
        return true;
    }

    public bool clearScreenDown()
    {
        return true;
    }

    public bool cursorTo(int x, int? y = null)
    {
        if (Console.IsOutputRedirected)
            return false;

        Console.SetCursorPosition(Math.Max(0, x), Math.Max(0, y ?? Console.CursorTop));
        return true;
    }

    public bool moveCursor(int dx, int dy)
    {
        if (Console.IsOutputRedirected)
            return false;

        Console.SetCursorPosition(Math.Max(0, Console.CursorLeft + dx), Math.Max(0, Console.CursorTop + dy));
        return true;
    }
}
