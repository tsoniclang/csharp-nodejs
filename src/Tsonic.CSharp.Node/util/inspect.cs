using System.Globalization;
using System.Text;
using Tsonic.CSharp.Js;

namespace Tsonic.CSharp.Node;

public static partial class util
{
    /// <summary>
    /// Returns a string representation of an object that is intended for debugging.
    /// </summary>
    /// <param name="obj">The object to inspect.</param>
    /// <returns>A string representation of the object.</returns>
    public static string inspect(object? obj)
    {
        return inspectClosedCarrier(obj);
    }

    private static string inspectClosedCarrier(object? obj)
    {
        obj = obj is TsValue tsValue ? tsValue.unwrap() : obj;
        obj = obj is TsUnion union ? union.value() : obj;

        return obj switch
        {
            null => "null",
            JSUndefined => "undefined",
            string text => $"'{text}'",
            bool value => value ? "true" : "false",
            char value => $"'{value}'",
            byte value => value.ToString(CultureInfo.InvariantCulture),
            sbyte value => value.ToString(CultureInfo.InvariantCulture),
            short value => value.ToString(CultureInfo.InvariantCulture),
            ushort value => value.ToString(CultureInfo.InvariantCulture),
            int value => value.ToString(CultureInfo.InvariantCulture),
            uint value => value.ToString(CultureInfo.InvariantCulture),
            long value => value.ToString(CultureInfo.InvariantCulture),
            ulong value => value.ToString(CultureInfo.InvariantCulture),
            float value => value.ToString(CultureInfo.InvariantCulture),
            double value => value.ToString(CultureInfo.InvariantCulture),
            decimal value => value.ToString(CultureInfo.InvariantCulture),
            JSObject value => inspectObject(value.entries()),
            TsObject value => inspectObject(value.entries()),
            TsArray value => inspectArray(value.entries(), value.length),
            IJSArray value => inspectJsArray(value),
            _ => throw UnsupportedOpenCarrierOperation("node:util.inspect")
        };
    }

    private static string inspectObject(IEnumerable<(string key, object? value)> entries)
    {
        var builder = new StringBuilder();
        builder.Append("{ ");
        var first = true;
        foreach (var entry in entries)
        {
            if (!first)
            {
                builder.Append(", ");
            }
            first = false;
            builder.Append(entry.key);
            builder.Append(": ");
            builder.Append(inspectClosedCarrier(entry.value));
        }
        builder.Append(" }");
        return builder.ToString();
    }

    private static string inspectObject(IEnumerable<KeyValuePair<string, object?>> entries)
    {
        var builder = new StringBuilder();
        builder.Append("{ ");
        var first = true;
        foreach (var entry in entries)
        {
            if (!first)
            {
                builder.Append(", ");
            }
            first = false;
            builder.Append(entry.Key);
            builder.Append(": ");
            builder.Append(inspectClosedCarrier(entry.Value));
        }
        builder.Append(" }");
        return builder.ToString();
    }

    private static string inspectArray(IEnumerable<KeyValuePair<string, object?>> entries, int length)
    {
        var slots = new string[length];
        for (var index = 0; index < slots.Length; index++)
        {
            slots[index] = "<empty>";
        }
        foreach (var entry in entries)
        {
            if (int.TryParse(entry.Key, NumberStyles.None, CultureInfo.InvariantCulture, out var index) && index >= 0 && index < slots.Length)
            {
                slots[index] = inspectClosedCarrier(entry.Value);
            }
        }
        return $"[ {string.Join(", ", slots)} ]";
    }

    private static string inspectJsArray(IJSArray array)
    {
        var slots = new string[array.length];
        for (var index = 0; index < slots.Length; index++)
        {
            slots[index] = array.tryGetAtObject(index, out var value) ? inspectClosedCarrier(value) : "<empty>";
        }
        return $"[ {string.Join(", ", slots)} ]";
    }
}
