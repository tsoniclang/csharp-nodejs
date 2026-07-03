using System.Threading.Tasks;
using Xunit;

namespace Tsonic.CSharp.Node.Tests;

public class readlinePromisesTests
{
    [Fact]
    public void createInterface_ShouldCreateInterface()
    {
        var input = new Readable();
        var output = new Writable();

        var rl = readline.promises.createInterface(input, output);
        Assert.NotNull(rl);
    }

    [Fact]
    public async Task question_ShouldResolveAnswer()
    {
        var input = new Readable();
        var output = new Writable();
        var rl = readline.promises.createInterface(input, output);

        var answerTask = readline.promises.question(rl, "question?");
        input.resume();
        input.push("value\n");

        var answer = await answerTask;
        Assert.Equal("value", answer);
    }
}
