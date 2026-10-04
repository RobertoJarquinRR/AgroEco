using AgroEco.Core;
using AgroEco.Core.Triggers;
using AgroEco.Core.Triggers.Implementations;
using AgroEco.Core.Jobs;
using AgroEco.Core.Jobs.Actions.Implementations;
using AgroEco.Data.Repositories;
using AgroEco.Data;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace AgroEco.Core.UnitTests
{
    public class UnitTest1
    {
        [Fact]
        public async Task DateTimeTriggerIsMaterializedThroughBaseType()
        {
            await using SqliteConnection connection = new("Data Source=:memory:");
            await connection.OpenAsync();

            DbContextOptions<DataContext> options = new DbContextOptionsBuilder<DataContext>()
                .UseSqlite(connection)
                .Options;

            DateTimeOffset targetTime = DateTimeOffset.UtcNow.AddHours(1);
            await using (DataContext writeContext = new(options))
            {
                await writeContext.Database.EnsureCreatedAsync();
                writeContext.Triggers.Add(new DateTimeTrigger("watering", targetTime));
                await writeContext.SaveChangesAsync();
            }

            await using (DataContext readContext = new(options))
            {
                Trigger? trigger = await readContext.Triggers
                    .SingleAsync();

                DateTimeTrigger dateTimeTrigger = Assert.IsType<DateTimeTrigger>(trigger);
                Assert.Equal(targetTime, dateTimeTrigger.TargetTime);
            }

        }

        [Fact]
        public async Task JobRejectsInvalidStatusTransition()
        {
            DateTimeTrigger trigger = new(
                "watering",
                DateTimeOffset.UtcNow.AddHours(1));
            Result<Job> result = await Job.CreateJob(
                "job",
                null,
                Status.Created,
                null,
                new List<Jobs.Actions.Action>(),
                trigger);

            Assert.True(result.Success);
            Result transition = result.Value.ChangeStatus(Status.Succeeded);

            Assert.False(transition.Success);
            Assert.Equal(Status.Created, result.Value.Status);
        }

        [Fact]
        public void ActionRejectsTransitionFromTerminalState()
        {
            ActionTest action = new("action", Status.Created);

            Assert.True(action.ChangeStatus(Status.Running).Success);
            Assert.True(action.ChangeStatus(Status.Succeeded).Success);
            Result transition = action.ChangeStatus(Status.Running);

            Assert.False(transition.Success);
            Assert.Equal(Status.Succeeded, action.Status);
        }

        [Fact]
        public void TriggerUpdateRejectsBlankName()
        {
            DateTimeTrigger trigger = new(
                "watering",
                DateTimeOffset.UtcNow.AddHours(1));

            Result result = trigger.UpdateDetails(" ");

            Assert.False(result.Success);
            Assert.Equal("watering", trigger.Name);
        }

        [Fact]
        public async Task RunningJobsIncludeActionsAndDerivedTrigger()
        {
            await using SqliteConnection connection = new("Data Source=:memory:");
            await connection.OpenAsync();

            DbContextOptions<DataContext> options = new DbContextOptionsBuilder<DataContext>()
                .UseSqlite(connection)
                .Options;

            DateTimeOffset targetTime = DateTimeOffset.UtcNow.AddHours(1);
            await using (DataContext writeContext = new(options))
            {
                await writeContext.Database.EnsureCreatedAsync();
                DateTimeTrigger trigger = new("watering", targetTime);
                ActionTest action = new("action", Status.Created);
                Result<Job> creation = await Job.CreateJob(
                    "running job",
                    "description",
                    Status.Created,
                    1,
                    new List<Jobs.Actions.Action> { action },
                    trigger);

                Assert.True(creation.Success);
                Job job = creation.Value;
                Assert.True(job.ChangeStatus(Status.Running).Success);
                writeContext.Jobs.Add(job);
                await writeContext.SaveChangesAsync();
            }

            await using DataContext readContext = new(options);
            JobRepository repository = new(readContext);
            List<Job> jobs = await repository.GetRunningJobsAsync();

            Job runningJob = Assert.Single(jobs);
            Assert.IsType<DateTimeTrigger>(runningJob.Trigger);
            Assert.Equal(targetTime, ((DateTimeTrigger)runningJob.Trigger).TargetTime);
            Assert.Single(runningJob.Actions);
        }
    }
}
