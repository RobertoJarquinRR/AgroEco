using AgroEco.Core;
using AgroEco.Core.Triggers.Configuration;
using AgroEco.Core.Triggers;
using AgroEco.Core.Triggers.Implementations;
using Moq;

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
    public void UpdateConfiguration_WithTypedConfiguration_UpdatesTargetTime()
    {
        // Arrange
        DateTimeTrigger trigger = new(
            "watering",
            DateTimeOffset.UtcNow.AddHours(1));
        DateTimeOffset targetTime = DateTimeOffset.UtcNow.AddDays(1);
        DateTimeTriggerConfiguration config = new(targetTime);

        // Act
        Result result = trigger.UpdateConfiguration(config);

        // Assert
        Assert.True(result.Success);
        Assert.Equal(targetTime, trigger.TargetTime);
    }

    [Fact]
    public void UpdateConfiguration_WithNullConfiguration_ThrowsArgumentNullException()
    {
        // Arrange
        DateTimeTrigger trigger = new(
            "watering",
            DateTimeOffset.UtcNow.AddHours(1));
        // Act
        Action action = () => trigger.UpdateConfiguration(null!);

        // Assert
        Assert.Throws<ArgumentNullException>(action);
    }

    [Fact]
    public void UpdateConfiguration_WithPastTargetTime_ReturnsFailure()
    {
        // Arrange
        DateTimeTrigger trigger = new(
            "watering",
            DateTimeOffset.UtcNow.AddHours(1));
        DateTimeTriggerConfiguration config = new(
            DateTimeOffset.UtcNow.AddMinutes(-1));

        // Act
        Result result = trigger.UpdateConfiguration(config);

        // Assert
        Assert.False(result.Success);
        Assert.Contains("future", result.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task InitTrigger_WhenTargetTimeHasJustPassed_ExecutesSubscribersImmediately()
    {
        // Arrange
        DateTimeTrigger trigger = new(
            "watering",
            DateTimeOffset.UtcNow.AddMilliseconds(-1));
        Mock<ITriggerable> triggerable = new();
        triggerable
            .Setup(value => value.OnTrigger())
            .ReturnsAsync(Result.CreateSuccess());
        trigger.Subscribe(triggerable.Object);

        // Act
        Result result = await trigger.InitTrigger();

        // Assert
        Assert.True(result.Success);
        triggerable.Verify(value => value.OnTrigger(), Times.Once);
    }

}
