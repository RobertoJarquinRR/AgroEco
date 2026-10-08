using AgroEco.Core.Interfaces;
using AgroEco.Core.Finanzas;
using AgroEco.Core;

namespace AgroEco.Core.Finanzas.Persistence;

public class GetAllRegistroFinanciero
{
    private readonly IRepository<RegistroFinanciero> _repository;

    public GetAllRegistroFinanciero(IRepository<RegistroFinanciero> repository)
    {
        _repository = repository;
    }

    public async Task<Result<List<RegistroFinanciero>>> HandleAsync(CancellationToken ct = default)
    {
        try
        {
            var registros = await _repository.GetAllAsync(ct);
            return Result<List<RegistroFinanciero>>.CreateSuccess(registros);
        }
        catch (Exception ex)
        {
            return Result<List<RegistroFinanciero>>.CreateFailure($"Error obteniendo registros: {ex.Message}");
        }
    }
}
