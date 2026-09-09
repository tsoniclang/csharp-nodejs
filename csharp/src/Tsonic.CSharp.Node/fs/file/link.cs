namespace Tsonic.CSharp.Node;

#pragma warning disable CS1591

public static partial class fs
{
    public static void linkSync(string existingPath, string newPath)
    {
        File.Copy(existingPath, newPath, overwrite: false);
    }
    public static Task link(string existingPath, string newPath, Action<Exception?>? callback = null) => CallbackTask(() => linkSync(existingPath, newPath), callback);
}
