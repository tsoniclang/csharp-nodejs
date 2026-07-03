using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Tsonic.CSharp.Node;

#pragma warning disable CS1591
#pragma warning disable IDE1006

public static class stream_consumers
{
    public static Task<Buffer> buffer(Readable readable)
    {
        return Task.FromResult(Buffer.from(ReadAllBytes(readable)));
    }

    public static Task<string> text(Readable readable)
    {
        return Task.FromResult(Encoding.UTF8.GetString(ReadAllBytes(readable)));
    }

    public static Task<byte[]> arrayBuffer(Readable readable)
    {
        return Task.FromResult(ReadAllBytes(readable));
    }

    public static Task<Buffer> blob(Readable readable)
    {
        return buffer(readable);
    }

    public static async Task<JsonDocument> json(Readable readable)
    {
        var content = await text(readable).ConfigureAwait(false);
        return JsonDocument.Parse(content);
    }

    private static byte[] ReadAllBytes(Readable readable)
    {
        if (readable == null)
            throw new System.ArgumentNullException(nameof(readable));

        var bytes = new List<byte>();
        object? chunk;
        while ((chunk = readable.read()) != null)
        {
            switch (chunk)
            {
                case Buffer buffer:
                    for (var i = 0; i < buffer.length; i++)
                        bytes.Add(buffer[i]);
                    break;
                case byte[] array:
                    bytes.AddRange(array);
                    break;
                case string text:
                    bytes.AddRange(Encoding.UTF8.GetBytes(text));
                    break;
                default:
                    bytes.AddRange(Encoding.UTF8.GetBytes(chunk.ToString() ?? string.Empty));
                    break;
            }
        }

        return bytes.ToArray();
    }
}
