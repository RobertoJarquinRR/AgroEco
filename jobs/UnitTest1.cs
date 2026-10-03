using AgroEco.Core.Triggers;
using AgroEco.Core.Triggers.Implementations;
using AgroEco.Data;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace jobs
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
    }
}
