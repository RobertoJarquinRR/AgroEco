using AgroEco.Core.Alerts;
using AgroEco.Core.Interfaces;
using AgroEco.Core;

namespace AgroEco.Core.Alerts.Persistence;

public class DeleteAlert
{
    private readonly IRepository<AlertEntity> _repository;
    private readonly IUnitOfWork _unitOfWork;

    public DeleteAlert(
        IRepository<AlertEntity> repository,
        IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> HandleAsync(int id, CancellationToken cancellationToken = default)
    {
        if (id <= 0)
        {
            return Result.CreateFailure("ID de alerta inválido");
        }

        var existing = await _repository.GetByIdAsync(id, cancellationToken);
        if (existing == null)
        {
            return Result.CreateFailure($"Alerta con ID {id} no encontrada");
        }

        await _repository.DeleteAsync(id, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.CreateSuccess("Alerta eliminada correctamente");
    }
}