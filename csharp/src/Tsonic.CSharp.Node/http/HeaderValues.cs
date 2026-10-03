using System;
using Microsoft.Extensions.Primitives;

namespace Tsonic.CSharp.Node.Http;

/// <summary>Exact indexed reads from native HTTP header storage.</summary>
public static class HeaderValues
{
    /// <summary>Reads the current native value, rejecting absent payloads.</summary>
    public static string read(StringValues values, int index) =>
        values[index] ?? throw new InvalidOperationException("Native HTTP header value is null.");
}
