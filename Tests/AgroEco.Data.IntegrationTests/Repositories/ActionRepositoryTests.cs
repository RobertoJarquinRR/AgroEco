using AgroEco.Core;
using AgroEco.Core.Jobs;
using AgroEco.Core.Jobs.Actions;
using AgroEco.Core.Jobs.Actions.Implementations;
using AgroEco.Core.Triggers.Implementations;
using AgroEco.Data;
using AgroEco.Data.Repositories;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace AgroEco.Data.IntegrationTests.Repositories;

public sealed class ActionRepositoryTests
{
    [Fact]
    public async Task UpdateAsync_WhenStatusChanges_PersistsUpdatedStatus()
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
                "job",
                null,
                Status.Created,
                null,
                [new NoOpAction("test action")],
                new DateTimeTrigger(
                    "trigger",
                    DateTimeOffset.UtcNow.AddHours(1)));
            context.Jobs.Add(Assert.IsType<Job>(creation.Value));
            await context.SaveChangesAsync();
        }

        await using DataContext updateContext = new(options);
        ActionRepository repository = new(updateContext);
        AgroEco.Core.Jobs.Actions.Action action = Assert.IsType<NoOpAction>(
            await repository.GetByIdAsync(1));
        Assert.True(action.ChangeStatus(Status.Running).Success);

        // Act
        await repository.UpdateAsync(action);
        await updateContext.SaveChangesAsync();

        // Assert
        await using DataContext readContext = new(options);
        AgroEco.Core.Jobs.Actions.Action persisted = Assert.IsType<NoOpAction>(
            await new ActionRepository(readContext).GetByIdAsync(1));
        Assert.Equal(Status.Running, persisted.Status);
    }
}
