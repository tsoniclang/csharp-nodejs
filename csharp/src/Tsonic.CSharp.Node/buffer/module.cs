using System;
using System.Text;

namespace Tsonic.CSharp.Node;

/// <summary>
/// Buffer module-level helpers.
/// </summary>
public static class buffer
{
    private static readonly BufferConstants _constants = new();
    private static readonly UTF8Encoding _strictUtf8 = new(false, true);

    /// <summary>
    /// Maximum number of bytes for util.inspect truncation.
    /// </summary>
    public static int INSPECT_MAX_BYTES { get; set; } = 50;

    /// <summary>
    /// Buffer constants object.
    /// </summary>
    public static BufferConstants constants => _constants;

    /// <summary>
    /// Maximum buffer length.
    /// </summary>
    public static int kMaxLength => int.MaxValue;

    /// <summary>
    /// Maximum string length.
    /// </summary>
    public static int kStringMaxLength => int.MaxValue / 2;

    /// <summary>
    /// SlowBuffer compatibility helper.
    /// </summary>
    public static Buffer SlowBuffer(int size)
    {
        return Buffer.alloc(size);
    }

    /// <summary>
    /// Decodes base64 to a latin1 string.
    /// </summary>
    public static string atob(string data)
    {
        if (data == null)
            throw new ArgumentNullException(nameof(data));

        var bytes = Convert.FromBase64String(data);
        return Encoding.Latin1.GetString(bytes);
    }

    /// <summary>
    /// Encodes a latin1 string to base64.
    /// </summary>
    public static string btoa(string data)
    {
        if (data == null)
            throw new ArgumentNullException(nameof(data));

        var bytes = Encoding.Latin1.GetBytes(data);
        return Convert.ToBase64String(bytes);
    }

    /// <summary>
    /// Returns true if all bytes are ASCII.
    /// </summary>
    public static bool isAscii(Buffer value)
    {
        if (value == null)
            throw new ArgumentNullException(nameof(value));

        return isAscii(value.InternalData);
    }

    /// <summary>
    /// Returns true if all bytes are ASCII.
    /// </summary>
    public static bool isAscii(byte[] value)
    {
        if (value == null)
            throw new ArgumentNullException(nameof(value));

        foreach (var b in value)
        {
            if (b > 0x7F) return false;
        }

        return true;
    }

    /// <summary>
    /// Returns true if bytes are valid UTF-8.
    /// </summary>
    public static bool isUtf8(Buffer value)
    {
        if (value == null)
            throw new ArgumentNullException(nameof(value));

        return isUtf8(value.InternalData);
    }

    /// <summary>
    /// Returns true if bytes are valid UTF-8.
    /// </summary>
    public static bool isUtf8(byte[] value)
    {
        if (value == null)
            throw new ArgumentNullException(nameof(value));

        try
        {
            _ = _strictUtf8.GetString(value);
            return true;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Transcodes a buffer between encodings.
    /// </summary>
    public static Buffer transcode(Buffer source, string fromEncoding, string toEncoding)
    {
        if (source == null)
            throw new ArgumentNullException(nameof(source));

        var srcEncoding = GetEncoding(fromEncoding);
        var dstEncoding = GetEncoding(toEncoding);

        var text = srcEncoding.GetString(source.InternalData);
        return Buffer.from(dstEncoding.GetBytes(text));
    }

    /// <summary>
    /// resolveObjectURL is currently not implemented.
    /// </summary>
    public static object? resolveObjectURL(string id)
    {
        _ = id;
        return null;
    }

    private static Encoding GetEncoding(string encoding)
    {
        if (string.IsNullOrWhiteSpace(encoding))
            return Encoding.UTF8;

        var normalized = encoding.ToLowerInvariant().Replace("-", "").Replace("_", "");
        return normalized switch
        {
            "utf8" => Encoding.UTF8,
            "ascii" => Encoding.ASCII,
            "latin1" => Encoding.Latin1,
            "binary" => Encoding.Latin1,
            "base64" => Encoding.UTF8,
            "base64url" => Encoding.UTF8,
            "hex" => Encoding.UTF8,
            "utf16le" => Encoding.Unicode,
            "ucs2" => Encoding.Unicode,
            _ => throw new ArgumentException($"Unknown encoding: {encoding}", nameof(encoding)),
        };
    }
}

/// <summary>
/// Constants for the buffer module.
/// </summary>
public class BufferConstants
{
    /// <summary>
    /// Maximum allowed Buffer length.
    /// </summary>
    public int MAX_LENGTH => buffer.kMaxLength;

    /// <summary>
    /// Maximum allowed string length.
    /// </summary>
    public int MAX_STRING_LENGTH => buffer.kStringMaxLength;
}
