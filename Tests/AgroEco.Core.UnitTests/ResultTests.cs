namespace AgroEco.Core.UnitTests;

public sealed class ResultTests
{
    [Fact]
    public void CreateSuccess_WithMessage_ReturnsSuccessfulResult()
    {
        // Arrange
        const string message = "completed";

        // Act
        Result result = Result.CreateSuccess(message);

        // Assert
        Assert.True(result.Success);
        Assert.Equal(message, result.Message);
        Assert.Null(result.Exception);
    }

    [Fact]
    public void CreateFailure_WithException_ReturnsFailedResultWithException()
    {
        // Arrange
        InvalidOperationException exception = new("invalid state");

        // Act
        Result result = Result.CreateFailure("failed", exception);

        // Assert
        Assert.False(result.Success);
        Assert.Equal("failed", result.Message);
        Assert.Same(exception, result.Exception);
    }

    [Fact]
    public void CreateSuccess_GenericResult_ExposesValue()
    {
        // Arrange
        const int expected = 5;

        // Act
        Result<int> result = Result<int>.CreateSuccess(expected);

        // Assert
        Assert.True(result.Success);
        Assert.Equal(expected, result.Value);
    }
}
