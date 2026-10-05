using AgroEco.Core.Triggers;
using AgroEco.Core.Triggers.Implementations;
using AgroEco.Data;
using AgroEco.Data.Repositories;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace AgroEco.Data.IntegrationTests.Repositories;

public sealed class TriggerRepositoryTests
{
    [Fact]
    public async Task UpdateAsync_WhenNameChanges_PersistsUpdatedName()
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
            context.Triggers.Add(new DateTimeTrigger(
                "original",
                DateTimeOffset.UtcNow.AddHours(1)));
            await context.SaveChangesAsync();
        }

        await using DataContext updateContext = new(options);
        TriggerRepository repository = new(updateContext);
        Trigger trigger = Assert.IsType<DateTimeTrigger>(
            await repository.GetByIdAsync(1));
        Assert.True(trigger.UpdateDetails("updated").Success);

        // Act
        await repository.UpdateAsync(trigger);
        await updateContext.SaveChangesAsync();

        // Assert
        await using DataContext readContext = new(options);
        Trigger persisted = Assert.IsType<DateTimeTrigger>(
            await new TriggerRepository(readContext).GetByIdAsync(1));
        Assert.Equal("updated", persisted.Name);
    }
}
