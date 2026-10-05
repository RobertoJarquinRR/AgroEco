using AgroEco.Core;
using AgroEco.Core.Jobs;
using AgroEco.Core.Jobs.Actions;
using AgroEco.Core.Jobs.Actions.Implementations;

namespace AgroEco.Core.UnitTests.Jobs.Actions;

public sealed class ActionTests
{
    [Fact]
    public void ChangeStatus_FromSucceededToRunning_ReturnsFailure()
    {
        // Arrange
        ActionTest action = new("action");
        action.ChangeStatus(Status.Running);
        action.ChangeStatus(Status.Succeeded);

        // Act
        Result result = action.ChangeStatus(Status.Running);

        // Assert
        Assert.False(result.Success);
        Assert.Equal(Status.Succeeded, action.Status);
    }
}
