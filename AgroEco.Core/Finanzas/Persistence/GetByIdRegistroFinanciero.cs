using AgroEco.Core.Interfaces;
using AgroEco.Core.Finanzas;
using AgroEco.Core;

namespace AgroEco.Core.Finanzas.Persistence;

public class GetByIdRegistroFinanciero
{
    private readonly IRepository<RegistroFinanciero> _repository;

    public GetByIdRegistroFinanciero(IRepository<RegistroFinanciero> repository)
    {
        _repository = repository;
    }

    public async Task<Result<RegistroFinanciero>> HandleAsync(int id, CancellationToken ct = default)
    {
        try
        {
            var registro = await _repository.GetByIdAsync(id, ct);
            if (registro == null)
            {
                return Result<RegistroFinanciero>.CreateFailure($"Registro con ID {id} no encontrado");
            }
            return Result<RegistroFinanciero>.CreateSuccess(registro);
        }
        catch (Exception ex)
        {
            return Result<RegistroFinanciero>.CreateFailure($"Error obteniendo registro: {ex.Message}");
        }
    }
}
