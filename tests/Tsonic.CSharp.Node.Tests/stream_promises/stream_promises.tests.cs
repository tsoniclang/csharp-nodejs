using System.Threading.Tasks;
using Xunit;

namespace Tsonic.CSharp.Node.Tests;

public class StreamPromisesModuleTests
{
    [Fact]
    public async Task Finished_CompletesWhenStreamFinishes()
    {
        var writable = new Writable();
        var finished = stream_promises.finished(writable);

        writable.end();

        await finished;
    }

    [Fact]
    public async Task Pipeline_RejectsTooFewStreams()
    {
        await Assert.ThrowsAsync<System.ArgumentException>(() => stream_promises.pipeline(new Readable()));
    }
}
