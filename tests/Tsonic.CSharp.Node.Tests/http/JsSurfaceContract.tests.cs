using System;
using System.Collections.Generic;
using Xunit;

namespace Tsonic.CSharp.Node.Tests;

public class HttpJsSurfaceContractTests
{
    [Fact]
    public void Http_IntegerBackedMembers_CompileAgainstExactNumericContracts()
    {
        Action compile = () =>
        {
            Tsonic.CSharp.Node.Http.Server server = null!;
            Tsonic.CSharp.Node.Http.ServerResponse response = null!;
            Tsonic.CSharp.Node.Http.IncomingMessage incoming = null!;

            int timeout = server.timeout;
            int headersTimeout = server.headersTimeout;
            int requestTimeout = server.requestTimeout;
            int keepAliveTimeout = server.keepAliveTimeout;
            int statusCode = response.statusCode;
            int? incomingStatusCode = incoming.statusCode;

            server.timeout = timeout;
            server.headersTimeout = headersTimeout;
            server.requestTimeout = requestTimeout;
            server.keepAliveTimeout = keepAliveTimeout;
            response.statusCode = statusCode;

            _ = incomingStatusCode;
        };

        Assert.NotNull(compile);
    }

    [Fact]
    public void Http_IntegerBackedMethods_CompileAgainstExactNumericContracts()
    {
        Func<Tsonic.CSharp.Node.Http.Server, Tsonic.CSharp.Node.Http.Server> listen = (server) => server.listen(80, "127.0.0.1", 128, () => { });
        Func<Tsonic.CSharp.Node.Http.Server, Tsonic.CSharp.Node.Http.Server> serverSetTimeout = (server) => server.setTimeout(1000, () => { });
        Func<Tsonic.CSharp.Node.Http.IncomingMessage, Tsonic.CSharp.Node.Http.IncomingMessage> incomingSetTimeout = (incoming) => incoming.setTimeout(1000, () => { });
        Func<Tsonic.CSharp.Node.Http.ServerResponse, Tsonic.CSharp.Node.Http.ServerResponse> responseSetTimeout = (response) => response.setTimeout(1000, () => { });
        Func<Tsonic.CSharp.Node.Http.ServerResponse, Tsonic.CSharp.Node.Http.ServerResponse> writeHead = (response) =>
            response.writeHead(204, "No Content", new Dictionary<string, string>());

        Assert.NotNull(listen);
        Assert.NotNull(serverSetTimeout);
        Assert.NotNull(incomingSetTimeout);
        Assert.NotNull(responseSetTimeout);
        Assert.NotNull(writeHead);
    }
}
