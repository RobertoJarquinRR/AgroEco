using System.Text.Json;
using AgroEco.Core;
using AgroEco.Core.Triggers.Implementations;

namespace AgroEco.Core.UnitTests.Triggers;

public sealed class DateTimeTriggerTests
{
    [Fact]
    public void UpdateDetails_WithBlankName_PreservesName()
    {
        // Arrange
        DateTimeTrigger trigger = new(
            "watering",
            DateTimeOffset.UtcNow.AddHours(1));

        // Act
        Result result = trigger.UpdateDetails(" ");

        // Assert
        Assert.False(result.Success);
        Assert.Equal("watering", trigger.Name);
    }

    [Fact]
    public void UpdateConfiguration_WithValidTargetTime_UpdatesTargetTime()
    {
        // Arrange
        DateTimeTrigger trigger = new(
            "watering",
            DateTimeOffset.UtcNow.AddHours(1));
        DateTimeOffset targetTime = DateTimeOffset.UtcNow.AddDays(1);
        using JsonDocument config = JsonDocument.Parse(
            $$"""{"targetTime":"{{targetTime:O}}"}""");

        // Act
        Result result = trigger.UpdateConfiguration(config.RootElement);

        // Assert
        Assert.True(result.Success);
        Assert.Equal(targetTime, trigger.TargetTime);
    }

    [Fact]
    public void UpdateConfiguration_WithInvalidTargetTime_ReturnsFailure()
    {
        // Arrange
        DateTimeTrigger trigger = new(
            "watering",
            DateTimeOffset.UtcNow.AddHours(1));
        using JsonDocument config = JsonDocument.Parse("{}");

        // Act
        Result result = trigger.UpdateConfiguration(config.RootElement);

        // Assert
        Assert.False(result.Success);
        Assert.Contains("targetTime", result.Message);
    }

}
