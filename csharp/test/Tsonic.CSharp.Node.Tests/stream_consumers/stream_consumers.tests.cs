using System.Threading.Tasks;
using Xunit;

namespace Tsonic.CSharp.Node.Tests;

public class StreamConsumersTests
{
    [Fact]
    public async Task Text_ReadsReadableContent()
    {
        var readable = new Readable();
        readable.push("hello");
        readable.push(null);

        var result = await stream_consumers.text(readable);

        Assert.Equal("hello", result);
    }

    [Fact]
    public async Task Buffer_ReadsReadableContent()
    {
        var readable = new Readable();
        readable.push("abc");
        readable.push(null);

        var result = await stream_consumers.buffer(readable);

        Assert.Equal("abc", result.toString());
    }

    [Fact]
    public async Task Json_DeserializesReadableContent()
    {
        var readable = new Readable();
        readable.push("{\"value\":42}");
        readable.push(null);

        using var result = await stream_consumers.json(readable);

        Assert.NotNull(result);
        Assert.Equal(42, result.RootElement.GetProperty("value").GetInt32());
    }
}
