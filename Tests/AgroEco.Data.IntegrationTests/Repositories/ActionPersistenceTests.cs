using AgroEco.Core;
using AgroEco.Core.Alerts;
using AgroEco.Core.Jobs;
using AgroEco.Core.Jobs.Actions.Configuration;
using AgroEco.Core.Jobs.Actions.Implementations;
using AgroEco.Core.Triggers.Implementations;
using AgroEco.Data;
using AgroEco.Data.Repositories;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace AgroEco.Data.IntegrationTests.Repositories;

public sealed class ActionPersistenceTests
{
    private static DbContextOptions<DataContext> SqliteOptions(SqliteConnection connection)
        => new DbContextOptionsBuilder<DataContext>().UseSqlite(connection).Options;

    [Fact]
    public async Task GetByIdWithDetailsAsync_SendAlertAction_RoundTripsTypeAndConfiguration()
    {
        // Arrange
        await using SqliteConnection connection = new("Data Source=:memory:");
        await connection.OpenAsync();
        DbContextOptions<DataContext> options = SqliteOptions(connection);

        using ServiceProvider provider = new ServiceCollection().BuildServiceProvider();
        AlertEngine engine = new(provider.GetRequiredService<IServiceScopeFactory>());

        await using (DataContext writeContext = new(options))
        {
            await writeContext.Database.EnsureCreatedAsync();
            var action = new SendAlertAction("accion", engine)
            {
                Config = new SendAlertActionConfiguration
                {
                    Title = "Riego bajo",
                    Message = "Nivel crítico",
                    Level = AlertLevel.Warning,
                    EnableChannels = new List<string> { "database", "windows-toast" },
                    Options = new Dictionary<string, object> { ["actionUrl"] = "/alertas/7" }
                }
            };

            Result<Job> creation = await Job.CreateJob(
                "alert job", null, Status.Created, 1, [action],
                new DateTimeTrigger("t", DateTimeOffset.UtcNow.AddHours(1)));
            Assert.True(creation.Success, creation.Message);
            writeContext.Jobs.Add(creation.Value);
            await writeContext.SaveChangesAsync();
        }

        // Act
        await using DataContext readContext = new(options);
        Job? job = await new JobRepository(readContext).GetByIdWithDetailsAsync(1);

        // Assert
        Assert.NotNull(job);
        SendAlertAction restored = Assert.IsType<SendAlertAction>(Assert.Single(job.Actions));
        Assert.Equal("Riego bajo", restored.Config.Title);
        Assert.Equal("Nivel crítico", restored.Config.Message);
        Assert.Equal(AlertLevel.Warning, restored.Config.Level);
        Assert.Equal(new[] { "database", "windows-toast" }, restored.Config.EnableChannels);
        Assert.Equal("/alertas/7", restored.Config.Options["actionUrl"]);
    }

    [Fact]
    public async Task GetByIdWithDetailsAsync_ExecuteTaskAction_RoundTripsTypeAndConfiguration()
    {
        // Arrange
        await using SqliteConnection connection = new("Data Source=:memory:");
        await connection.OpenAsync();
        DbContextOptions<DataContext> options = SqliteOptions(connection);

        using ServiceProvider provider = new ServiceCollection().BuildServiceProvider();

        await using (DataContext writeContext = new(options))
        {
            await writeContext.Database.EnsureCreatedAsync();
            var action = new ExecuteTaskAction(
                "accion",
                provider.GetRequiredService<IServiceScopeFactory>())
            {
                Config = new ExecuteTaskActionConfiguration
                {
                    InsumoId = 5,
                    CantidadDescontar = 2.5m,
                    CostoUnitario = 10m,
                    Cultivo = "café"
                }
            };

            Result<Job> creation = await Job.CreateJob(
                "inventory job", null, Status.Created, 2, [action],
                new DateTimeTrigger("t", DateTimeOffset.UtcNow.AddHours(1)));
            Assert.True(creation.Success, creation.Message);
            writeContext.Jobs.Add(creation.Value);
            await writeContext.SaveChangesAsync();
        }

        // Act
        await using DataContext readContext = new(options);
        Job? job = await new JobRepository(readContext).GetByIdWithDetailsAsync(1);

        // Assert
        Assert.NotNull(job);
        ExecuteTaskAction restored = Assert.IsType<ExecuteTaskAction>(Assert.Single(job.Actions));
        Assert.Equal(5, restored.Config.InsumoId);
        Assert.Equal(2.5m, restored.Config.CantidadDescontar);
        Assert.Equal(10m, restored.Config.CostoUnitario);
        Assert.Equal("café", restored.Config.Cultivo);
    }

    [Fact]
    public async Task GetByIdWithDetailsAsync_UnknownDiscriminator_StillReadsBaseRow()
    {
        // Guarda: NoOpAction sigue materializándose tras mover la jerarquía a TPH.
        // Arrange
        await using SqliteConnection connection = new("Data Source=:memory:");
        await connection.OpenAsync();
        DbContextOptions<DataContext> options = SqliteOptions(connection);

        await using (DataContext writeContext = new(options))
        {
            await writeContext.Database.EnsureCreatedAsync();
            Result<Job> creation = await Job.CreateJob(
                "noop job", null, Status.Created, 3, [new NoOpAction("accion")],
                new DateTimeTrigger("t", DateTimeOffset.UtcNow.AddHours(1)));
            Assert.True(creation.Success, creation.Message);
            writeContext.Jobs.Add(creation.Value);
            await writeContext.SaveChangesAsync();
        }

        // Act
        await using DataContext readContext = new(options);
        Job? job = await new JobRepository(readContext).GetByIdWithDetailsAsync(1);

        // Assert
        Assert.NotNull(job);
        Assert.IsType<NoOpAction>(Assert.Single(job.Actions));
    }
}
