using Tsonic.CSharp.Js;
using Xunit;

namespace Tsonic.CSharp.Node.Tests;

public class readlinePromisesTests
{
    [Fact]
    public void createInterface_ShouldCreateInterface() => JsEventLoop.Run(() =>
    {
        var input = new Readable();
        var output = new Writable();

        var rl = readline.promises.createInterface(input, output);
        Assert.NotNull(rl);
    });

    [Fact]
    public void question_ShouldResolveAnswer()
    {
        JsEventLoop.Run(async () =>
        {
            var input = new Readable();
            var output = new Writable();
            var rl = readline.promises.createInterface(input, output);

            var answerTask = readline.promises.question(rl, "question?");
            input.resume();
            input.push("value\n");

            var answer = await answerTask;
            Assert.Equal("value", answer);
        });
    }
}
