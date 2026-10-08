using AgroEco.Core;
using AgroEco.Core.Jobs;
using AgroEco.Core.Jobs.Actions.Implementations;
using AgroEco.Core.Triggers.Implementations;
using AgroEco.Data;
using AgroEco.Data.Repositories;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using AgroEco.Core.Triggers.Configuration;

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
                [new NoOpAction("action")],
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
    public async Task GetRunningJobsAsync_WhenEnabledTriggerExists_ReturnsJob()
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
            var trigger = new DateTimeTrigger(
                "watering",
                DateTimeOffset.UtcNow.AddHours(1));
            trigger.Enable(); // Enable the trigger
            
            Result<Job> creation = await Job.CreateJob(
                "running job",
                null,
                Status.Created,
                null,
                [new NoOpAction("action")],
                trigger);
            Assert.True(creation.Success);
            context.Jobs.Add(creation.Value);
            await context.SaveChangesAsync();
        }

        // Act
        await using DataContext readContext = new(options);
        List<Job> jobs = await new JobRepository(readContext)
            .GetRunningJobsAsync();

        // Assert
        Job job = Assert.Single(jobs);
        Assert.NotNull(job.Trigger);
        Assert.True(job.Trigger.Enabled);
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
                [new NoOpAction("action")],
                new DateTimeTrigger("watering", originalTarget));
            context.Jobs.Add(Assert.IsType<Job>(creation.Value));
            await context.SaveChangesAsync();
        }

        await using DataContext updateContext = new(options);
        JobRepository repository = new(updateContext);
        Job job = Assert.IsType<Job>(await repository.GetByIdWithDetailsAsync(1));
        Assert.True(
            Assert.IsType<DateTimeTrigger>(job.Trigger)
                .UpdateConfiguration(new DateTimeTriggerConfiguration(updatedTarget))
                .Success);

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
