using AgroEco.Core.Alerts;
using AgroEco.Core.Jobs.Actions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace AgroEco.Core.Jobs.Actions.Implementations;

public sealed class SendAlertAction : Action
{
    private AlertEngine _alertEngine = null!;
    private SendAlertActionConfiguration _config = null!;

    public SendAlertAction(string name, AlertEngine alertEngine) : base(name)
    {
        _alertEngine = alertEngine;
    }

    private SendAlertAction() : base(string.Empty)
    {
    }

    public SendAlertActionConfiguration Config
    {
        get => _config;
        init => _config = value ?? throw new ArgumentNullException(nameof(value));
    }

    internal override void AttachServices(IServiceProvider services)
    {
        _alertEngine = services.GetRequiredService<AlertEngine>();
    }

    public override async Task<Result> Execute()
    {
        var config = _config;
        
        var alertConfig = new AlertConfiguration();
        
        if (config.EnableChannels != null)
        {
            foreach (var channel in config.EnableChannels)
            {
                alertConfig.EnableChannel(channel);
            }
        }
        
        if (config.DisableChannels != null)
        {
            foreach (var channel in config.DisableChannels)
            {
                alertConfig.DisableChannel(channel);
            }
        }
        
        if (config.Options != null)
        {
            foreach (var opt in config.Options)
            {
                alertConfig.WithOption(opt.Key, opt.Value);
            }
        }

        ChangeStatus(Status.Running);

        var result = await _alertEngine.RaiseAsync(
            config.Title,
            config.Message,
            config.Level,
            alertConfig);

        if (result.Success)
        {
            ChangeStatus(Status.Succeeded);
        }
        else
        {
            ChangeStatus(Status.Faulted);
        }

        return result;
    }
}