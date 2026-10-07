using AgroEco.Core.Interfaces;
using AgroEco.Core.Inventario;
using AgroEco.Core;

namespace AgroEco.Core.Inventario.Persistence;

public class UpdateInsumo
{
    private readonly IRepository<Insumo> _repository;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateInsumo(
        IRepository<Insumo> repository,
        IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> HandleAsync(Insumo insumo, CancellationToken ct = default)
    {
        try
        {
            var existing = await _repository.GetByIdAsync(insumo.Id, ct);
            if (existing == null)
            {
                return Result.CreateFailure($"Insumo con ID {insumo.Id} no encontrado");
            }

            await _repository.UpdateAsync(insumo, ct);
            await _unitOfWork.SaveChangesAsync();

            return Result.CreateSuccess("Insumo actualizado correctamente");
        }
        catch (Exception ex)
        {
            return Result.CreateFailure($"Error actualizando insumo: {ex.Message}");
        }
    }
}
