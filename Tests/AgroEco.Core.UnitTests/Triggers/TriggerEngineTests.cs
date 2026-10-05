using AgroEco.Core.Interfaces;
using AgroEco.Core.Triggers;
using AgroEco.Core.Triggers.Implementations;
using AgroEco.Core.Triggers.Persistence;
using Microsoft.Extensions.DependencyInjection;
using Moq;

namespace AgroEco.Core.UnitTests.Triggers;

public sealed class TriggerEngineTests
{
    [Fact]
    public async Task GetOrCreateAsync_WhenTriggerDoesNotExist_ReturnsFailure()
    {
        // Arrange
        Mock<IRepository<Trigger>> repository = new();
        repository
            .Setup(value => value.GetByIdAsync(7, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Trigger?)null);
        using ServiceProvider provider = BuildProvider(repository.Object);
        TriggerEngine engine = new(provider.GetRequiredService<IServiceScopeFactory>());

        // Act
        Result<Trigger> result = await engine.GetOrCreateAsync(7);

        // Assert
        Assert.False(result.Success);
        Assert.Equal("Trigger not found", result.Message);
        Assert.Empty(engine.GetActiveTriggers());
        repository.Verify(
            value => value.GetByIdAsync(7, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task StartAsync_WhenTriggerHasNoSubscribers_ReturnsFailure()
    {
        // Arrange
        Mock<IRepository<Trigger>> repository = new();
        Trigger trigger = new DateTimeTrigger(
            "Test trigger",
            DateTimeOffset.UtcNow.AddMinutes(5));
        repository
            .Setup(value => value.GetByIdAsync(7, It.IsAny<CancellationToken>()))
            .ReturnsAsync(trigger);
        using ServiceProvider provider = BuildProvider(repository.Object);
        TriggerEngine engine = new(provider.GetRequiredService<IServiceScopeFactory>());

        // Act
        Result result = await engine.StartAsync(7);

        // Assert
        Assert.False(result.Success);
        Assert.Contains("has no subscribers", result.Message);
        Assert.Empty(engine.GetActiveTriggers());
    }

    private static ServiceProvider BuildProvider(IRepository<Trigger> repository)
    {
        ServiceCollection services = new();
        services.AddScoped<IRepository<Trigger>>(_ => repository);
        services.AddScoped<GetByIdTrigger>();
        return services.BuildServiceProvider();
    }
}
