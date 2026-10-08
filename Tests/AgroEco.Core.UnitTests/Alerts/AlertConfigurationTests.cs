using AgroEco.Core.Alerts;

namespace AgroEco.Core.UnitTests.Alerts;

public sealed class AlertConfigurationTests
{
    [Fact]
    public void Default_HasNoChannelsEnabled()
    {
        // Act
        AlertConfiguration config = AlertConfiguration.Default;

        // Assert
        Assert.Empty(config.EnabledChannels);
        Assert.False(config.IsChannelEnabled("windows-toast"));
    }

    [Fact]
    public void AllChannels_EnablesImplementedChannels()
    {
        // Act
        AlertConfiguration config = AlertConfiguration.AllChannels;

        // Assert
        Assert.True(config.IsChannelEnabled("windows-toast"));
    }

    [Fact]
    public void EnableChannel_AddsChannel()
    {
        // Arrange
        AlertConfiguration config = new();

        // Act
        config.EnableChannel("windows-toast");

        // Assert
        Assert.True(config.IsChannelEnabled("windows-toast"));
    }

    [Fact]
    public void DisableChannel_RemovesChannel()
    {
        // Arrange
        AlertConfiguration config = AlertConfiguration.AllChannels;

        // Act
        config.DisableChannel("windows-toast");

        // Assert
        Assert.False(config.IsChannelEnabled("windows-toast"));
    }

    [Fact]
    public void EnableChannel_ThenDisableChannel_RemovesIt()
    {
        // Arrange
        AlertConfiguration config = new();

        // Act
        config.EnableChannel("windows-toast");
        config.DisableChannel("windows-toast");

        // Assert
        Assert.False(config.IsChannelEnabled("windows-toast"));
    }

    [Fact]
    public void IsChannelEnabled_CaseSensitive()
    {
        // Arrange
        AlertConfiguration config = new();
        config.EnableChannel("windows-toast");

        // Act & Assert
        Assert.True(config.IsChannelEnabled("windows-toast"));
        Assert.False(config.IsChannelEnabled("WINDOWS-TOAST"));
        Assert.False(config.IsChannelEnabled("Windows-Toast"));
    }

    [Fact]
    public void FluentApi_ReturnsSameInstance()
    {
        // Act
        AlertConfiguration config = new AlertConfiguration()
            .EnableChannel("windows-toast")
            .EnableChannel("custom-channel")
            .DisableChannel("windows-toast");

        // Assert
        Assert.True(config.IsChannelEnabled("custom-channel"));
        Assert.False(config.IsChannelEnabled("windows-toast"));
    }

    [Fact]
    public void WithOption_SetsAndGetsValue()
    {
        // Arrange
        AlertConfiguration config = new();

        // Act
        config.WithOption("actionUrl", "/test/123");
        config.WithOption("retryCount", 3);

        // Assert
        Assert.Equal("/test/123", config.GetOption<string>("actionUrl"));
        Assert.Equal(3, config.GetOption<int>("retryCount"));
        Assert.Equal("default", config.GetOption<string>("missing", "default"));
        Assert.Equal(0, config.GetOption<int>("missingInt"));
    }

    [Fact]
    public void ChannelOptions_AreIndependent()
    {
        // Arrange
        AlertConfiguration config1 = new();
        config1.WithOption("key", "value1");
        
        AlertConfiguration config2 = new();
        config2.WithOption("key", "value2");

        // Assert
        Assert.Equal("value1", config1.GetOption<string>("key"));
        Assert.Equal("value2", config2.GetOption<string>("key"));
    }
}