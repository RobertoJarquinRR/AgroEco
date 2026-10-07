using AgroEco.Core.Interfaces;
using AgroEco.Core.Inventario;
using AgroEco.Core;

namespace AgroEco.Core.Inventario.Persistence;

public class DeleteInsumo
{
    private readonly IRepository<Insumo> _repository;
    private readonly IUnitOfWork _unitOfWork;

    public DeleteInsumo(
        IRepository<Insumo> repository,
        IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> HandleAsync(int id, CancellationToken ct = default)
    {
        try
        {
            var existing = await _repository.GetByIdAsync(id, ct);
            if (existing == null)
            {
                return Result.CreateFailure($"Insumo con ID {id} no encontrado");
            }

            await _repository.DeleteAsync(id, ct);
            await _unitOfWork.SaveChangesAsync();

            return Result.CreateSuccess("Insumo eliminado correctamente");
        }
        catch (Exception ex)
        {
            return Result.CreateFailure($"Error eliminando insumo: {ex.Message}");
        }
    }
}
