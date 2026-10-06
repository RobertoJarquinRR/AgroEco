using AgroEco.Core.Interfaces;
using AgroEco.Core.Inventario;
using AgroEco.Core;

namespace AgroEco.Core.Inventario.Persistence;

public class GetAllInsumo
{
    private readonly IRepository<Insumo> _repository;

    public GetAllInsumo(IRepository<Insumo> repository)
    {
        _repository = repository;
    }

    public async Task<Result<List<Insumo>>> HandleAsync(CancellationToken ct = default)
    {
        try
        {
            var insumos = await _repository.GetAllAsync(ct);
            return Result<List<Insumo>>.CreateSuccess(insumos);
        }
        catch (Exception ex)
        {
            return Result<List<Insumo>>.CreateFailure($"Error obteniendo insumos: {ex.Message}");
        }
    }
}
