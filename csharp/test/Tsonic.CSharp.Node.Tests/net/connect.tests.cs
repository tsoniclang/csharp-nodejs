using System.Threading.Tasks;
using Tsonic.CSharp.Js;
using System;
using System.Threading;
using Xunit;

namespace Tsonic.CSharp.Node.Tests;

[Collection(JsEventLoopCollection.Name)]
public class Net_connectTests
{
    [Fact]
    public void connect_CreatesSocket()
    {
        JsEventLoop.Run(async () =>
        {
            Socket? acceptedSocket = null;
            Socket? socket = null;
            var connected = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
            var server = net.createServer(client => acceptedSocket = client);
            server.listen(0, "127.0.0.1");
            var address = Assert.IsType<AddressInfo>(server.address());
            try
            {

                socket = net.connect(address.port, "127.0.0.1", () => connected.TrySetResult(true));

                Assert.True(await connected.Task.WaitAsync(TimeSpan.FromSeconds(5)));
                Assert.NotNull(socket);
                Assert.IsType<Socket>(socket);
            }
            finally
            {
                socket?.destroy();
                acceptedSocket?.destroy();
                server.close();
            }
        });
    }
}
