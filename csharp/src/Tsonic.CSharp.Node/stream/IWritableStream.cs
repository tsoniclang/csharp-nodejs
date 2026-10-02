namespace Tsonic.CSharp.Node;

/// <summary>Native write and finalization operations shared by writable and duplex streams.</summary>
public interface IWritableStream
{
    /// <summary>Accepts a native chunk and reports whether producers may continue writing.</summary>
    bool WriteChunk(object? chunk);
    /// <summary>Finalizes the writable side after its accepted chunks.</summary>
    void EndChunk();
}
