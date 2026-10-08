using AgroEco.Core;
using AgroEco.Core.Jobs;
using AgroEco.Core.Jobs.Actions;
using AgroEco.Core.Jobs.Actions.Implementations;
using AgroEco.Core.Jobs.Engine;
using AgroEco.Core.Jobs.Runs;
using AgroEco.Core.Triggers.Implementations;
using CoreAction = AgroEco.Core.Jobs.Actions.Action;

namespace AgroEco.Core.UnitTests.Jobs;

public sealed class JobTests
{
    [Fact]
    public async Task CreateJob_WithoutActions_ReturnsFailure()
    {
        // Arrange
        DateTimeTrigger trigger = new(
            "watering",
            DateTimeOffset.UtcNow.AddHours(1));

        // Act
        Result<Job> result = await Job.CreateJob(
            "job",
            null,
            Status.Created,
            null,
            [],
            trigger);

        // Assert
        Assert.False(result.Success);
        Assert.Contains("action", result.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task ChangeStatus_WhenStatusIsAlreadyCurrent_ReturnsSuccess()
    {
        // Arrange
        Result<Job> creation = await Job.CreateJob(
            "job",
            null,
            Status.Created,
            null,
            [new NoOpAction("action")],
            new DateTimeTrigger("watering", DateTimeOffset.UtcNow.AddHours(1)));

        Job job = Assert.IsType<Job>(creation.Value);
        Assert.True(job.ChangeStatus(Status.Running).Success);

        // Act
        Result result = job.ChangeStatus(Status.Running);

        // Assert
        Assert.True(result.Success);
        Assert.Equal(Status.Running, job.Status);
    }

    [Fact]
    public async Task ChangeStatus_FromCreatedToSucceeded_ReturnsFailure()
    {
        // Arrange
        Result<Job> creation = await Job.CreateJob(
            "job",
            null,
            Status.Created,
            null,
            [new NoOpAction("action")],
            new DateTimeTrigger("watering", DateTimeOffset.UtcNow.AddHours(1)));

        Job job = Assert.IsType<Job>(creation.Value);

        // Act
        Result result = job.ChangeStatus(Status.Succeeded);

        // Assert
        Assert.False(result.Success);
        Assert.Equal(Status.Created, job.Status);
    }

    [Fact]
    public async Task PrepareForRun_WhenJobIsSucceeded_ResetsToCreated()
    {
        // Arrange
        Result<Job> creation = await Job.CreateJob(
            "job",
            null,
            Status.Created,
            null,
            [new NoOpAction("action")],
            new DateTimeTrigger("watering", DateTimeOffset.UtcNow.AddHours(1)));

        Job job = Assert.IsType<Job>(creation.Value);
        job.ChangeStatus(Status.Running);
        job.ChangeStatus(Status.Succeeded);
        Assert.Equal(Status.Succeeded, job.Status);

        // Act
        Result result = job.PrepareForRun();

        // Assert
        Assert.True(result.Success);
        Assert.Equal(Status.Created, job.Status);
        Assert.Equal(Status.Created, job.Actions[0].Status);
    }

    [Fact]
    public async Task PrepareForRun_WhenJobIsFaulted_ResetsToCreated()
    {
        // Arrange
        Result<Job> creation = await Job.CreateJob(
            "job",
            null,
            Status.Created,
            null,
            [new NoOpAction("action")],
            new DateTimeTrigger("watering", DateTimeOffset.UtcNow.AddHours(1)));

        Job job = Assert.IsType<Job>(creation.Value);
        job.ChangeStatus(Status.Running);
        job.ChangeStatus(Status.Faulted);
        Assert.Equal(Status.Faulted, job.Status);

        // Act
        Result result = job.PrepareForRun();

        // Assert
        Assert.True(result.Success);
        Assert.Equal(Status.Created, job.Status);
        Assert.Equal(Status.Created, job.Actions[0].Status);
    }

    [Fact]
    public async Task PrepareForRun_WhenJobIsRunning_ResetsToCreated()
    {
        // Arrange
        Result<Job> creation = await Job.CreateJob(
            "job",
            null,
            Status.Created,
            null,
            [new NoOpAction("action")],
            new DateTimeTrigger("watering", DateTimeOffset.UtcNow.AddHours(1)));

        Job job = Assert.IsType<Job>(creation.Value);
        job.ChangeStatus(Status.Running);

        // Act
        Result result = job.PrepareForRun();

        // Assert
        Assert.True(result.Success);
        Assert.Equal(Status.Created, job.Status);
        Assert.Equal(Status.Created, job.Actions[0].Status);
    }

    [Fact]
    public async Task ChangeStatus_FromSucceededToCreated_ReturnsSuccess()
    {
        // Arrange
        Result<Job> creation = await Job.CreateJob(
            "job",
            null,
            Status.Created,
            null,
            [new NoOpAction("action")],
            new DateTimeTrigger("watering", DateTimeOffset.UtcNow.AddHours(1)));

        Job job = Assert.IsType<Job>(creation.Value);
        job.ChangeStatus(Status.Running);
        job.ChangeStatus(Status.Succeeded);

        // Act
        Result result = job.ChangeStatus(Status.Created);

        // Assert
        Assert.True(result.Success);
        Assert.Equal(Status.Created, job.Status);
    }

    private sealed class FailingAction : CoreAction
    {
        public FailingAction() : base("failing action")
        {
        }

        public override Task<Result> Execute()
            => Task.FromResult(Result.CreateFailure("Expected failure"));
    }

    private sealed class ActionWithInvalidCompletionTransition : CoreAction
    {
        public ActionWithInvalidCompletionTransition() : base("invalid completion")
        {
        }

        public override Task<Result> Execute()
        {
            ChangeStatus(Status.Canceled);
            return Task.FromResult(Result.CreateSuccess());
        }
    }
}

public sealed class JobRunnerTests
{
    [Fact]
    public async Task JobRunner_RunsTwice_ProducesTwoRunsWithActions()
    {
        // Arrange
        DateTimeTrigger trigger = new(
            "watering",
            DateTimeOffset.UtcNow.AddHours(1));
        Result<Job> creation = await Job.CreateJob(
            "job",
            null,
            Status.Created,
            null,
            [new NoOpAction("action1"), new NoOpAction("action2")],
            trigger);

        Job job = Assert.IsType<Job>(creation.Value);
        var runner = new JobRunner();

        // Act - First run
        JobRun firstRun = await runner.RunAsync(job, 1, TriggeredBy.Schedule);

        // Assert - First run
        Assert.Equal(Status.Succeeded, firstRun.Status);
        Assert.Equal(2, firstRun.Actions.Count);
        Assert.All(firstRun.Actions, a => Assert.Equal(Status.Succeeded, a.Status));
        Assert.Equal(Status.Succeeded, job.Status);

        // Act - Second run (same job, should be able to run again)
        JobRun secondRun = await runner.RunAsync(job, 1, TriggeredBy.Schedule);

        // Assert - Second run
        Assert.Equal(Status.Succeeded, secondRun.Status);
        Assert.Equal(2, secondRun.Actions.Count);
        Assert.All(secondRun.Actions, a => Assert.Equal(Status.Succeeded, a.Status));
        Assert.Equal(Status.Succeeded, job.Status);

        // Verify they are separate runs
        Assert.NotEqual(firstRun.StartedAt, secondRun.StartedAt);
        Assert.NotEqual(firstRun.FinishedAt, secondRun.FinishedAt);
    }

    [Fact]
    public async Task JobRunner_WhenAnActionFails_ReturnsRunWithFailureAndError()
    {
        // Arrange
        DateTimeTrigger trigger = new(
            "watering",
            DateTimeOffset.UtcNow.AddHours(1));
        Result<Job> creation = await Job.CreateJob(
            "job",
            null,
            Status.Created,
            null,
            [new NoOpAction("successful action"), new FailingAction()],
            trigger);

        Job job = Assert.IsType<Job>(creation.Value);
        var runner = new JobRunner();

        // Act
        JobRun run = await runner.RunAsync(job, 1, TriggeredBy.Schedule);

        // Assert
        Assert.Equal(Status.CompletedWithErrors, run.Status);
        Assert.Equal(2, run.Actions.Count);
        Assert.Contains(run.Actions, a => a.Status == Status.Succeeded && a.ActionName == "successful action");
        Assert.Contains(run.Actions, a => a.Status == Status.Faulted && a.ActionName == "failing action");
        Assert.NotNull(run.Error);
        Assert.Contains("Expected failure", run.Error, StringComparison.OrdinalIgnoreCase);
        Assert.Equal(Status.CompletedWithErrors, job.Status);
    }

    [Fact]
    public async Task JobRunner_WhenAllActionsFail_ReturnsFaultedRun()
    {
        // Arrange
        DateTimeTrigger trigger = new(
            "watering",
            DateTimeOffset.UtcNow.AddHours(1));
        Result<Job> creation = await Job.CreateJob(
            "job",
            null,
            Status.Created,
            null,
            [new FailingAction(), new FailingAction()],
            trigger);

        Job job = Assert.IsType<Job>(creation.Value);
        var runner = new JobRunner();

        // Act
        JobRun run = await runner.RunAsync(job, 1, TriggeredBy.Schedule);

        // Assert
        Assert.Equal(Status.Faulted, run.Status);
        Assert.Equal(2, run.Actions.Count);
        Assert.All(run.Actions, a => Assert.Equal(Status.Faulted, a.Status));
        Assert.NotNull(run.Error);
        Assert.Equal(Status.Faulted, job.Status);
    }

    private sealed class FailingAction : CoreAction
    {
        public FailingAction() : base("failing action")
        {
        }

        public override Task<Result> Execute()
            => Task.FromResult(Result.CreateFailure("Expected failure"));
    }
}