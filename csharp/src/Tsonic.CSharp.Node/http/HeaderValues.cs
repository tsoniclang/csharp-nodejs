using System;
using Microsoft.Extensions.Primitives;

namespace Tsonic.CSharp.Node.Http;

public static class HeaderValues
{
    public static string read(StringValues values, int index) =>
        values[index] ?? throw new InvalidOperationException("Native HTTP header value is null.");
}
