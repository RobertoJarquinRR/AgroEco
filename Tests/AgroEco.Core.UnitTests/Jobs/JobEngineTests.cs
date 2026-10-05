using AgroEco.Core.Jobs;
using AgroEco.Core.Jobs.Persistence;
using AgroEco.Core.Triggers;
using AgroEco.Core.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Moq;

namespace AgroEco.Core.UnitTests.Jobs;

public sealed class JobEngineTests
{
    [Fact]
    public async Task Init_WhenThereAreNoRunningJobs_ReturnsSuccess()
    {
        // Arrange
        Mock<IJobRepository> repository = new();
        repository
            .Setup(value => value.GetRunningJobsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);
        using ServiceProvider provider = BuildProvider(repository.Object);
        TriggerEngine triggerEngine = new(provider.GetRequiredService<IServiceScopeFactory>());
        JobEngine engine = new(provider.GetRequiredService<IServiceScopeFactory>(), triggerEngine);

        // Act
        Result result = await engine.Init();

        // Assert
        Assert.True(result.Success);
        Assert.Contains("No running jobs", result.Message);
        repository.Verify(
            value => value.GetRunningJobsAsync(It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task RunJob_WhenJobDoesNotExist_ReturnsFailure()
    {
        // Arrange
        Mock<IJobRepository> repository = new();
        repository
            .Setup(value => value.GetByIdWithDetailsAsync(42, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Job?)null);
        using ServiceProvider provider = BuildProvider(repository.Object);
        TriggerEngine triggerEngine = new(provider.GetRequiredService<IServiceScopeFactory>());
        JobEngine engine = new(provider.GetRequiredService<IServiceScopeFactory>(), triggerEngine);

        // Act
        Result result = await engine.RunJob(42);

        // Assert
        Assert.False(result.Success);
        Assert.Contains("No job found with id: 42", result.Message);
        repository.Verify(
            value => value.GetByIdWithDetailsAsync(42, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    private static ServiceProvider BuildProvider(IJobRepository repository)
    {
        ServiceCollection services = new();
        services.AddScoped<IJobRepository>(_ => repository);
        services.AddScoped<GetRunningJobs>();
        services.AddScoped<GetByIdJobWithDetails>();
        services.AddScoped<UpdateJob>();
        services.AddScoped<IRepository<Job>>(_ => new Mock<IRepository<Job>>().Object);
        services.AddScoped<IUnitOfWork>(_ => new Mock<IUnitOfWork>().Object);
        return services.BuildServiceProvider();
    }
}
