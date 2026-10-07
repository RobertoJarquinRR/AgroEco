using AgroEco.Core.Alerts;
using AgroEco.Core.Interfaces;
using AgroEco.Core;

namespace AgroEco.Core.Alerts.Persistence;

public class GetAllAlerts
{
    private readonly IAlertRepository _repository;

    public GetAllAlerts(IAlertRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result<List<Alert>>> HandleAsync(
        int? limit = null,
        int? offset = null,
        AlertLevel? level = null,
        AlertStatus? status = null,
        DateTime? fromDate = null,
        DateTime? toDate = null,
        CancellationToken cancellationToken = default)
    {
        var entities = await _repository.GetAllWithDeliveriesAsync(
            limit, offset, level, status, fromDate, toDate, cancellationToken);

        var alerts = entities.Select(MapToAlert).ToList();
        return Result<List<Alert>>.CreateSuccess(alerts);
    }

    private static Alert MapToAlert(AlertEntity entity)
    {
        var alert = new Alert
        {
            Id = entity.Id,
            Title = entity.Title,
            Message = entity.Message,
            Level = (AlertLevel)entity.Level,
            Status = (AlertStatus)entity.Status,
            CreatedAt = entity.CreatedAt,
            DeliveredAt = entity.DeliveredAt,
            Deliveries = entity.Deliveries
        };

        try
        {
            alert.Configuration = System.Text.Json.JsonSerializer.Deserialize<AlertConfiguration>(entity.ConfigurationJson)
                ?? AlertConfiguration.Default;
        }
        catch
        {
            alert.Configuration = AlertConfiguration.Default;
        }

        try
        {
            alert.Metadata = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, object>>(entity.MetadataJson)
                ?? new();
        }
        catch
        {
            alert.Metadata = new();
        }

        return alert;
    }
}