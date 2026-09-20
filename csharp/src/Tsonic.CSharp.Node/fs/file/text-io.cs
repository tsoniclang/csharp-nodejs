using System.Buffers;
using System.Text;

namespace Tsonic.CSharp.Node;

public static partial class fs
{
    private const int TextReadBufferSize = 16 * 1024;

    private static StreamReader OpenTextReader(string path, Encoding encoding, bool asynchronous)
    {
        var stream = new FileStream(path, new FileStreamOptions
        {
            Mode = FileMode.Open,
            Access = FileAccess.Read,
            Share = FileShare.Read,
            BufferSize = 1,
            Options = FileOptions.SequentialScan | (asynchronous ? FileOptions.Asynchronous : FileOptions.None)
        });
        return new StreamReader(stream, encoding, detectEncodingFromByteOrderMarks: true, TextReadBufferSize);
    }

    private const int TextWriteBufferSize = 128 * 1024;

    private static FileStream OpenTextWriter(string path, bool asynchronous) => new(path, new FileStreamOptions
    {
        Mode = FileMode.Create,
        Access = FileAccess.Write,
        Share = FileShare.Read,
        BufferSize = 1,
        Options = asynchronous ? FileOptions.Asynchronous : FileOptions.None
    });

    private static void WriteText(string path, string data, Encoding encoding)
    {
        using var stream = OpenTextWriter(path, asynchronous: false);
        var bytes = ArrayPool<byte>.Shared.Rent(TextWriteBufferSize);
        try
        {
            var encoder = encoding.GetEncoder();
            var offset = 0;
            var preamble = encoding.Preamble.Length;
            encoding.Preamble.CopyTo(bytes);
            bool completed;
            do
            {
                encoder.Convert(data.AsSpan(offset), bytes.AsSpan(preamble, TextWriteBufferSize - preamble), true,
                    out var charsUsed, out var bytesUsed, out completed);
                stream.Write(bytes.AsSpan(0, preamble + bytesUsed));
                offset += charsUsed;
                preamble = 0;
            } while (!completed);
        }
        finally
        {
            ArrayPool<byte>.Shared.Return(bytes);
        }
    }

    private static async Task WriteTextAsync(string path, string data, Encoding encoding)
    {
        await using var stream = OpenTextWriter(path, asynchronous: true);
        var bytes = ArrayPool<byte>.Shared.Rent(TextWriteBufferSize);
        try
        {
            var encoder = encoding.GetEncoder();
            var offset = 0;
            var preamble = encoding.Preamble.Length;
            encoding.Preamble.CopyTo(bytes);
            bool completed;
            do
            {
                encoder.Convert(data.AsSpan(offset), bytes.AsSpan(preamble, TextWriteBufferSize - preamble), true,
                    out var charsUsed, out var bytesUsed, out completed);
                await stream.WriteAsync(bytes.AsMemory(0, preamble + bytesUsed)).ConfigureAwait(false);
                offset += charsUsed;
                preamble = 0;
            } while (!completed);
        }
        finally
        {
            ArrayPool<byte>.Shared.Return(bytes);
        }
    }
}
