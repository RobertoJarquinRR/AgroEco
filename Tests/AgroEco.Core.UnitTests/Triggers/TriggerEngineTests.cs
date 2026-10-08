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
    public async Task StartAsync_WhenTriggerHasNoSubscribers_Succeeds()
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
        Assert.True(result.Success);
        Assert.Contains("started", result.Message, StringComparison.OrdinalIgnoreCase);
        Assert.Single(engine.GetActiveTriggers());
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
        int completedNotifications = 0;
        engine.ExecutionCompleted += (report) =>
        {
            completedNotifications++;
            return Task.CompletedTask;
        };
        await engine.SubscribeAsync(7, triggerable.Object);

        // Act
        Result startResult = await engine.StartAsync(7);
        Result stopResult = await engine.StopAsync(7);
        await Task.Delay(100);

        // Assert
        Assert.True(startResult.Success);
        Assert.True(stopResult.Success);
        triggerable.Verify(value => value.OnTrigger(), Times.Never);
        Assert.Equal(0, completedNotifications);
    }

    [Fact]
    public async Task StopAsync_WhileExecutionIsFinishing_KeepsTriggerRegistered()
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
        await trigger.WaitCompletion;

        // Assert
        Assert.True(stopResult.Success);
        Assert.True(registeredResult.Success);
        Assert.Same(trigger, registeredResult.Value);
        Assert.False(trigger.Enabled);
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
        TaskCompletionSource<TriggerExecutionReport> completion =
            new(TaskCreationOptions.RunContinuationsAsynchronously);
        using ServiceProvider provider = BuildProvider(
            triggerRepository.Object,
            jobRepository.Object,
            unitOfWork.Object);
        TriggerEngine engine = new(provider.GetRequiredService<IServiceScopeFactory>());
        engine.ExecutionCompleted += (report) =>
        {
            completion.TrySetResult(report);
            return Task.CompletedTask;
        };
        await engine.SubscribeAsync(7, job);

        // Act
        Result startResult = await engine.StartAsync(7);
        TriggerExecutionReport completedReport = await completion.Task.WaitAsync(TimeSpan.FromSeconds(5));

        // Assert
        Assert.True(startResult.Success);
        Assert.True(completedReport.OverallResult.Success);
        Assert.Equal(Status.Succeeded, job.Status);
        Assert.Equal(Status.Succeeded, job.Actions.Single().Status);
    }

    [Fact]
    public async Task StartAsync_WhenWaitFails_StopsWithoutFiring()
    {
        // Arrange
        Mock<IRepository<Trigger>> repository = new();
        FailingTrigger trigger = new("Test trigger");
        repository
            .Setup(value => value.GetByIdAsync(7, It.IsAny<CancellationToken>()))
            .ReturnsAsync(trigger);
        using ServiceProvider provider = BuildProvider(repository.Object);
        TriggerEngine engine = new(provider.GetRequiredService<IServiceScopeFactory>());
        Mock<ITriggerable> triggerable = new();
        triggerable
            .Setup(value => value.OnTrigger())
            .ReturnsAsync(Result.CreateSuccess());
        await engine.SubscribeAsync(7, triggerable.Object);

        // Act
        Result startResult = await engine.StartAsync(7);
        await Task.Delay(200);

        // Assert
        Assert.True(startResult.Success);
        Assert.Equal(0, trigger.FiredCount);
        Assert.False(trigger.Enabled);
        triggerable.Verify(value => value.OnTrigger(), Times.Never);
        Assert.Empty(engine.GetActiveTriggers());
    }

    [Fact]
    public async Task StartAsync_WhenSubscriberFails_PublishesFailureReport()
    {
        // Arrange
        Mock<IRepository<Trigger>> repository = new();
        AlwaysReadyTrigger trigger = new("Test trigger");
        trigger.SetMaxExecutions(1);
        repository
            .Setup(value => value.GetByIdAsync(7, It.IsAny<CancellationToken>()))
            .ReturnsAsync(trigger);
        using ServiceProvider provider = BuildProvider(repository.Object);
        TriggerEngine engine = new(provider.GetRequiredService<IServiceScopeFactory>());
        TaskCompletionSource<TriggerExecutionReport> completion =
            new(TaskCreationOptions.RunContinuationsAsynchronously);
        engine.ExecutionCompleted += (report) =>
        {
            completion.TrySetResult(report);
            return Task.CompletedTask;
        };
        Mock<ITriggerable> triggerable = new();
        triggerable
            .Setup(value => value.OnTrigger())
            .ReturnsAsync(Result.CreateFailure("Test trigger failure"));
        await engine.SubscribeAsync(7, triggerable.Object);

        // Act
        Result startResult = await engine.StartAsync(7);
        TriggerExecutionReport completedReport = await completion.Task.WaitAsync(TimeSpan.FromSeconds(5));

        // Assert
        Assert.True(startResult.Success);
        Assert.False(completedReport.OverallResult.Success);
        Assert.Contains("One or more triggerables failed", completedReport.OverallResult.Message);
    }

    [Fact]
    public async Task SubscribeAsync_AfterTriggerStopped_KeepsSameTrigger()
    {
        // Arrange
        Mock<IRepository<Trigger>> repository = new();
        DateTimeTrigger trigger = new(
            "Test trigger",
            DateTimeOffset.UtcNow.AddHours(1));
        repository
            .Setup(value => value.GetByIdAsync(
                7,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(trigger);
        using ServiceProvider provider = BuildProvider(repository.Object);
        TriggerEngine engine = new(provider.GetRequiredService<IServiceScopeFactory>());
        await engine.SubscribeAsync(7, Mock.Of<ITriggerable>());
        await engine.StartAsync(7);
        await engine.StopAsync(7);

        // Act
        Result result = await engine.SubscribeAsync(7, Mock.Of<ITriggerable>());

        // Assert
        Assert.True(result.Success);
        repository.Verify(
            value => value.GetByIdAsync(7, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Trigger_FiresMultipleTimes_WhenEnabled()
    {
        // Arrange
        Mock<IRepository<Trigger>> repository = new();
        AlwaysReadyTrigger trigger = new("Test trigger");
        repository
            .Setup(value => value.GetByIdAsync(7, It.IsAny<CancellationToken>()))
            .ReturnsAsync(trigger);
        using ServiceProvider provider = BuildProvider(repository.Object);
        TriggerEngine engine = new(provider.GetRequiredService<IServiceScopeFactory>());
        Mock<ITriggerable> triggerable = new();
        triggerable
            .Setup(value => value.OnTrigger())
            .ReturnsAsync(Result.CreateSuccess());
        await engine.SubscribeAsync(7, triggerable.Object);

        // Act
        await engine.StartAsync(7);
        await Task.Delay(200); // Allow multiple firings
        await engine.StopAsync(7);

        // Assert
        Assert.True(trigger.FiredCount >= 2);
        triggerable.Verify(value => value.OnTrigger(), Times.AtLeast(2));
    }

    [Fact]
    public async Task Trigger_DisablesAfterMaxExecutions()
    {
        // Arrange
        Mock<IRepository<Trigger>> repository = new();
        AlwaysReadyTrigger trigger = new("Test trigger");
        trigger.SetMaxExecutions(3);
        repository
            .Setup(value => value.GetByIdAsync(7, It.IsAny<CancellationToken>()))
            .ReturnsAsync(trigger);
        using ServiceProvider provider = BuildProvider(repository.Object);
        TriggerEngine engine = new(provider.GetRequiredService<IServiceScopeFactory>());
        Mock<ITriggerable> triggerable = new();
        triggerable
            .Setup(value => value.OnTrigger())
            .ReturnsAsync(Result.CreateSuccess());
        await engine.SubscribeAsync(7, triggerable.Object);

        // Act
        await engine.StartAsync(7);
        await Task.Delay(500); // Allow all firings
        // Trigger should auto-disable after 3 executions

        // Assert
        Assert.Equal(3, trigger.FiredCount);
        Assert.False(trigger.Enabled);
        triggerable.Verify(value => value.OnTrigger(), Times.Exactly(3));
    }

    [Fact]
    public async Task Trigger_CanBeRestartedAfterDisable()
    {
        // Arrange
        Mock<IRepository<Trigger>> repository = new();
        BlockingTrigger trigger = new("Test trigger");
        repository
            .Setup(value => value.GetByIdAsync(7, It.IsAny<CancellationToken>()))
            .ReturnsAsync(trigger);
        using ServiceProvider provider = BuildProvider(repository.Object);
        TriggerEngine engine = new(provider.GetRequiredService<IServiceScopeFactory>());
        Mock<ITriggerable> triggerable = new();
        triggerable
            .Setup(value => value.OnTrigger())
            .ReturnsAsync(Result.CreateSuccess());
        await engine.SubscribeAsync(7, triggerable.Object);

        // Act - Start, stop while waiting, restart
        await engine.StartAsync(7).WaitAsync(TimeSpan.FromSeconds(5));
        await trigger.WaitStarted.WaitAsync(TimeSpan.FromSeconds(5));
        await engine.StopAsync(7);
        trigger.Complete(); // Allow first wait to complete
        await trigger.WaitCompletion;
        await Task.Delay(50); // Allow first RunAsync to fully exit
        
        // Restart
        await engine.StartAsync(7).WaitAsync(TimeSpan.FromSeconds(5));
        await trigger.WaitStarted.WaitAsync(TimeSpan.FromSeconds(5));
        trigger.Complete(); // Allow second wait to complete
        await trigger.WaitCompletion;
        // Wait for firing to complete
        for (int i = 0; i < 50 && trigger.FiredCount == 0; i++)
        {
            await Task.Delay(20);
        }
        await engine.StopAsync(7);

        // Assert
        Assert.True(trigger.FiredCount >= 1);
        triggerable.Verify(value => value.OnTrigger(), Times.AtLeast(1));
    }

    private sealed class FailingTrigger : Trigger
    {
        public FailingTrigger(string name) : base(name)
        {
        }

        protected override Task<Result> WaitUntilReadyAsync(
            CancellationToken cancellationToken)
            => Task.FromResult(Result.CreateFailure("Test trigger failure"));
    }

    private sealed class AlwaysReadyTrigger : Trigger
    {
        public AlwaysReadyTrigger(string name) : base(name)
        {
        }

        protected override Task<Result> WaitUntilReadyAsync(
            CancellationToken cancellationToken)
            => Task.FromResult(Result.CreateSuccess());
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
        private TaskCompletionSource<Result> _completion =
            new(TaskCreationOptions.RunContinuationsAsynchronously);
        private readonly TaskCompletionSource<bool> _waitStarted = new(TaskCreationOptions.RunContinuationsAsynchronously);

        public BlockingTrigger(string name) : base(name)
        {
        }

        public Task WaitCompletion => _completion.Task;
        public Task WaitStarted => _waitStarted.Task;

        public void Complete()
            => _completion.TrySetResult(Result.CreateSuccess());

        protected override async Task<Result> WaitUntilReadyAsync(
            CancellationToken cancellationToken)
        {
            var currentCompletion = _completion;
            _waitStarted.TrySetResult(true);
            using var registration = cancellationToken.Register(() => currentCompletion.TrySetResult(Result.CreateFailure("Canceled")));
            var result = await currentCompletion.Task;
            // Reset for next wait
            _completion = new TaskCompletionSource<Result>(TaskCreationOptions.RunContinuationsAsynchronously);
            return result;
        }
    }
}