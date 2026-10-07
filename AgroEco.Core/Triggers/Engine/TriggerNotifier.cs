namespace AgroEco.Core.Triggers.Engine;

public static class TriggerNotifier
{
    public static async Task InvokeEventSafelyAsync<T>(Func<T, Task>? eventDelegate, T args)
    {
        if (eventDelegate is null)
        {
            return;
        }

        var handlers = eventDelegate.GetInvocationList();
        var tasks = handlers
            .Cast<Func<T, Task>>()
            .Select(handler => InvokeHandlerSafelyAsync(handler, args));
        await Task.WhenAll(tasks);
    }

    public static async Task InvokeHandlerSafelyAsync<T>(Func<T, Task> handler, T args)
    {
        try
        {
            await handler(args);
        }
        catch
        {
        }
    }
}