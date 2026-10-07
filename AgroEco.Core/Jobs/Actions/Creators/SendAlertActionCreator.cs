using AgroEco.Core.Alerts;
using AgroEco.Core.Configuration;
using AgroEco.Core.Jobs.Actions.Configuration;
using AgroEco.Core.Jobs.Actions.Implementations;

namespace AgroEco.Core.Jobs.Actions.Creators;

public sealed class SendAlertActionCreator : IActionCreator
{
    private static readonly IReadOnlyList<ConfigFieldChoice> ChannelChoices =
    [
        new("windows-toast", "Notificación de Windows")
    ];

    private static readonly IReadOnlyList<ConfigFieldChoice> LevelChoices =
    [
        new(nameof(AlertLevel.Info), "Info"),
        new(nameof(AlertLevel.Warning), "Advertencia"),
        new(nameof(AlertLevel.Error), "Error"),
        new(nameof(AlertLevel.Critical), "Crítico")
    ];

    private readonly AlertEngine _alertEngine;

    public SendAlertActionCreator(AlertEngine alertEngine)
    {
        _alertEngine = alertEngine;
    }

    public ActionDescriptor Descriptor { get; } =
        new("sendAlert", "Enviar alerta",
        [
            new ConfigFieldDescriptor("title", "Título de la alerta", "text"),
            new ConfigFieldDescriptor("message", "Mensaje de la alerta", "textarea"),
            new ConfigFieldDescriptor("level", "Nivel", "select", true, LevelChoices),
            new ConfigFieldDescriptor("enableChannels", "Canales a habilitar", "select", true, ChannelChoices, true),
            new ConfigFieldDescriptor("disableChannels", "Canales a deshabilitar (opcional)", "select", false, ChannelChoices, true),
            new ConfigFieldDescriptor("options", "Opciones adicionales JSON (opcional, ej: {\"actionUrl\": \"...\"})", "textarea", false)
        ]);

    public Result<Action> Create(string name, ActionConfiguration configuration)
    {
        if (configuration is not SendAlertActionConfiguration alertConfig)
        {
            return Result<Action>.CreateFailure("Invalid configuration for SendAlertAction.");
        }

        var action = new SendAlertAction(name.Trim(), _alertEngine)
        {
            Config = alertConfig
        };

        return Result<Action>.CreateSuccess(action);
    }
}