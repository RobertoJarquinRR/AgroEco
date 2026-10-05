using System.Text.Json;
using AgroEco.Core;
using AgroEco.Core.Triggers;
using AgroEco.Core.Triggers.Creators;
using AgroEco.Core.Triggers.Implementations;

namespace AgroEco.Core.UnitTests.Triggers;

public sealed class TriggerFactoryTests
{
    [Fact]
    public void GetAvailable_ReturnsDateTimeDescriptorWithFields()
    {
        // Arrange
        TriggerFactory factory = new([new DateTimeTriggerCreator()]);

        // Act
        IReadOnlyList<TriggerDescriptor> descriptors = factory.GetAvailable();

        // Assert
        TriggerDescriptor descriptor = Assert.Single(descriptors);
        Assert.Equal("datetime", descriptor.TypeId);
        Assert.Contains(descriptor.Fields, field => field.Name == "targetTime");
    }

    [Fact]
    public void Create_WithValidConfiguration_ReturnsDateTimeTrigger()
    {
        // Arrange
        TriggerFactory factory = new([new DateTimeTriggerCreator()]);
        using JsonDocument config = JsonDocument.Parse(
            """{"targetTime":"2026-10-05T07:00:00Z"}""");

        // Act
        Result<Trigger> result = factory.Create(
            "datetime",
            "Morning watering",
            config.RootElement);

        // Assert
        DateTimeTrigger trigger = Assert.IsType<DateTimeTrigger>(result.Value);
        Assert.Equal("Morning watering", trigger.Name);
    }

    [Fact]
    public void Create_WithUnknownType_ReturnsFailure()
    {
        // Arrange
        TriggerFactory factory = new([new DateTimeTriggerCreator()]);
        using JsonDocument config = JsonDocument.Parse("{}");

        // Act
        Result<Trigger> result = factory.Create(
            "unknown",
            "trigger",
            config.RootElement);

        // Assert
        Assert.False(result.Success);
        Assert.Contains("Unknown trigger type", result.Message);
    }
}
