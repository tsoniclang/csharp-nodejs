using System.Threading.Tasks;
using Xunit;

namespace Tsonic.CSharp.Node.Tests;

public class streamPromisesTests
{
    [Fact]
    public async Task finished_ShouldResolveOnWritableFinish()
    {
        var writable = new Writable();
        var finishedTask = stream.promises.finished(writable);

        writable.end("done");
        await finishedTask;
    }

    [Fact]
    public async Task pipeline_ShouldResolveForSimpleStreams()
    {
        var source = new Readable();
        var destination = new Writable();

        var pipelineTask = stream.promises.pipeline(source, destination);
        source.push("data");
        source.push(null);

        await pipelineTask;
        Assert.True(destination.writableEnded);
    }
}
