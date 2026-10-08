using AgroEco.Core.Jobs;
using AgroEco.Core.Jobs.Persistence;
using AgroEco.Core.Jobs.Runs;
using AgroEco.Core.Jobs.Runs.Persistence;
using AgroEco.Core.Jobs.Actions.Implementations;
using AgroEco.Core.Triggers;
using AgroEco.Core.Triggers.Implementations;
using AgroEco.Core.Triggers.Persistence;
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

    [Fact]
    public async Task RunJob_WhenTriggerCompletes_PersistsJobAndJobRun()
    {
        // Arrange
        Mock<IJobRepository> jobRepository = new();
        Mock<IRepository<Job>> persistenceRepository = new();
        Mock<IRepository<Trigger>> triggerRepository = new();
        Mock<IRepository<JobRun>> jobRunRepository = new();
        Mock<IUnitOfWork> unitOfWork = new();
        DateTimeTrigger trigger = new(
            "timer",
            DateTimeOffset.UtcNow);
        Result<Job> creation = await Job.CreateJob(
            "job",
            null,
            Status.Created,
            null,
            [new NoOpAction("action")],
            trigger);
        Job job = Assert.IsType<Job>(creation.Value);
        jobRepository
            .Setup(value => value.GetByIdWithDetailsAsync(
                7,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(job);
        persistenceRepository
            .Setup(value => value.GetByIdAsync(
                It.IsAny<int>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(job);
        triggerRepository
            .Setup(value => value.GetByIdAsync(
                0,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(trigger);
        using ServiceProvider provider = BuildProvider(
            jobRepository.Object,
            persistenceRepository.Object,
            triggerRepository.Object,
            jobRunRepository.Object,
            unitOfWork.Object);
        TriggerEngine triggerEngine = provider.GetRequiredService<TriggerEngine>();
        JobEngine engine = provider.GetRequiredService<JobEngine>();
        TaskCompletionSource<Job> completion =
            new(TaskCreationOptions.RunContinuationsAsynchronously);
        engine.JobExecutionCompleted += completed => completion.TrySetResult(completed);

        // Act
        Result result = await engine.RunJob(7);
        Assert.True(result.Success, result.Message);
        Job completedJob = await completion.Task.WaitAsync(TimeSpan.FromSeconds(5));

        // Assert
        Assert.True(result.Success);
        Assert.Same(job, completedJob);
        Assert.Equal(Status.Succeeded, completedJob.Status);
        persistenceRepository.Verify(
            value => value.UpdateAsync(job, It.IsAny<CancellationToken>()),
            Times.AtLeastOnce);
        jobRunRepository.Verify(
            value => value.AddAsync(It.IsAny<JobRun>()),
            Times.Once);
        unitOfWork.Verify(
            value => value.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.AtLeastOnce);
    }

    private static ServiceProvider BuildProvider(
        IJobRepository repository,
        IRepository<Job>? persistenceRepository = null,
        IRepository<Trigger>? triggerRepository = null,
        IRepository<JobRun>? jobRunRepository = null,
        IUnitOfWork? unitOfWork = null)
    {
        ServiceCollection services = new();
        services.AddScoped<IJobRepository>(_ => repository);
        services.AddScoped<GetRunningJobs>();
        services.AddScoped<GetByIdJobWithDetails>();
        services.AddScoped<UpdateJob>();
        services.AddScoped<CreateJobRun>();
        services.AddScoped<IRepository<Job>>(
            _ => persistenceRepository ?? new Mock<IRepository<Job>>().Object);
        services.AddScoped<IRepository<Trigger>>(
            _ => triggerRepository ?? new Mock<IRepository<Trigger>>().Object);
        services.AddScoped<IRepository<JobRun>>(
            _ => jobRunRepository ?? new Mock<IRepository<JobRun>>().Object);
        services.AddScoped<IUnitOfWork>(
            _ => unitOfWork ?? new Mock<IUnitOfWork>().Object);
        services.AddScoped<GetByIdTrigger>();
        services.AddSingleton<TriggerEngine>();
        services.AddSingleton<JobEngine>();
        return services.BuildServiceProvider();
    }
}
