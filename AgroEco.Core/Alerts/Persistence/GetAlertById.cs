using AgroEco.Core.Alerts;
using AgroEco.Core.Interfaces;
using AgroEco.Core;

namespace AgroEco.Core.Alerts.Persistence;

public class GetAlertById
{
    private readonly IAlertRepository _repository;

    public GetAlertById(IAlertRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result<Alert>> HandleAsync(int id, CancellationToken cancellationToken = default)
    {
        var entity = await _repository.GetByIdWithDeliveriesAsync(id, cancellationToken);
        if (entity == null)
        {
            return Result<Alert>.CreateFailure($"Alerta con ID {id} no encontrada");
        }

        var alert = MapToAlert(entity);
        return Result<Alert>.CreateSuccess(alert);
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