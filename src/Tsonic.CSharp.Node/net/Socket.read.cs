using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Tsonic.CSharp.Node;

#pragma warning disable CS8981 // Lowercase type names
#pragma warning disable IDE1006 // Naming rule violation

/// <summary>
/// This class is an abstraction of a TCP socket or a streaming IPC endpoint.
/// It is also an EventEmitter.
/// </summary>
public partial class Socket : Stream
{
    /// <summary>
    /// Starts the asynchronous read loop to emit "data" events.
    /// Called internally after the connection event is emitted to allow handlers to be registered first.
    /// </summary>
    internal void StartReading()
    {
        if (_reading || _stream == null) return;
        _reading = true;

        BackgroundDispatch.RunAsync(async () =>
        {
            var buffer = new byte[65536]; // 64KB buffer
            try
            {
                while (!_destroyed && _stream != null && _client?.Connected == true)
                {
                    // Wait while paused
                    while (_paused && !_destroyed)
                    {
                        await Task.Delay(10);
                    }

                    if (_destroyed) break;

                    int bytesReadCount;
                    try
                    {
                        bytesReadCount = await _stream.ReadAsync(buffer, 0, buffer.Length);
                    }
                    catch (ObjectDisposedException)
                    {
                        // Stream was closed
                        break;
                    }
                    catch (System.IO.IOException)
                    {
                        // Connection reset or closed
                        break;
                    }

                    if (bytesReadCount == 0)
                    {
                        // End of stream - connection closed by remote
                        emit("end");
                        if (!_allowHalfOpen)
                        {
                            end();
                        }
                        break;
                    }

                    bytesRead += bytesReadCount;

                    // Create a Buffer with the received data
                    var data = new byte[bytesReadCount];
                    System.Array.Copy(buffer, 0, data, 0, bytesReadCount);
                    var nodeBuffer = Buffer.from(data);

                    emit("data", nodeBuffer);
                }
            }
            catch (Exception ex)
            {
                if (!_destroyed)
                {
                    emit("error", ex);
                }
            }
            finally
            {
                _reading = false;
                if (!_destroyed)
                {
                    emit("close", false);
                }
            }
        });
    }

    private void UpdateAddressInfo()
    {
        if (_client?.Client?.LocalEndPoint is IPEndPoint localEP)
        {
            localAddress = localEP.Address.ToString();
            localPort = localEP.Port;
            localFamily = localEP.AddressFamily == AddressFamily.InterNetwork ? "IPv4" : "IPv6";
        }

        if (_client?.Client?.RemoteEndPoint is IPEndPoint remoteEP)
        {
            remoteAddress = remoteEP.Address.ToString();
            remotePort = remoteEP.Port;
            remoteFamily = remoteEP.AddressFamily == AddressFamily.InterNetwork ? "IPv4" : "IPv6";
        }
    }

    /// <summary>
    /// Gets the underlying TcpClient (for TLS wrapping).
    /// </summary>
    internal TcpClient? GetTcpClient()
    {
        return _client;
    }
}
