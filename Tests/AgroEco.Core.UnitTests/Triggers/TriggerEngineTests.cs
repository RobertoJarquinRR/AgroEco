using AgroEco.Core.Interfaces;
using AgroEco.Core.Jobs;
using AgroEco.Core.Jobs.Actions.Implementations;
using AgroEco.Core.Jobs.Persistence;
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

    [Fact]
    public async Task StopAsync_BeforeTargetTime_DoesNotExecuteSubscribers()
    {
        // Arrange
        Mock<IRepository<Trigger>> triggerRepository = new();
        Mock<IRepository<AgroEco.Core.Jobs.Job>> jobRepository = new();
        Mock<IUnitOfWork> unitOfWork = new();
        DateTimeTrigger trigger = new(
            "Test trigger",
            DateTimeOffset.UtcNow.AddSeconds(1));
        Mock<ITriggerable> triggerable = new();
        triggerRepository
            .Setup(value => value.GetByIdAsync(7, It.IsAny<CancellationToken>()))
            .ReturnsAsync(trigger);
        using ServiceProvider provider = BuildProvider(
            triggerRepository.Object,
            jobRepository.Object,
            unitOfWork.Object);
        TriggerEngine engine = new(provider.GetRequiredService<IServiceScopeFactory>());
        await engine.SubscribeAsync(7, triggerable.Object);

        // Act
        Result startResult = await engine.StartAsync(7);
        Result stopResult = await engine.StopAsync(7);
        await Task.Delay(100);

        // Assert
        Assert.True(startResult.Success);
        Assert.True(stopResult.Success);
        triggerable.Verify(value => value.OnTrigger(), Times.Never);
    }

    [Fact]
    public async Task StopAsync_WhileExecutionIsFinishing_KeepsTriggerRegisteredUntilCompletion()
    {
        // Arrange
        Mock<IRepository<Trigger>> repository = new();
        BlockingTrigger trigger = new("Test trigger");
        repository
            .Setup(value => value.GetByIdAsync(7, It.IsAny<CancellationToken>()))
            .ReturnsAsync(trigger);
        using ServiceProvider provider = BuildProvider(repository.Object);
        TriggerEngine engine = new(provider.GetRequiredService<IServiceScopeFactory>());
        await engine.SubscribeAsync(7, Mock.Of<ITriggerable>());
        await engine.StartAsync(7);

        // Act
        Result stopResult = await engine.StopAsync(7);
        Result<Trigger> registeredResult = await engine.GetOrCreateAsync(7);
        trigger.Complete();
        await trigger.ExecutionCompleted;

        // Assert
        Assert.True(stopResult.Success);
        Assert.True(registeredResult.Success);
        Assert.Same(trigger, registeredResult.Value);
        repository.Verify(
            value => value.GetByIdAsync(7, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task StartAsync_WithDateTimeJob_CompletesJobAfterTargetTime()
    {
        // Arrange
        Mock<IRepository<Trigger>> triggerRepository = new();
        Mock<IRepository<Job>> jobRepository = new();
        Mock<IUnitOfWork> unitOfWork = new();
        DateTimeTrigger trigger = new(
            "Test trigger",
            DateTimeOffset.UtcNow.AddSeconds(1));
        Result<Job> jobResult = await Job.CreateJob(
            "Test job",
            "Timer integration test",
            Status.Created,
            1,
            [new NoOpAction("Test action")],
            trigger);
        Assert.True(jobResult.Success);
        Job job = jobResult.Value!;
        triggerRepository
            .Setup(value => value.GetByIdAsync(7, It.IsAny<CancellationToken>()))
            .ReturnsAsync(trigger);
        jobRepository
            .Setup(value => value.GetByIdAsync(
                It.IsAny<int>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(job);
        TaskCompletionSource<Job> completion =
            new(TaskCreationOptions.RunContinuationsAsynchronously);
        using ServiceProvider provider = BuildProvider(
            triggerRepository.Object,
            jobRepository.Object,
            unitOfWork.Object);
        TriggerEngine engine = new(provider.GetRequiredService<IServiceScopeFactory>());
        engine.JobExecutionCompleted += completed => completion.TrySetResult(completed);
        await engine.SubscribeAsync(7, job);

        // Act
        Result startResult = await engine.StartAsync(7);
        Job completedJob = await completion.Task.WaitAsync(TimeSpan.FromSeconds(5));

        // Assert
        Assert.True(startResult.Success);
        Assert.Same(job, completedJob);
        Assert.Equal(Status.Succeeded, job.Status);
        Assert.Equal(Status.Succeeded, job.Actions.Single().Status);
        jobRepository.Verify(
            value => value.UpdateAsync(job, It.IsAny<CancellationToken>()),
            Times.Once);
        unitOfWork.Verify(
            value => value.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task StartAsync_WhenTriggerFails_PreservesFailureResultOnJob()
    {
        // Arrange
        Mock<IRepository<Trigger>> triggerRepository = new();
        Mock<IRepository<Job>> jobRepository = new();
        Mock<IUnitOfWork> unitOfWork = new();
        FailingTrigger trigger = new("Test trigger");
        Result<Job> jobResult = await Job.CreateJob(
            "Test job",
            "Timer failure test",
            Status.Created,
            1,
            [new NoOpAction("Test action")],
            trigger);
        Assert.True(jobResult.Success);
        Job job = jobResult.Value!;
        triggerRepository
            .Setup(value => value.GetByIdAsync(7, It.IsAny<CancellationToken>()))
            .ReturnsAsync(trigger);
        jobRepository
            .Setup(value => value.GetByIdAsync(
                It.IsAny<int>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(job);
        using ServiceProvider provider = BuildProvider(
            triggerRepository.Object,
            jobRepository.Object,
            unitOfWork.Object);
        TriggerEngine engine = new(provider.GetRequiredService<IServiceScopeFactory>());
        TaskCompletionSource<Job> completion =
            new(TaskCreationOptions.RunContinuationsAsynchronously);
        engine.JobExecutionCompleted += completed => completion.TrySetResult(completed);
        await engine.SubscribeAsync(7, job);

        // Act
        Result startResult = await engine.StartAsync(7);
        Job completedJob = await completion.Task.WaitAsync(TimeSpan.FromSeconds(5));

        // Assert
        Assert.True(startResult.Success);
        Assert.Equal(Status.Faulted, completedJob.Status);
        Assert.Contains(
            completedJob.Results,
            result => !result.Success
                && result.Message!.Contains("Test trigger failure"));
    }

    private sealed class FailingTrigger : Trigger
    {
        public FailingTrigger(string name) : base(name)
        {
        }

        public override Task<Result> InitTrigger()
            => Task.FromResult(Result.CreateFailure("Test trigger failure"));
    }

    private static ServiceProvider BuildProvider(
        IRepository<Trigger> repository,
        IRepository<AgroEco.Core.Jobs.Job>? jobRepository = null,
        IUnitOfWork? unitOfWork = null)
    {
        ServiceCollection services = new();
        services.AddScoped<IRepository<Trigger>>(_ => repository);
        services.AddScoped<IRepository<AgroEco.Core.Jobs.Job>>(
            _ => jobRepository ?? Mock.Of<IRepository<AgroEco.Core.Jobs.Job>>());
        services.AddScoped<IUnitOfWork>(
            _ => unitOfWork ?? Mock.Of<IUnitOfWork>());
        services.AddScoped<GetByIdTrigger>();
        services.AddScoped<UpdateJob>();
        return services.BuildServiceProvider();
    }

    private sealed class BlockingTrigger : Trigger
    {
        private readonly TaskCompletionSource<Result> _completion =
            new(TaskCreationOptions.RunContinuationsAsynchronously);

        public BlockingTrigger(string name) : base(name)
        {
        }

        public Task ExecutionCompleted => _completion.Task;

        public void Complete()
            => _completion.TrySetResult(Result.CreateSuccess());

        public override Task<Result> InitTrigger()
            => _completion.Task;
    }
}
