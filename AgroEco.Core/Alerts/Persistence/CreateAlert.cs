using AgroEco.Core.Alerts;
using AgroEco.Core.Interfaces;
using AgroEco.Core;

namespace AgroEco.Core.Alerts.Persistence;

public class CreateAlert
{
    private readonly IRepository<AlertEntity> _repository;
    private readonly IUnitOfWork _unitOfWork;

    public CreateAlert(
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
        if (string.IsNullOrWhiteSpace(alert.Title))
        {
            return Result<Alert>.CreateFailure("El título de la alerta es obligatorio");
        }

        if (string.IsNullOrWhiteSpace(alert.Message))
        {
            return Result<Alert>.CreateFailure("El mensaje de la alerta es obligatorio");
        }

        var entity = new AlertEntity
        {
            Title = alert.Title.Trim(),
            Message = alert.Message.Trim(),
            Level = (int)alert.Level,
            Status = (int)alert.Status,
            ConfigurationJson = System.Text.Json.JsonSerializer.Serialize(alert.Configuration),
            CreatedAt = alert.CreatedAt,
            DeliveredAt = alert.DeliveredAt,
            MetadataJson = System.Text.Json.JsonSerializer.Serialize(alert.Metadata)
        };

        await _repository.AddAsync(entity, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        alert.Id = entity.Id;

        return Result<Alert>.CreateSuccess(alert, "Alerta creada correctamente");
    }
}