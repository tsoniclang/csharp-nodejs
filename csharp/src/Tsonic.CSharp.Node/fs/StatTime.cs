namespace Tsonic.CSharp.Node;

internal static class StatTime
{
    public static Tsonic.CSharp.Js.Date ToJsDate(DateTime value)
    {
        return new Tsonic.CSharp.Js.Date(ToUnixMilliseconds(value));
    }

    public static double ToUnixMilliseconds(DateTime value)
    {
        return new DateTimeOffset(value.ToUniversalTime()).ToUnixTimeMilliseconds();
    }
}
