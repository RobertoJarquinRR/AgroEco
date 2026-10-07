using AgroEco.Core;
using AgroEco.Core.Alerts;
using AgroEco.Core.Alerts.Persistence;
using AgroEco.Core.Interfaces;
using AgroEco.Data;
using AgroEco.Data.Repositories;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace AgroEco.Data.IntegrationTests.Repositories;

public sealed class AlertDeliveryIntegrationTests
{
    private static ServiceProvider BuildProvider(SqliteConnection connection)
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddDbContext<DataContext>(o => o.UseSqlite(connection));
        services.AddScoped<IRepository<AlertEntity>, AlertRepository>();
        services.AddScoped<IAlertRepository, AlertRepository>();
        services.AddScoped<IUnitOfWork, UnitOfWork<DataContext>>();
        services.AddScoped<CreateAlert>();
        services.AddScoped<UpdateAlert>();
        services.AddScoped<GetAllAlerts>();
        services.AddScoped<GetAlertById>();
        services.AddScoped<DeleteAlert>();
        return services.BuildServiceProvider();
    }

    private static async Task<(Result result, List<AlertEntity> alerts)> RaiseAsync(
        AlertConfiguration config,
        params IAlertChannel[] channels)
    {
        await using SqliteConnection connection = new("Data Source=:memory:");
        await connection.OpenAsync();
        using ServiceProvider provider = BuildProvider(connection);

        using (IServiceScope scope = provider.CreateScope())
        {
            await scope.ServiceProvider.GetRequiredService<DataContext>().Database.MigrateAsync();
        }

        var engine = new AlertEngine(provider.GetRequiredService<IServiceScopeFactory>());
        foreach (IAlertChannel channel in channels)
        {
            engine.RegisterChannel(channel);
        }

        Result result = await engine.RaiseAsync("t", "m", AlertLevel.Info, config);

        using (IServiceScope scope = provider.CreateScope())
        {
            var repository = scope.ServiceProvider.GetRequiredService<IAlertRepository>();
            List<AlertEntity> alerts = await repository.GetAllWithDeliveriesAsync();
            return (result, alerts);
        }
    }

    [Fact]
    public async Task Raise_WithoutAnyChannel_StillPersistsTheAlert()
    {
        // Arrange
        var config = new AlertConfiguration();

        // Act
        (Result result, List<AlertEntity> alerts) = await RaiseAsync(config);

        // Assert
        Assert.True(result.Success, result.Message);
        Assert.Contains("no delivery channels", result.Message);
        Assert.Single(alerts);
        Assert.Empty(alerts[0].Deliveries);
    }

    [Fact]
    public async Task Raise_WithSucceedingAndFailingChannel_ReportsPartialSuccessAndPersistsDeliveries()
    {
        // Arrange
        var config = new AlertConfiguration().EnableChannel("ok").EnableChannel("bad");

        // Act
        (Result result, List<AlertEntity> alerts) = await RaiseAsync(
            config,
            new StubChannel("ok", succeed: true),
            new StubChannel("bad", succeed: false));

        // Assert
        Assert.True(result.Success, result.Message);
        AlertEntity alert = Assert.Single(alerts);
        Assert.Equal(2, alert.Deliveries.Count);
        Assert.Contains(alert.Deliveries, d => d.ChannelType == "ok" && d.Success);
        Assert.Contains(alert.Deliveries, d => d.ChannelType == "bad" && !d.Success);
    }

    [Fact]
    public async Task Raise_WithOnlyFailingChannel_FailsButStillPersists()
    {
        // Arrange
        var config = new AlertConfiguration().EnableChannel("bad");

        // Act
        (Result result, List<AlertEntity> alerts) = await RaiseAsync(
            config,
            new StubChannel("bad", succeed: false));

        // Assert
        Assert.False(result.Success);
        AlertEntity alert = Assert.Single(alerts);
        Assert.Single(alert.Deliveries);
        Assert.False(alert.Deliveries[0].Success);
    }

    private sealed class StubChannel : IAlertChannel
    {
        private readonly bool _succeed;

        public StubChannel(string channelType, bool succeed)
        {
            ChannelType = channelType;
            _succeed = succeed;
        }

        public string ChannelType { get; }
        public bool IsEnabled { get; set; } = true;
        public int Priority => 10;

        public Task<AlertDeliveryResult> SendAsync(Alert alert, CancellationToken cancellationToken = default)
            => Task.FromResult(_succeed
                ? AlertDeliveryResult.SuccessResult(ChannelType, TimeSpan.Zero)
                : AlertDeliveryResult.Failure(ChannelType, "simulated failure", TimeSpan.Zero));

        public Task<Result> InitializeAsync(CancellationToken cancellationToken = default)
            => Task.FromResult(Result.CreateSuccess());

        public Task ShutdownAsync(CancellationToken cancellationToken = default)
            => Task.CompletedTask;
    }
}
