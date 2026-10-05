using AgroEco.Core;
using AgroEco.Core.Jobs;
using AgroEco.Core.Jobs.Actions;
using AgroEco.Core.Jobs.Actions.Implementations;
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
    public async Task OnTrigger_WhenActionFails_MarksJobAsFaulted()
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
            [new FailingAction()],
            trigger);

        Job job = Assert.IsType<Job>(creation.Value);

        // Act
        Result result = await job.OnTrigger();

        // Assert
        Assert.False(result.Success);
        Assert.Equal(Status.Faulted, job.Status);
    }

    [Fact]
    public async Task OnTrigger_WhenSomeActionsFail_MarksJobAsCompletedWithErrors()
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

        // Act
        Result result = await job.OnTrigger();

        // Assert
        Assert.False(result.Success);
        Assert.Equal(Status.CompletedWithErrors, job.Status);
    }

    [Fact]
    public async Task OnTrigger_WhenActionCannotCompleteTransition_MarksJobAsFaulted()
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
            [new ActionWithInvalidCompletionTransition()],
            trigger);

        Job job = Assert.IsType<Job>(creation.Value);

        // Act
        Result result = await job.OnTrigger();

        // Assert
        Assert.False(result.Success);
        Assert.Equal(Status.Faulted, job.Status);
        Assert.Contains(
            job.Results,
            item => (item.Message ?? string.Empty)
                .Contains("cannot transition", StringComparison.OrdinalIgnoreCase));
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
