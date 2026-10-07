using AgroEco.Core;
using AgroEco.Core.Alerts;
using AgroEco.Core.UnitTests.TestSupport;
using Microsoft.Extensions.DependencyInjection;

namespace AgroEco.Core.UnitTests.Alerts;

public sealed class AlertEngineTests
{
    [Fact]
    public async Task RaiseAsync_WhenAllChannelsFail_DoesNotMarkAlertAsDelivered()
    {
        // Arrange
        using ServiceProvider provider = AlertTestProvider.Create();
        AlertEngine engine = new(provider.GetRequiredService<IServiceScopeFactory>());
        engine.RegisterChannel(new FailingChannel());
        Alert alert = Alert.Create(
            "Test alert",
            "Delivery failed",
            config: new AlertConfiguration().EnableChannel("database"));

        // Act
        Result result = await engine.RaiseAsync(alert);

        // Assert
        Assert.False(result.Success);
        Assert.Equal(AlertStatus.Failed, alert.Status);
        Assert.False(alert.Deliveries.First(d => d.ChannelType == "database").Success);
    }

    [Fact]
    public async Task RaiseAsync_WhenChannelSucceeds_MarksAlertAsDelivered()
    {
        // Arrange
        using ServiceProvider provider = AlertTestProvider.Create();
        AlertEngine engine = new(provider.GetRequiredService<IServiceScopeFactory>());
        engine.RegisterChannel(new SuccessfulChannel());
        Alert alert = Alert.Create(
            "Test alert",
            "Delivery succeeded",
            config: new AlertConfiguration().EnableChannel("database"));

        // Act
        Result result = await engine.RaiseAsync(alert);

        // Assert
        Assert.True(result.Success);
        Assert.Equal(AlertStatus.Delivered, alert.Status);
        Assert.True(alert.Deliveries.First(d => d.ChannelType == "database").Success);
    }

    [Fact]
    public async Task RaiseAsync_WhenChannelExceedsTimeout_ReturnsFailureDelivery()
    {
        // Arrange
        using ServiceProvider provider = AlertTestProvider.Create();
        AlertEngine engine = new(
            provider.GetRequiredService<IServiceScopeFactory>(),
            new AlertEngineOptions { DefaultTimeout = TimeSpan.FromMilliseconds(10) });
        engine.RegisterChannel(new SlowChannel());
        Alert alert = Alert.Create(
            "Test alert",
            "Delivery timed out",
            config: new AlertConfiguration().EnableChannel("slow"));

        // Act
        Result result = await engine.RaiseAsync(alert);

        // Assert
        Assert.False(result.Success);
        AlertDeliveryEntity delivery = Assert.Single(alert.Deliveries);
        Assert.Equal("slow", delivery.ChannelType);
        Assert.False(delivery.Success);
        Assert.Contains("Timeout", delivery.ErrorMessage);
    }

    private class FailingChannel : IAlertChannel
    {
        public string ChannelType => "database";
        public bool IsEnabled { get; set; } = true;
        public int Priority => 1;

        public virtual Task<AlertDeliveryResult> SendAsync(
            Alert alert,
            CancellationToken cancellationToken = default)
            => Task.FromResult(AlertDeliveryResult.Failure(
                ChannelType,
                "Expected failure",
                TimeSpan.Zero));

        public Task<Result> InitializeAsync(CancellationToken cancellationToken = default)
            => Task.FromResult(Result.CreateSuccess());

        public Task ShutdownAsync(CancellationToken cancellationToken = default)
            => Task.CompletedTask;
    }

    private sealed class SuccessfulChannel : FailingChannel
    {
        public override Task<AlertDeliveryResult> SendAsync(
            Alert alert,
            CancellationToken cancellationToken = default)
            => Task.FromResult(AlertDeliveryResult.SuccessResult(
                ChannelType,
                TimeSpan.Zero));
    }

    private sealed class SlowChannel : IAlertChannel
    {
        public string ChannelType => "slow";
        public bool IsEnabled { get; set; } = true;
        public int Priority => 1;

        public async Task<AlertDeliveryResult> SendAsync(
            Alert alert,
            CancellationToken cancellationToken = default)
        {
            await Task.Delay(Timeout.InfiniteTimeSpan, cancellationToken);
            return AlertDeliveryResult.SuccessResult(ChannelType, TimeSpan.Zero);
        }

        public Task<Result> InitializeAsync(CancellationToken cancellationToken = default)
            => Task.FromResult(Result.CreateSuccess());

        public Task ShutdownAsync(CancellationToken cancellationToken = default)
            => Task.CompletedTask;
    }
}
