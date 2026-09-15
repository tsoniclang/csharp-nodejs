namespace Tsonic.CSharp.Node;

public static class v8
{
    public static void setFlagsFromString(string flags)
    {
        throw new System.PlatformNotSupportedException(
            "node:v8.setFlagsFromString requires a V8 engine; native programs do not host V8");
    }
}
