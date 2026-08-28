using System;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Concurrent;
using System.Threading;

namespace Tsonic.CSharp.Node;

#pragma warning disable CS8981 // Lowercase type names
#pragma warning disable IDE1006 // Naming rule violation
#pragma warning disable SYSLIB0058 // Obsolete cipher algorithm APIs
#pragma warning disable SYSLIB0039 // Obsolete TLS protocol versions
#pragma warning disable CS0649 // Field never assigned

/// <summary>
/// Performs transparent encryption of written data and all required TLS negotiation.
/// </summary>
public partial class TLSSocket : Socket
{
    /// <summary>
    /// Writes data to the TLS stream.
    /// </summary>
    public new bool write(byte[] data, Action<Exception?>? callback = null)
    {
        if (destroyed)
        {
            if (callback != null)
                Tsonic.CSharp.Js.JsEventLoop.EnqueueReferenced(() => callback(new InvalidOperationException("TLS socket is destroyed.")));
            return false;
        }

        var queued = Interlocked.Add(ref _tlsQueuedWriteBytes, data.Length);
        if (queued >= TlsWriteHighWaterMark)
            Interlocked.Exchange(ref _tlsNeedsDrain, 1);
        _tlsWriteQueue.Add(new TlsWriteRequest(data, callback, null));
        StartTlsWriteLoop();
        return queued < TlsWriteHighWaterMark;
    }

    public new bool write(Buffer data, Action<Exception?>? callback = null) =>
        write(data.InternalData, callback);

    /// <summary>
    /// Writes string data to the TLS stream.
    /// </summary>
    public new bool write(string data, string? encoding = null, Action<Exception?>? callback = null)
    {
        var bytes = System.Text.Encoding.UTF8.GetBytes(data);
        return write(bytes, callback);
    }

    public new TLSSocket end(Action? callback = null)
    {
        if (!destroyed)
        {
            _tlsWriteQueue.Add(new TlsWriteRequest(null, null, callback));
            StartTlsWriteLoop();
        }
        return this;
    }

    public new TLSSocket end(byte[] data, Action? callback = null)
    {
        write(data, error =>
        {
            if (error is null)
                end(callback);
        });
        return this;
    }

    public new TLSSocket end(Buffer data, Action? callback = null) =>
        end(data.InternalData, callback);

    public new TLSSocket end(string data, string? encoding = null, Action? callback = null)
    {
        var bytes = Encoding.GetEncoding(encoding ?? "utf-8").GetBytes(data);
        return end(bytes, callback);
    }

    private void StartTlsWriteLoop()
    {
        lock (_tlsWriteLoopLock)
        {
            if (_tlsWriteLoopStarted)
                return;
            _tlsWriteLoopStarted = true;
        }

        BackgroundDispatch.RunHandleOwnedAsync(async () =>
        {
            try
            {
                foreach (var request in _tlsWriteQueue.GetConsumingEnumerable())
                {
                    _tlsReady.Wait();
                    if (_tlsHandshakeError is not null)
                        throw new InvalidOperationException("TLS handshake failed.", _tlsHandshakeError);
                    if (_sslStream is null || !_sslStream.CanWrite)
                        throw new InvalidOperationException("TLS stream is not writable.");

                    if (request.Data is null)
                    {
                        await _sslStream.FlushAsync();
                        _sslStream.Close();
                        Tsonic.CSharp.Js.JsEventLoop.EnqueueHandleOwned(() =>
                        {
                            emit("finish");
                            request.EndCallback?.Invoke();
                        });
                        destroy();
                        _tlsWriteQueue.CompleteAdding();
                        break;
                    }

                    try
                    {
                        await _sslStream.WriteAsync(request.Data, 0, request.Data.Length);
                        await _sslStream.FlushAsync();
                        if (request.Callback != null)
                            Tsonic.CSharp.Js.JsEventLoop.EnqueueHandleOwned(() => request.Callback(null));
                    }
                    catch (Exception error)
                    {
                        if (request.Callback != null)
                            Tsonic.CSharp.Js.JsEventLoop.EnqueueHandleOwned(() => request.Callback(error));
                        Tsonic.CSharp.Js.JsEventLoop.EnqueueHandleOwned(() => emit("error", error));
                    }
                    finally
                    {
                        var remaining = Interlocked.Add(ref _tlsQueuedWriteBytes, -request.Data.Length);
                        if (remaining < TlsWriteHighWaterMark && Interlocked.Exchange(ref _tlsNeedsDrain, 0) == 1)
                            Tsonic.CSharp.Js.JsEventLoop.EnqueueHandleOwned(() => emit("drain"));
                    }
                }
            }
            catch (Exception error)
            {
                Tsonic.CSharp.Js.JsEventLoop.EnqueueHandleOwned(() => emit("error", error));
            }
        });
    }

    /// <summary>
    /// Reads data from the TLS stream.
    /// </summary>
    internal new void StartReading()
    {
        if (_sslStream == null || !_sslStream.CanRead)
            return;

        BackgroundDispatch.RunHandleOwnedAsync(async () =>
        {
            var buffer = new byte[8192];
            try
            {
                while (_sslStream.CanRead && !destroyed)
                {
                    var bytesRead = await _sslStream.ReadAsync(buffer, 0, buffer.Length);
                    if (bytesRead == 0)
                    {
                        Tsonic.CSharp.Js.JsEventLoop.EnqueueHandleOwned(() => emit("end"));
                        destroy();
                        break;
                    }

                    var data = new byte[bytesRead];
                    Array.Copy(buffer, data, bytesRead);
                    var chunk = Buffer.from(data);
                    Tsonic.CSharp.Js.JsEventLoop.EnqueueHandleOwned(() => emit("data", chunk));
                }
            }
            catch (Exception ex)
            {
                if (!destroyed)
                    destroy(ex);
            }
        });
    }

}
