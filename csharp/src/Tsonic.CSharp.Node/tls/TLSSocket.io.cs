using System;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

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
        if (_sslStream == null || !_sslStream.CanWrite)
        {
            callback?.Invoke(new Exception("Stream not writable"));
            return false;
        }

        Task.Run(async () =>
        {
            try
            {
                await _sslStream.WriteAsync(data, 0, data.Length);
                await _sslStream.FlushAsync();
                callback?.Invoke(null);
            }
            catch (Exception ex)
            {
                callback?.Invoke(ex);
                emit("error", ex);
            }
        });

        return true;
    }

    /// <summary>
    /// Writes string data to the TLS stream.
    /// </summary>
    public new bool write(string data, string? encoding = null, Action<Exception?>? callback = null)
    {
        var bytes = System.Text.Encoding.UTF8.GetBytes(data);
        return write(bytes, callback);
    }

    /// <summary>
    /// Reads data from the TLS stream.
    /// </summary>
    internal new void StartReading()
    {
        if (_sslStream == null || !_sslStream.CanRead)
            return;

        Task.Run(async () =>
        {
            var buffer = new byte[8192];
            try
            {
                while (_sslStream.CanRead && !destroyed)
                {
                    var bytesRead = await _sslStream.ReadAsync(buffer, 0, buffer.Length);
                    if (bytesRead == 0)
                    {
                        // Connection closed
                        emit("end");
                        emit("close", false);
                        break;
                    }

                    var data = new byte[bytesRead];
                    Array.Copy(buffer, data, bytesRead);
                    emit("data", data);
                }
            }
            catch (Exception ex)
            {
                if (!destroyed)
                {
                    emit("error", ex);
                }
            }
        });
    }

}
