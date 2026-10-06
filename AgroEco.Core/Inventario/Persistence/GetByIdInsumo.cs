using AgroEco.Core.Interfaces;
using AgroEco.Core.Inventario;
using AgroEco.Core;

namespace AgroEco.Core.Inventario.Persistence;

public class GetByIdInsumo
{
    private readonly IRepository<Insumo> _repository;

    public GetByIdInsumo(IRepository<Insumo> repository)
    {
        _repository = repository;
    }

    public async Task<Result<Insumo>> HandleAsync(int id, CancellationToken ct = default)
    {
        try
        {
            var insumo = await _repository.GetByIdAsync(id, ct);
            if (insumo == null)
            {
                return Result<Insumo>.CreateFailure($"Insumo con ID {id} no encontrado");
            }
            return Result<Insumo>.CreateSuccess(insumo);
        }
        catch (Exception ex)
        {
            return Result<Insumo>.CreateFailure($"Error obteniendo insumo: {ex.Message}");
        }
    }
}
