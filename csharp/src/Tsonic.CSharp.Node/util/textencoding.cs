#pragma warning disable CS1591
#pragma warning disable IDE1006

using System;
using System.Text;

namespace Tsonic.CSharp.Node;

public sealed class TextEncoderEncodeIntoResult
{
    public int read { get; set; }

    public int written { get; set; }
}

public sealed class TextEncoder
{
    public string encoding => "utf-8";

    public byte[] encode(string input = "")
    {
        return Encoding.UTF8.GetBytes(input);
    }

    public TextEncoderEncodeIntoResult encodeInto(string source, Span<byte> destination)
    {
        var encoded = Encoding.UTF8.GetBytes(source);
        var written = Math.Min(encoded.Length, destination.Length);
        encoded.AsSpan(0, written).CopyTo(destination);

        return new TextEncoderEncodeIntoResult
        {
            read = Encoding.UTF8.GetCharCount(destination[..written]),
            written = written
        };
    }

    public TextEncoderEncodeIntoResult encodeInto(string source, byte[] destination)
    {
        return encodeInto(source, destination.AsSpan());
    }
}

public sealed class TextDecoderOptions
{
    public bool fatal { get; set; }

    public bool ignoreBOM { get; set; }
}

public sealed class TextDecodeOptions
{
    public bool stream { get; set; }
}

public sealed class TextDecoder
{
    private readonly Encoding _encoding;

    public TextDecoder(string label = "utf-8", TextDecoderOptions? options = null)
    {
        _encoding = ResolveEncoding(label, options?.fatal ?? false);
        encoding = NormalizeEncodingName(_encoding);
        fatal = options?.fatal ?? false;
        ignoreBOM = options?.ignoreBOM ?? false;
    }

    public string encoding { get; }

    public bool fatal { get; }

    public bool ignoreBOM { get; }

    public string decode(byte[]? input = null, TextDecodeOptions? options = null)
    {
        _ = options;
        if (input == null || input.Length == 0)
            return string.Empty;

        var bytes = input.AsSpan();
        if (!ignoreBOM && bytes.Length >= 3 && bytes[0] == 0xEF && bytes[1] == 0xBB && bytes[2] == 0xBF)
            bytes = bytes[3..];

        return _encoding.GetString(bytes);
    }

    public string decode(Buffer input, TextDecodeOptions? options = null)
    {
        if (input == null)
            throw new ArgumentNullException(nameof(input));

        return decode(input.InternalData, options);
    }

    public static TextDecoder newFromOptions(TextDecoderOptions options)
    {
        return new TextDecoder("utf-8", options);
    }

    private static Encoding ResolveEncoding(string label, bool fatal)
    {
        var fallback = fatal ? EncoderFallback.ExceptionFallback : EncoderFallback.ReplacementFallback;
        var decoderFallback = fatal ? DecoderFallback.ExceptionFallback : DecoderFallback.ReplacementFallback;
        var normalized = label.Trim().ToLowerInvariant().Replace("_", "-", StringComparison.Ordinal);

        return normalized switch
        {
            "utf-8" or "utf8" => new UTF8Encoding(false, fatal),
            "utf-16le" or "utf-16" or "ucs-2" or "ucs2" => new UnicodeEncoding(false, false, fatal),
            "latin1" or "iso-8859-1" or "binary" => Encoding.GetEncoding("iso-8859-1", fallback, decoderFallback),
            "ascii" or "us-ascii" => Encoding.GetEncoding("us-ascii", fallback, decoderFallback),
            _ => throw new ArgumentException($"Unsupported text encoding: {label}", nameof(label))
        };
    }

    private static string NormalizeEncodingName(Encoding encoding)
    {
        if (encoding.CodePage == Encoding.UTF8.CodePage)
            return "utf-8";

        if (encoding.CodePage == Encoding.Unicode.CodePage)
            return "utf-16le";

        if (encoding.WebName.Equals("iso-8859-1", StringComparison.OrdinalIgnoreCase))
            return "windows-1252";

        return encoding.WebName;
    }
}
