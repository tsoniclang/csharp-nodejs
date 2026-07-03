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
    /// Sends data on the socket.
    /// </summary>
    /// <param name="data">Data to write</param>
    /// <param name="callback">Callback when write completes</param>
    /// <returns>True if flushed to kernel buffer</returns>
    public bool write(byte[] data, Action<Exception?>? callback = null)
    {
        if (_stream == null || _destroyed)
        {
            callback?.Invoke(new InvalidOperationException("Socket not connected"));
            return false;
        }

        // Queue the write request for FIFO processing (like Node.js)
        _writeQueueEmpty.Reset();
        _writeQueue.Add(new WriteRequest(data, callback));

        // Start the write loop if not already running
        StartWriteLoop();

        return true;
    }

    /// <summary>
    /// Starts the write loop that processes queued writes in FIFO order.
    /// </summary>
    private void StartWriteLoop()
    {
        lock (_writeLoopLock)
        {
            if (_writeLoopStarted) return;
            _writeLoopStarted = true;
        }

        _writeLoopTask = BackgroundDispatch.RunAsync(async () =>
        {
            try
            {
                foreach (var request in _writeQueue.GetConsumingEnumerable())
                {
                    if (_destroyed || _stream == null) break;

                    try
                    {
                        await _stream.WriteAsync(request.Data, 0, request.Data.Length);
                        bytesWritten += request.Data.Length;
                        request.Callback?.Invoke(null);
                        emit("drain");
                    }
                    catch (Exception ex)
                    {
                        request.Callback?.Invoke(ex);
                        emit("error", ex);
                    }

                    // Signal if queue is empty
                    if (_writeQueue.Count == 0)
                    {
                        _writeQueueEmpty.Set();
                    }
                }
            }
            catch (InvalidOperationException)
            {
                // Queue was completed, this is expected during shutdown
            }
            finally
            {
                _writeQueueEmpty.Set();
            }
        });
    }

    /// <summary>
    /// Sends string data on the socket.
    /// </summary>
    /// <param name="data">String data to write</param>
    /// <param name="encoding">Encoding to use</param>
    /// <param name="callback">Callback when write completes</param>
    /// <returns>True if flushed to kernel buffer</returns>
    public bool write(string data, string? encoding = null, Action<Exception?>? callback = null)
    {
        var enc = encoding == null ? Encoding.UTF8 : Encoding.GetEncoding(encoding);
        return write(enc.GetBytes(data), callback);
    }

    /// <summary>
    /// Half-closes the socket.
    /// </summary>
    /// <param name="callback">Callback when finished</param>
    /// <returns>The socket itself</returns>
    public Socket end(Action? callback = null)
    {
        if (_stream != null && !_destroyed)
        {
            // Wait for pending writes to complete before closing (like Node.js)
            BackgroundDispatch.Run(() =>
            {
                // Wait for write queue to be empty with timeout
                _writeQueueEmpty.Wait(TimeSpan.FromSeconds(30));

                // Complete the write queue to stop the write loop
                _writeQueue.CompleteAdding();

                _stream?.Close();
                emit("end");
                callback?.Invoke();
            });
        }
        return this;
    }

    /// <summary>
    /// Half-closes the socket after writing data.
    /// </summary>
    /// <param name="data">Data to write before closing</param>
    /// <param name="callback">Callback when finished</param>
    /// <returns>The socket itself</returns>
    public Socket end(byte[] data, Action? callback = null)
    {
        write(data, (err) =>
        {
            if (err == null)
            {
                end(callback);
            }
        });
        return this;
    }

    /// <summary>
    /// Half-closes the socket after writing string data.
    /// </summary>
    /// <param name="data">String data to write before closing</param>
    /// <param name="encoding">Encoding to use</param>
    /// <param name="callback">Callback when finished</param>
    /// <returns>The socket itself</returns>
    public Socket end(string data, string? encoding = null, Action? callback = null)
    {
        var enc = encoding == null ? Encoding.UTF8 : Encoding.GetEncoding(encoding);
        return end(enc.GetBytes(data), callback);
    }

}
