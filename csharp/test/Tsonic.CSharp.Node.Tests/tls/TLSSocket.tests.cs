using Tsonic.CSharp.Js;
using System;
using System.Threading;
using Xunit;

namespace Tsonic.CSharp.Node.Tests;

public class TLSSocketTests
{
    [Fact]
    public void TLSSocket_Constructor_CreatesInstance()
    {
        JsEventLoop.Run(() =>
        {
            var baseSocket = new Socket();
            var tlsSocket = new TLSSocket(baseSocket);
            Assert.NotNull(tlsSocket);
        });
    }

    [Fact]
    public void TLSSocket_Encrypted_AlwaysTrue()
    {
        JsEventLoop.Run(() =>
        {
            var baseSocket = new Socket();
            var tlsSocket = new TLSSocket(baseSocket);
            Assert.True(tlsSocket.encrypted);
        });
    }

    [Fact]
    public void TLSSocket_Authorized_InitiallyFalse()
    {
        JsEventLoop.Run(() =>
        {
            var baseSocket = new Socket();
            var tlsSocket = new TLSSocket(baseSocket);
            Assert.False(tlsSocket.authorized);
        });
    }

    [Fact]
    public void TLSSocket_GetCertificate_NoCert_ReturnsNull()
    {
        JsEventLoop.Run(() =>
        {
            var baseSocket = new Socket();
            var tlsSocket = new TLSSocket(baseSocket);
            var cert = tlsSocket.getCertificate();
            Assert.Null(cert);
        });
    }

    [Fact]
    public void TLSSocket_GetPeerCertificate_NoCert_ReturnsNull()
    {
        JsEventLoop.Run(() =>
        {
            var baseSocket = new Socket();
            var tlsSocket = new TLSSocket(baseSocket);
            var cert = tlsSocket.getPeerCertificate();
            Assert.Null(cert);
        });
    }

    [Fact]
    public void TLSSocket_GetCipher_ReturnsInfo()
    {
        JsEventLoop.Run(() =>
        {
            var baseSocket = new Socket();
            var tlsSocket = new TLSSocket(baseSocket);
            var cipher = tlsSocket.getCipher();
            Assert.NotNull(cipher);
            Assert.NotNull(cipher.name);
        });
    }

    [Fact]
    public void TLSSocket_GetProtocol_NoHandshake_ReturnsNull()
    {
        JsEventLoop.Run(() =>
        {
            var baseSocket = new Socket();
            var tlsSocket = new TLSSocket(baseSocket);
            var protocol = tlsSocket.getProtocol();
            Assert.Null(protocol);
        });
    }

    [Fact]
    public void TLSSocket_GetSharedSigalgs_ReturnsArray()
    {
        JsEventLoop.Run(() =>
        {
            var baseSocket = new Socket();
            var tlsSocket = new TLSSocket(baseSocket);
            var sigalgs = tlsSocket.getSharedSigalgs();
            Assert.NotNull(sigalgs);
        });
    }

    [Fact]
    public void TLSSocket_IsSessionReused_ReturnsFalse()
    {
        JsEventLoop.Run(() =>
        {
            var baseSocket = new Socket();
            var tlsSocket = new TLSSocket(baseSocket);
            Assert.False(tlsSocket.isSessionReused());
        });
    }

    [Fact]
    public void TLSSocket_Renegotiate_ReturnsfalseAndCallsCallback()
    {
        var resetEvent = new ManualResetEventSlim(false);
        Exception? error = null;
        JsEventLoop.Run(() =>
        {
            var baseSocket = new Socket();
            var tlsSocket = new TLSSocket(baseSocket);
            var result = tlsSocket.renegotiate(new { }, (err) =>
            {
                error = err;
                resetEvent.Set();
            });
            Assert.False(result);
        });
        resetEvent.Wait(1000);
        Assert.NotNull(error);
        Assert.IsType<NotSupportedException>(error);
    }

    [Fact]
    public void TLSSocket_SetMaxSendFragment_ReturnsFalse()
    {
        JsEventLoop.Run(() =>
        {
            var baseSocket = new Socket();
            var tlsSocket = new TLSSocket(baseSocket);
            var result = tlsSocket.setMaxSendFragment(1024);
            Assert.False(result);
        });
    }

    [Fact]
    public void TLSSocket_DisableRenegotiation_DoesNotThrow()
    {
        JsEventLoop.Run(() =>
        {
            var baseSocket = new Socket();
            var tlsSocket = new TLSSocket(baseSocket);
            var exception = Record.Exception(() => tlsSocket.disableRenegotiation());
            Assert.Null(exception);
        });
    }

    [Fact]
    public void TLSSocket_EnableTrace_DoesNotThrow()
    {
        JsEventLoop.Run(() =>
        {
            var baseSocket = new Socket();
            var tlsSocket = new TLSSocket(baseSocket);
            var exception = Record.Exception(() => tlsSocket.enableTrace());
            Assert.Null(exception);
        });
    }

    [Fact]
    public void TLSSocket_ExportKeyingMaterial_ThrowsNotSupported()
    {
        JsEventLoop.Run(() =>
        {
            var baseSocket = new Socket();
            var tlsSocket = new TLSSocket(baseSocket);
            Assert.Throws<NotSupportedException>(() =>
                tlsSocket.exportKeyingMaterial(128, "label", new byte[0]));
        });
    }
}
