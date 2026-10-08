using AgroEco.Core.Triggers.Events;
using AgroEco.Data;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace AgroEco.Data.IntegrationTests.Repositories;

public sealed class TriggerEventPersistenceTests
{
    private static DbContextOptions<DataContext> SqliteOptions(SqliteConnection connection)
        => new DbContextOptionsBuilder<DataContext>().UseSqlite(connection).Options;

    [Fact]
    public async Task TriggerEvent_RoundTrips()
    {
        // Arrange
        await using SqliteConnection connection = new("Data Source=:memory:");
        await connection.OpenAsync();
        DbContextOptions<DataContext> options = SqliteOptions(connection);

        await using (DataContext writeContext = new(options))
        {
            await writeContext.Database.EnsureCreatedAsync();

            var triggerEvent = new TriggerEvent
            {
                TriggerId = 5,
                OccurredAt = DateTimeOffset.UtcNow,
                EventType = TriggerEventType.Fired,
                Message = "Trigger fired successfully",
                JobRunId = 42
            };

            writeContext.TriggerEvents.Add(triggerEvent);
            await writeContext.SaveChangesAsync();
        }

        // Act
        await using DataContext readContext = new(options);
        TriggerEvent? evt = await readContext.TriggerEvents
            .FirstOrDefaultAsync(e => e.Id == 1);

        // Assert
        Assert.NotNull(evt);
        Assert.Equal(5, evt.TriggerId);
        Assert.Equal(TriggerEventType.Fired, evt.EventType);
        Assert.Equal("Trigger fired successfully", evt.Message);
        Assert.Equal(42, evt.JobRunId);
    }
}