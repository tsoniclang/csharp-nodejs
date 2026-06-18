namespace Tsonic.CSharp.Node;

public partial class StringDecoder
{
    /// <summary>
    /// Returns any remaining input stored in the internal buffer as a string.
    /// Bytes representing incomplete UTF-8 and UTF-16 characters will be replaced
    /// with substitution characters appropriate for the character encoding.
    /// After end() is called, the StringDecoder object can be reused for new input.
    /// </summary>
    /// <param name="buffer">Optional bytes to decode before returning remaining input.</param>
    /// <returns>The decoded string including any remaining buffered bytes.</returns>
    public string end(byte[]? buffer = null)
    {
        string result = string.Empty;

        if (buffer != null && buffer.Length > 0)
        {
            // Decode with flush: true to output any remaining bytes
            int charCount = _decoder.GetCharCount(buffer, 0, buffer.Length, true);
            char[] chars = new char[charCount];
            _decoder.GetChars(buffer, 0, buffer.Length, chars, 0, true);
            result = new string(chars);
        }
        else
        {
            // Flush any remaining incomplete bytes
            int charCount = _decoder.GetCharCount(Array.Empty<byte>(), 0, 0, true);
            if (charCount > 0)
            {
                char[] chars = new char[charCount];
                _decoder.GetChars(Array.Empty<byte>(), 0, 0, chars, 0, true);
                result = new string(chars);
            }
        }

        // Reset the decoder for potential reuse
        _decoder.Reset();

        return result;
    }
}
