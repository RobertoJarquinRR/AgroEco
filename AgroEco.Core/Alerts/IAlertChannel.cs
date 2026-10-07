using System.Threading;

namespace AgroEco.Core.Alerts;

public interface IAlertChannel
{
    string ChannelType { get; }
    bool IsEnabled { get; set; }
    int Priority { get; }

    Task<AlertDeliveryResult> SendAsync(Alert alert, CancellationToken cancellationToken = default);
    Task<Result> InitializeAsync(CancellationToken cancellationToken = default);
    Task ShutdownAsync(CancellationToken cancellationToken = default);
}