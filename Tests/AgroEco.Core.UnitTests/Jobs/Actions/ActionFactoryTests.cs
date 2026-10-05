using System.Text.Json;
using AgroEco.Core;
using AgroEco.Core.Jobs.Actions;
using AgroEco.Core.Jobs.Actions.Creators;
using AgroEco.Core.Jobs.Actions.Implementations;
using CoreAction = AgroEco.Core.Jobs.Actions.Action;

namespace AgroEco.Core.UnitTests.Jobs.Actions;

public sealed class ActionFactoryTests
{
    [Fact]
    public void GetAvailable_ReturnsTestDescriptor()
    {
        // Arrange
        ActionFactory factory = new([new ActionTestCreator()]);

        // Act
        IReadOnlyList<ActionDescriptor> descriptors = factory.GetAvailable();

        // Assert
        ActionDescriptor descriptor = Assert.Single(descriptors);
        Assert.Equal("test", descriptor.TypeId);
    }

    [Fact]
    public void Create_WithTestType_ReturnsActionTest()
    {
        // Arrange
        ActionFactory factory = new([new ActionTestCreator()]);
        using JsonDocument config = JsonDocument.Parse("{}");

        // Act
        Result<CoreAction> result = factory.Create(
            "test",
            "Test action",
            config.RootElement);

        // Assert
        Assert.IsType<ActionTest>(result.Value);
    }

    [Fact]
    public void Create_WithUnknownType_ReturnsFailure()
    {
        // Arrange
        ActionFactory factory = new([new ActionTestCreator()]);
        using JsonDocument config = JsonDocument.Parse("{}");

        // Act
        Result<CoreAction> result = factory.Create(
            "unknown",
            "Test action",
            config.RootElement);

        // Assert
        Assert.False(result.Success);
        Assert.Contains("Unknown action type", result.Message);
    }
}
