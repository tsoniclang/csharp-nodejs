namespace Tsonic.CSharp.Node;

public static partial class os
{
    /// <summary>
    /// Returns a string identifying the endianness of the CPU for which the Node.js binary was compiled.
    /// Possible values are 'BE' for big endian and 'LE' for little endian.
    /// </summary>
    /// <returns>'BE' or 'LE'</returns>
    public static string endianness()
    {
        return BitConverter.IsLittleEndian ? "LE" : "BE";
    }
}
