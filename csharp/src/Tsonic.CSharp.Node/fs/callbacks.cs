namespace Tsonic.CSharp.Node;

#pragma warning disable CS1591

public static partial class fs
{
    private static Task CallbackTask(Action action, Action<Exception?>? callback)
    {
        try
        {
            action();
            callback?.Invoke(null);
            return Task.CompletedTask;
        }
        catch (Exception ex)
        {
            callback?.Invoke(ex);
            return Task.FromException(ex);
        }
    }
}
