using System.Collections.Generic;

namespace AgroEco.Core.Alerts;

public sealed class AlertConfiguration
{
    public HashSet<string> EnabledChannels { get; set; } = new();

    public Dictionary<string, object> ChannelOptions { get; set; } = new();

    public static AlertConfiguration Default => new();

    public static AlertConfiguration AllChannels => new()
    {
        EnabledChannels = new HashSet<string> { "windows-toast" }
    };

    public AlertConfiguration EnableChannel(string channelType)
    {
        EnabledChannels.Add(channelType);
        return this;
    }

    public AlertConfiguration DisableChannel(string channelType)
    {
        EnabledChannels.Remove(channelType);
        return this;
    }

    public bool IsChannelEnabled(string channelType)
        => EnabledChannels.Contains(channelType);

    public AlertConfiguration WithOption(string key, object value)
    {
        ChannelOptions[key] = value;
        return this;
    }

    public T GetOption<T>(string key, T defaultValue = default!)
    {
        if (ChannelOptions.TryGetValue(key, out var value) && value is T typed)
            return typed;
        return defaultValue;
    }
}