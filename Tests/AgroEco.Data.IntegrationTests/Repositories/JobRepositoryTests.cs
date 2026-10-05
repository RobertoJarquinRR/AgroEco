using AgroEco.Core;
using AgroEco.Core.Jobs;
using AgroEco.Core.Jobs.Actions.Implementations;
using AgroEco.Core.Triggers.Implementations;
using AgroEco.Data;
using AgroEco.Data.Repositories;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace AgroEco.Data.IntegrationTests.Repositories;

public sealed class JobRepositoryTests
{
    [Fact]
    public async Task GetByIdWithDetailsAsync_ExistingJob_ReturnsTriggerAndActions()
    {
        // Arrange
        await using SqliteConnection connection = new("Data Source=:memory:");
        await connection.OpenAsync();
        DbContextOptions<DataContext> options = new DbContextOptionsBuilder<DataContext>()
            .UseSqlite(connection)
            .Options;

        await using (DataContext writeContext = new(options))
        {
            await writeContext.Database.EnsureCreatedAsync();
            Result<Job> creation = await Job.CreateJob(
                "running job",
                "description",
                Status.Running,
                1,
                [new ActionTest("action")],
                new DateTimeTrigger(
                    "watering",
                    DateTimeOffset.UtcNow.AddHours(1)));
            Assert.True(creation.Success);
            writeContext.Jobs.Add(creation.Value);
            await writeContext.SaveChangesAsync();
        }

        // Act
        await using DataContext readContext = new(options);
        Job? job = await new JobRepository(readContext)
            .GetByIdWithDetailsAsync(1);

        // Assert
        Assert.NotNull(job);
        Assert.IsType<DateTimeTrigger>(job.Trigger);
        Assert.Single(job.Actions);
    }

    [Fact]
    public async Task GetRunningJobsAsync_WhenRunningJobExists_ReturnsOnlyRunningJobs()
    {
        // Arrange
        await using SqliteConnection connection = new("Data Source=:memory:");
        await connection.OpenAsync();
        DbContextOptions<DataContext> options = new DbContextOptionsBuilder<DataContext>()
            .UseSqlite(connection)
            .Options;

        await using (DataContext context = new(options))
        {
            await context.Database.EnsureCreatedAsync();
            Result<Job> creation = await Job.CreateJob(
                "running job",
                null,
                Status.Created,
                null,
                [new ActionTest("action")],
                new DateTimeTrigger(
                    "watering",
                    DateTimeOffset.UtcNow.AddHours(1)));
            Assert.True(creation.Success);
            Assert.True(creation.Value.ChangeStatus(Status.Running).Success);
            context.Jobs.Add(creation.Value);
            await context.SaveChangesAsync();
        }

        // Act
        await using DataContext readContext = new(options);
        List<Job> jobs = await new JobRepository(readContext)
            .GetRunningJobsAsync();

        // Assert
        Job job = Assert.Single(jobs);
        Assert.Equal(Status.Running, job.Status);
        Assert.NotNull(job.Trigger);
        Assert.Single(job.Actions);
    }

    [Fact]
    public async Task UpdateAsync_WhenDateTimeTriggerChanges_PersistsNewTargetTime()
    {
        // Arrange
        await using SqliteConnection connection = new("Data Source=:memory:");
        await connection.OpenAsync();
        DbContextOptions<DataContext> options = new DbContextOptionsBuilder<DataContext>()
            .UseSqlite(connection)
            .Options;
        DateTimeOffset originalTarget = DateTimeOffset.UtcNow.AddHours(1);
        DateTimeOffset updatedTarget = DateTimeOffset.UtcNow.AddDays(1);

        await using (DataContext context = new(options))
        {
            await context.Database.EnsureCreatedAsync();
            Result<Job> creation = await Job.CreateJob(
                "job",
                null,
                Status.Created,
                null,
                [new ActionTest("action")],
                new DateTimeTrigger("watering", originalTarget));
            context.Jobs.Add(Assert.IsType<Job>(creation.Value));
            await context.SaveChangesAsync();
        }

        await using DataContext updateContext = new(options);
        JobRepository repository = new(updateContext);
        Job job = Assert.IsType<Job>(await repository.GetByIdWithDetailsAsync(1));
        using JsonDocument config = JsonDocument.Parse(
            $$"""{"targetTime":"{{updatedTarget:O}}"}""");
        Assert.True(job.Trigger.UpdateConfiguration(config.RootElement).Success);

        // Act
        await repository.UpdateAsync(job);
        await updateContext.SaveChangesAsync();

        // Assert
        await using DataContext readContext = new(options);
        Job persisted = Assert.IsType<Job>(
            await new JobRepository(readContext).GetByIdWithDetailsAsync(1));
        DateTimeTrigger trigger = Assert.IsType<DateTimeTrigger>(persisted.Trigger);
        Assert.Equal(updatedTarget, trigger.TargetTime);
    }
}
