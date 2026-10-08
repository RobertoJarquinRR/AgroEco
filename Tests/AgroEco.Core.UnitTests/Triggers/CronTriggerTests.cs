using AgroEco.Core.Triggers;
using AgroEco.Core.Triggers.Configuration;
using AgroEco.Core.Triggers.Implementations;
using Moq;

namespace AgroEco.Core.UnitTests.Triggers;

public sealed class CronTriggerTests
{
    [Fact]
    public void UpdateDetails_WithBlankName_PreservesName()
    {
        // Arrange
        var config = new CronTriggerConfiguration
        {
            CronExpression = "0 0 * * *",
            TimeZone = "UTC"
        };
        CronTrigger trigger = new("daily-job", config);

        // Act
        Result result = trigger.UpdateDetails(" ");

        // Assert
        Assert.False(result.Success);
        Assert.Equal("daily-job", trigger.Name);
    }

    [Fact]
    public void UpdateConfiguration_WithValidCronExpression_UpdatesConfig()
    {
        // Arrange
        var config = new CronTriggerConfiguration
        {
            CronExpression = "0 0 * * *",
            TimeZone = "UTC"
        };
        CronTrigger trigger = new("daily-job", config);

        var newConfig = new CronTriggerConfiguration
        {
            CronExpression = "0 12 * * *",
            TimeZone = "America/Argentina/Buenos_Aires",
            StartDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(1)),
            EndDate = DateOnly.FromDateTime(DateTime.UtcNow.AddYears(1))
        };

        // Act
        Result result = trigger.UpdateConfiguration(newConfig);

        // Assert
        Assert.True(result.Success);
        Assert.Equal("0 12 * * *", trigger.Config.CronExpression);
        Assert.Equal("America/Argentina/Buenos_Aires", trigger.Config.TimeZone);
        Assert.Equal(newConfig.StartDate, trigger.Config.StartDate);
        Assert.Equal(newConfig.EndDate, trigger.Config.EndDate);
    }

    [Fact]
    public void UpdateConfiguration_WithNullConfiguration_ThrowsArgumentNullException()
    {
        // Arrange
        var config = new CronTriggerConfiguration
        {
            CronExpression = "0 0 * * *",
            TimeZone = "UTC"
        };
        CronTrigger trigger = new("daily-job", config);

        // Act
        Action action = () => trigger.UpdateConfiguration(null!);

        // Assert
        Assert.Throws<ArgumentNullException>(action);
    }

    [Fact]
    public void UpdateConfiguration_WithInvalidCronExpression_ReturnsFailure()
    {
        // Arrange
        var config = new CronTriggerConfiguration
        {
            CronExpression = "0 0 * * *",
            TimeZone = "UTC"
        };
        CronTrigger trigger = new("daily-job", config);

        var invalidConfig = new CronTriggerConfiguration
        {
            CronExpression = "invalid-cron",
            TimeZone = "UTC"
        };

        // Act
        Result result = trigger.UpdateConfiguration(invalidConfig);

        // Assert
        Assert.False(result.Success);
        Assert.Contains("inv", result.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void GetConfiguration_ReturnsCurrentConfig()
    {
        // Arrange
        var config = new CronTriggerConfiguration
        {
            CronExpression = "0 0 * * *",
            TimeZone = "UTC",
            StartDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(1)),
            EndDate = DateOnly.FromDateTime(DateTime.UtcNow.AddYears(1))
        };
        CronTrigger trigger = new("daily-job", config);

        // Act
        TriggerConfiguration result = trigger.GetConfiguration();

        // Assert
        Assert.IsType<CronTriggerConfiguration>(result);
        var cronConfig = (CronTriggerConfiguration)result;
        Assert.Equal("0 0 * * *", cronConfig.CronExpression);
        Assert.Equal("UTC", cronConfig.TimeZone);
        Assert.Equal(config.StartDate, cronConfig.StartDate);
        Assert.Equal(config.EndDate, cronConfig.EndDate);
    }

    [Fact]
    public void UpdateConfiguration_WithBaseType_Works()
    {
        // Arrange
        var config = new CronTriggerConfiguration
        {
            CronExpression = "0 0 * * *",
            TimeZone = "UTC"
        };
        CronTrigger trigger = new("daily-job", config);

        var newConfig = new CronTriggerConfiguration
        {
            CronExpression = "0 12 * * *",
            TimeZone = "UTC"
        };

        // Act - using base TriggerConfiguration type
        Result result = trigger.UpdateConfiguration((TriggerConfiguration)newConfig);

        // Assert
        Assert.True(result.Success);
        Assert.Equal("0 12 * * *", trigger.Config.CronExpression);
    }

    [Fact]
    public void UpdateConfiguration_WithWrongType_ReturnsFailure()
    {
        // Arrange
        var config = new CronTriggerConfiguration
        {
            CronExpression = "0 0 * * *",
            TimeZone = "UTC"
        };
        CronTrigger trigger = new("daily-job", config);

        var wrongConfig = new DateTimeTriggerConfiguration(DateTimeOffset.UtcNow.AddDays(1));

        // Act
        Result result = trigger.UpdateConfiguration(wrongConfig);

        // Assert
        Assert.False(result.Success);
        Assert.Contains("Invalid configuration type", result.Message);
    }

    }