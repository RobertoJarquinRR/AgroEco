using AgroEco.Core.Alerts;
using AgroEco.Core.Interfaces;
using AgroEco.Core;

namespace AgroEco.Core.Alerts.Persistence;

public class UpdateAlert
{
    private readonly IRepository<AlertEntity> _repository;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateAlert(
        IRepository<AlertEntity> repository,
        IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<Alert>> HandleAsync(
        Alert alert,
        CancellationToken cancellationToken = default)
    {
        if (alert.Id <= 0)
        {
            return Result<Alert>.CreateFailure("ID de alerta inválido");
        }

        if (string.IsNullOrWhiteSpace(alert.Title))
        {
            return Result<Alert>.CreateFailure("El título de la alerta es obligatorio");
        }

        var entity = new AlertEntity
        {
            Id = alert.Id,
            Title = alert.Title.Trim(),
            Message = alert.Message.Trim(),
            Level = (int)alert.Level,
            Status = (int)alert.Status,
            ConfigurationJson = System.Text.Json.JsonSerializer.Serialize(alert.Configuration),
            CreatedAt = alert.CreatedAt,
            DeliveredAt = alert.DeliveredAt,
            MetadataJson = System.Text.Json.JsonSerializer.Serialize(alert.Metadata),
            Deliveries = alert.Deliveries
        };

        await _repository.UpdateAsync(entity, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<Alert>.CreateSuccess(alert, "Alerta actualizada correctamente");
    }
}