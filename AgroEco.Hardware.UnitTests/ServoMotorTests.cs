using AgroEco.Hardware;

namespace AgroEco.Hardware.UnitTests;

public sealed class ServoMotorTests
{
    [Fact]
    public async Task Activate_WithConfiguredDegrees_ReturnsMovementMessage()
    {
        // Arrange
        ServoMotor servoMotor = new(45);

        // Act
        var result = await servoMotor.Activate();

        // Assert
        Assert.True(result.Success);
        Assert.Equal("Servomotor moved to 45 degrees", result.Message);
    }
}
