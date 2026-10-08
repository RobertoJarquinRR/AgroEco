using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace AgroEco.Core.Triggers.Engine;

public sealed class TriggerNotifier
{
    private readonly ILogger _logger;

    public TriggerNotifier(ILogger<TriggerNotifier>? logger = null)
    {
        _logger = logger ?? NullLogger<TriggerNotifier>.Instance;
    }

    public async Task InvokeEventSafelyAsync<T>(Func<T, Task>? eventDelegate, T args)
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

    private async Task InvokeHandlerSafelyAsync<T>(Func<T, Task> handler, T args)
    {
        try
        {
            await handler(args);
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Exception in event handler");
        }
    }
}