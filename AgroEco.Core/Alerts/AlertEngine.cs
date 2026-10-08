using AgroEco.Core.Alerts.Events;
using AgroEco.Core.Alerts.Persistence;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.Concurrent;
using System.Threading;

namespace AgroEco.Core.Alerts;

public sealed class AlertEngine
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ConcurrentDictionary<string, IAlertChannel> _channels = new();
    private readonly AlertEngineOptions _options;

    public event Func<AlertRaisedEvent, Task>? AlertRaised;
    public event Func<AlertDeliveredEvent, Task>? AlertDelivered;

    public AlertEngine(IServiceScopeFactory scopeFactory, AlertEngineOptions? options = null)
    {
        _scopeFactory = scopeFactory;
        _options = options ?? AlertEngineOptions.Default;
    }

    public async Task<Result> RaiseAsync(Alert alert, CancellationToken cancellationToken = default)
    {
        try
        {
            if (alert.Id <= 0)
            {
                using var scope = _scopeFactory.CreateScope();
                var createAlert = scope.ServiceProvider.GetRequiredService<CreateAlert>();
                var createResult = await createAlert.HandleAsync(alert, cancellationToken);
                if (!createResult.Success)
                {
                    return createResult;
                }
            }

            var channelsToUse = ResolveChannels(alert);
            if (channelsToUse.Count == 0)
            {
                return Result.CreateSuccess("Alert stored; no delivery channels are enabled");
            }

            var raisedEvent = new AlertRaisedEvent(alert, channelsToUse.Select(c => c.ChannelType).ToArray());
            await InvokeEventSafelyAsync(AlertRaised, raisedEvent);

            var deliveryResults = new List<AlertDeliveryEntity>();

            // Secuencial por prioridad: canales de mayor prioridad (menor número) se ejecutan primero
            foreach (var channel in channelsToUse)
            {
                var result = await DeliverToChannelWithTimeoutAsync(channel, alert, cancellationToken);
                
                var delivery = new AlertDeliveryEntity
                {
                    AlertId = alert.Id,
                    ChannelType = channel.ChannelType,
                    Success = result.Success,
                    ErrorMessage = result.ErrorMessage,
                    AttemptedAt = result.AttemptedAt,
                    Duration = result.Duration
                };
                
                deliveryResults.Add(delivery);
                alert.AddDelivery(delivery);
            }

            alert.UpdateStatusFromDeliveries();

            var deliveredEvent = new AlertDeliveredEvent(alert, deliveryResults.Select(d => 
                new AlertDeliveryResult(d.ChannelType, d.Success, d.ErrorMessage, d.AttemptedAt, d.Duration)).ToArray());
            await InvokeEventSafelyAsync(AlertDelivered, deliveredEvent);

            if (alert.Id > 0)
            {
                await PersistDeliveriesAsync(alert, deliveryResults, cancellationToken);
            }

            return alert.Status == AlertStatus.Delivered || alert.Status == AlertStatus.PartiallyDelivered
                ? Result.CreateSuccess($"Alert delivered via {deliveryResults.Count(r => r.Success)} channel(s)")
                : Result.CreateFailure("Alert failed to deliver on all channels");
        }
        catch (OperationCanceledException)
        {
            alert.MarkCanceled();
            await PersistCurrentStateAsync(alert, cancellationToken);
            return Result.CreateFailure("Alert delivery was canceled");
        }
        catch (Exception ex)
        {
            alert.MarkFailed();
            await PersistCurrentStateAsync(alert, cancellationToken);
            return Result.CreateFailure($"Alert delivery failed: {ex.Message}", ex);
        }
    }

    private async Task<AlertDeliveryResult> DeliverToChannelWithTimeoutAsync(IAlertChannel channel, Alert alert, CancellationToken ct)
    {
        var timeout = _options.DefaultTimeout;
        using var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(ct);
        timeoutCts.CancelAfter(timeout);

        try
        {
            return await channel.SendAsync(alert, timeoutCts.Token);
        }
        catch (OperationCanceledException) when (ct.IsCancellationRequested)
        {
            throw;
        }
        catch (OperationCanceledException)
        {
            var sw = System.Diagnostics.Stopwatch.StartNew();
            sw.Stop();
            return AlertDeliveryResult.Failure(channel.ChannelType, $"Timeout after {timeout.TotalSeconds}s", sw.Elapsed);
        }
        catch (Exception ex)
        {
            var sw = System.Diagnostics.Stopwatch.StartNew();
            sw.Stop();
            return AlertDeliveryResult.Failure(channel.ChannelType, ex.Message, sw.Elapsed);
        }
    }

    private async Task PersistDeliveriesAsync(Alert alert, List<AlertDeliveryEntity> deliveries, CancellationToken ct)
    {
        using var scope = _scopeFactory.CreateScope();
        var updateAlert = scope.ServiceProvider.GetRequiredService<UpdateAlert>();
        await updateAlert.HandleAsync(alert, ct);
    }

    private async Task PersistCurrentStateAsync(Alert alert, CancellationToken ct)
    {
        if (alert.Id <= 0 || ct.IsCancellationRequested)
        {
            return;
        }

        using var scope = _scopeFactory.CreateScope();
        var updateAlert = scope.ServiceProvider.GetRequiredService<UpdateAlert>();
        await updateAlert.HandleAsync(alert, ct);
    }

    public async Task<Result> RaiseAsync(
        string title,
        string message,
        AlertLevel level = AlertLevel.Info,
        AlertConfiguration? configuration = null,
        CancellationToken cancellationToken = default)
    {
        var alert = Alert.Create(title, message, level, configuration);

        if (configuration is not null)
        {
            foreach (var option in configuration.ChannelOptions)
            {
                alert.Metadata[option.Key] = option.Value;
            }
        }

        return await RaiseAsync(alert, cancellationToken);
    }

    public Result RegisterChannel(IAlertChannel channel)
    {
        if (channel == null)
            return Result.CreateFailure("Channel cannot be null");

        if (string.IsNullOrWhiteSpace(channel.ChannelType))
            return Result.CreateFailure("Channel type cannot be empty");

        _channels.AddOrUpdate(channel.ChannelType, channel, (_, _) => channel);
        return Result.CreateSuccess($"Channel '{channel.ChannelType}' registered");
    }

    public Result UnregisterChannel(string channelType)
    {
        if (string.IsNullOrWhiteSpace(channelType))
            return Result.CreateFailure("Channel type cannot be empty");

        var removed = _channels.TryRemove(channelType, out _);
        return removed
            ? Result.CreateSuccess($"Channel '{channelType}' unregistered")
            : Result.CreateFailure($"Channel '{channelType}' not found");
    }

    public IReadOnlyList<IAlertChannel> GetRegisteredChannels()
        => _channels.Values.OrderBy(c => c.Priority).ToList();

    public IReadOnlyList<ChannelRuntimeSnapshot> GetChannelSnapshots()
    {
        return _channels.Values.Select(c => new ChannelRuntimeSnapshot(
            ChannelType: c.ChannelType,
            IsEnabled: c.IsEnabled,
            Priority: c.Priority
        )).ToList();
    }

    private List<IAlertChannel> ResolveChannels(Alert alert)
    {
        return alert.Configuration.EnabledChannels
            .Select(channelType => _channels.TryGetValue(channelType, out var channel)
                ? channel
                : null)
            .Where(channel => channel is not null && channel.IsEnabled)
            .Cast<IAlertChannel>()
            .OrderBy(channel => channel.Priority)
            .ToList();
    }

    private static async Task InvokeEventSafelyAsync<T>(Func<T, Task>? eventDelegate, T args)
    {
        if (eventDelegate == null) return;

        var handlers = eventDelegate.GetInvocationList();
        var tasks = handlers
            .Cast<Func<T, Task>>()
            .Select(handler => InvokeHandlerSafelyAsync(handler, args));
        await Task.WhenAll(tasks);
    }

    private static async Task InvokeHandlerSafelyAsync<T>(Func<T, Task> handler, T args)
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

public sealed class AlertEngineOptions
{
    public TimeSpan DefaultTimeout { get; set; } = TimeSpan.FromSeconds(30);

    public static AlertEngineOptions Default => new();
}

public sealed record ChannelRuntimeSnapshot(
    string ChannelType,
    bool IsEnabled,
    int Priority
);