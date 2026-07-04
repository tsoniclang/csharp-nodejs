using System.Collections;
using System.Threading.Tasks;

namespace Tsonic.CSharp.Node;

#pragma warning disable CS1591
#pragma warning disable IDE1006

public static class util_types
{
    public static bool isArrayBuffer(object? value)
    {
        return value is byte[] or Buffer;
    }

    public static bool isAsyncFunction(object? value)
    {
        return value is Task;
    }

    public static bool isDate(object? value)
    {
        return value is System.DateTime or System.DateTimeOffset;
    }

    public static bool isMap(object? value)
    {
        return value is IDictionary;
    }

    public static bool isSet(object? value)
    {
        return value is ISet<object?>;
    }

    public static bool isPromise(object? value)
    {
        return value is Task;
    }

    public static bool isRegExp(object? value)
    {
        return value is System.Text.RegularExpressions.Regex;
    }

    public static bool isTypedArray(object? value)
    {
        return value is byte[] or sbyte[] or short[] or ushort[] or int[] or uint[] or long[] or ulong[] or float[] or double[];
    }
}
