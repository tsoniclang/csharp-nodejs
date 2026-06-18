namespace Tsonic.CSharp.Node;

public partial class StringDecoder
{
    /// <summary>
    /// Returns a decoded string, ensuring that any incomplete multibyte characters at
    /// the end of the Buffer are omitted from the returned string and stored in an
    /// internal buffer for the next call to write() or end().
    /// </summary>
    /// <param name="buffer">The bytes to decode.</param>
    /// <returns>The decoded string.</returns>
    public string write(byte[] buffer)
    {
        if (buffer == null || buffer.Length == 0)
            return string.Empty;

        // Use the decoder with flush: false to preserve incomplete sequences
        int charCount = _decoder.GetCharCount(buffer, 0, buffer.Length, false);
        char[] chars = new char[charCount];
        _decoder.GetChars(buffer, 0, buffer.Length, chars, 0, false);

        return new string(chars);
    }
}
