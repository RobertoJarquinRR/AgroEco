using AgroEco.Core.Alerts;
using AgroEco.Core.Alerts.Persistence;
using AgroEco.UI.Mensajeros;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace AgroEco.UI.Alerts;

public class AlertHandler
{
    private readonly Action<string, object> _enviar;
    private readonly ILogger<AlertHandler> _logger;
    private readonly AlertEngine _alertEngine;
    private readonly GetAllAlerts _getAllAlerts;

    public AlertHandler(
        Action<string, object> enviar,
        ILogger<AlertHandler> logger,
        AlertEngine alertEngine,
        GetAllAlerts getAllAlerts)
    {
        _enviar = enviar;
        _logger = logger;
        _alertEngine = alertEngine;
        _getAllAlerts = getAllAlerts;
    }

    public void ManejarMensaje(Mensaje msg)
    {
        _ = HandleMessageAsync(msg);
    }

    private async Task HandleMessageAsync(Mensaje msg)
    {
        try
        {
            switch (msg.Type)
            {
                case "raise_alert":
                    await HandleRaiseAlertAsync(msg);
                    break;
                case "get_alerts":
                    await HandleGetAlertsAsync(msg);
                    break;
                case "get_alert_channels":
                    await HandleGetChannelsAsync(msg);
                    break;
                case "toggle_channel":
                    await HandleToggleChannelAsync(msg);
                    break;
            }
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "Invalid JSON payload for {Type}", msg.Type);
            _enviar("alert_error", new { error = "Invalid JSON payload", details = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error handling alert message: {Type}", msg.Type);
            _enviar("alert_error", new { error = ex.Message });
        }
    }

    private async Task HandleRaiseAlertAsync(Mensaje msg)
    {
        var payload = msg.LeerPayload<RaiseAlertPayload>();
        if (payload == null)
        {
            _enviar("alert_result", new { success = false, error = "Invalid payload" });
            return;
        }

        // Validación
        if (string.IsNullOrWhiteSpace(payload.Title))
        {
            _enviar("alert_result", new { success = false, error = "Title is required" });
            return;
        }
        if (string.IsNullOrWhiteSpace(payload.Message))
        {
            _enviar("alert_result", new { success = false, error = "Message is required" });
            return;
        }

        var config = BuildConfiguration(payload);
        var alert = Alert.Create(payload.Title, payload.Message, payload.Level, config);
        var result = await _alertEngine.RaiseAsync(alert);

        _enviar("alert_result", new
        {
            success = result.Success,
            message = result.Message,
            alertId = alert.Id,
            status = alert.Status.ToString()
        });
    }

    private async Task HandleGetAlertsAsync(Mensaje msg)
    {
        var payload = msg.LeerPayload<GetAlertsPayload>();
        
        // Validación de paginación
        var limit = payload?.Limit ?? 50;
        var offset = payload?.Offset ?? 0;
        
        if (limit > 200) limit = 200;
        if (limit < 1) limit = 50;
        if (offset < 0) offset = 0;

        var result = await _getAllAlerts.HandleAsync(
            limit,
            offset,
            payload?.Level,
            payload?.Status,
            payload?.FromDate,
            payload?.ToDate);

        if (!result.Success || result.Value is null)
        {
            _enviar("alerts_list", new { success = false, error = result.Message });
            return;
        }

        _enviar("alerts_list", new { success = true, alerts = result.Value });
    }

    private async Task HandleGetChannelsAsync(Mensaje msg)
    {
        var snapshots = _alertEngine.GetChannelSnapshots();
        _enviar("alert_channels", snapshots.Select(s => new
        {
            channelType = s.ChannelType,
            isEnabled = s.IsEnabled,
            priority = s.Priority
        }).ToArray());
    }

    private async Task HandleToggleChannelAsync(Mensaje msg)
    {
        var payload = msg.LeerPayload<ToggleChannelPayload>();
        if (payload == null)
        {
            _enviar("alert_channel_toggled", new { success = false, error = "Invalid payload" });
            return;
        }

        if (string.IsNullOrWhiteSpace(payload.ChannelType))
        {
            _enviar("alert_channel_toggled", new { success = false, error = "ChannelType is required" });
            return;
        }

        var channels = _alertEngine.GetRegisteredChannels();
        var channel = channels.FirstOrDefault(c => c.ChannelType == payload.ChannelType);
        if (channel != null)
        {
            channel.IsEnabled = payload.Enabled;
            _enviar("alert_channel_toggled", new { success = true, channelType = payload.ChannelType, enabled = payload.Enabled });
        }
        else
        {
            _enviar("alert_channel_toggled", new { success = false, error = $"Channel {payload.ChannelType} not found" });
        }
    }

    private AlertConfiguration BuildConfiguration(RaiseAlertPayload payload)
    {
        var config = new AlertConfiguration();

        if (payload.EnableChannels != null)
        {
            foreach (var channel in payload.EnableChannels)
            {
                config.EnableChannel(channel);
            }
        }

        if (payload.DisableChannels != null)
        {
            foreach (var channel in payload.DisableChannels)
            {
                config.DisableChannel(channel);
            }
        }

        if (payload.Options != null)
        {
            foreach (var opt in payload.Options)
            {
                config.WithOption(opt.Key, opt.Value);
            }
        }

        return config;
    }

    private sealed record RaiseAlertPayload(
        string Title,
        string Message,
        AlertLevel Level = AlertLevel.Info,
        int? AlertId = null,
        List<string>? EnableChannels = null,
        List<string>? DisableChannels = null,
        Dictionary<string, object>? Options = null);

    private sealed record GetAlertsPayload(
        int? Limit = null,
        int? Offset = null,
        AlertLevel? Level = null,
        AlertStatus? Status = null,
        DateTime? FromDate = null,
        DateTime? ToDate = null);

    private sealed record ToggleChannelPayload(
        string ChannelType,
        bool Enabled);
}