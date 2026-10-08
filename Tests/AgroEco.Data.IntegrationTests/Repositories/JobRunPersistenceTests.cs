using AgroEco.Core.Jobs;
using AgroEco.Core.Jobs.Runs;
using AgroEco.Core.Triggers.Implementations;
using AgroEco.Data;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace AgroEco.Data.IntegrationTests.Repositories;

public sealed class JobRunPersistenceTests
{
    private static DbContextOptions<DataContext> SqliteOptions(SqliteConnection connection)
        => new DbContextOptionsBuilder<DataContext>().UseSqlite(connection).Options;

    [Fact]
    public async Task JobRun_RoundTrips_WithActions()
    {
        // Arrange
        await using SqliteConnection connection = new("Data Source=:memory:");
        await connection.OpenAsync();
        DbContextOptions<DataContext> options = SqliteOptions(connection);

        await using (DataContext writeContext = new(options))
        {
            await writeContext.Database.EnsureCreatedAsync();

            var jobRun = new JobRun
            {
                JobId = 1,
                TriggerId = 10,
                TriggeredBy = TriggeredBy.Schedule,
                StartedAt = DateTimeOffset.UtcNow,
                Status = Status.Running,
                Message = "Job started",
                Error = null,
                Actions =
                [
                    new JobRunAction
                    {
                        JobRunId = 0,
                        ActionId = 100,
                        ActionName = "SendAlert",
                        ActionType = "sendAlert",
                        Status = Status.Succeeded,
                        Message = "Alert sent",
                        Error = null,
                        DurationMs = 150
                    },
                    new JobRunAction
                    {
                        JobRunId = 0,
                        ActionId = 101,
                        ActionName = "ExecuteTask",
                        ActionType = "executeTask",
                        Status = Status.Faulted,
                        Message = null,
                        Error = "Task failed",
                        DurationMs = 500
                    }
                ]
            };

            writeContext.JobRuns.Add(jobRun);
            await writeContext.SaveChangesAsync();
        }

        // Act
        await using DataContext readContext = new(options);
        JobRun? run = await readContext.JobRuns
            .Include(r => r.Actions)
            .FirstOrDefaultAsync(r => r.Id == 1);

        // Assert
        Assert.NotNull(run);
        Assert.Equal(1, run.JobId);
        Assert.Equal(10, run.TriggerId);
        Assert.Equal(TriggeredBy.Schedule, run.TriggeredBy);
        Assert.Equal(Status.Running, run.Status);
        Assert.Equal("Job started", run.Message);
        Assert.Null(run.Error);

        Assert.Equal(2, run.Actions.Count);

        JobRunAction action1 = run.Actions.First(a => a.ActionName == "SendAlert");
        Assert.Equal(100, action1.ActionId);
        Assert.Equal("sendAlert", action1.ActionType);
        Assert.Equal(Status.Succeeded, action1.Status);
        Assert.Equal("Alert sent", action1.Message);
        Assert.Null(action1.Error);
        Assert.Equal(150, action1.DurationMs);

        JobRunAction action2 = run.Actions.First(a => a.ActionName == "ExecuteTask");
        Assert.Equal(101, action2.ActionId);
        Assert.Equal("executeTask", action2.ActionType);
        Assert.Equal(Status.Faulted, action2.Status);
        Assert.Null(action2.Message);
        Assert.Equal("Task failed", action2.Error);
        Assert.Equal(500, action2.DurationMs);
    }
}