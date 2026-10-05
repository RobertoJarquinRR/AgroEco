using AgroEco.Hardware;

namespace AgroEco.Hardware.UnitTests;

public sealed class SensorsTests
{
    [Fact]
    public async Task Activate_WhenCalled_ReturnsSuccess()
    {
        // Arrange
        Sensors sensors = new();

        // Act
        var result = await sensors.Activate();

        // Assert
        Assert.True(result.Success);
        Assert.Equal("Sensor activated successfully", result.Message);
    }
}
