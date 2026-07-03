using Tsonic.CSharp.Node;
using Xunit;

namespace Tsonic.CSharp.Node.Tests;

public class TtyTests
{
    [Fact]
    public void ReadStream_TracksRawMode()
    {
        var stream = new TtyReadStream(0);

        Assert.False(stream.isRaw);
        Assert.Same(stream, stream.setRawMode(true));
        Assert.True(stream.isRaw);
    }

    [Fact]
    public void Isatty_ReturnsFalseForUnknownFd()
    {
        Assert.False(tty.isatty(99));
    }

    [Fact]
    public void WriteStream_ExposesDimensionsAndClosedCursorHelpers()
    {
        var stream = new TtyWriteStream(1);

        Assert.Equal(1, stream.fd);
        Assert.True(stream.columns >= 0);
        Assert.True(stream.rows >= 0);
        Assert.True(stream.clearLine());
        Assert.True(stream.clearScreenDown());
    }
}
