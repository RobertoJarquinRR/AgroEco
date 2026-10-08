using AgroEco.Core.Interfaces;
using AgroEco.Core.Finanzas;
using AgroEco.Core;

namespace AgroEco.Core.Finanzas.Persistence;

public class UpdateRegistroFinanciero
{
    private readonly IRepository<RegistroFinanciero> _repository;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateRegistroFinanciero(
        IRepository<RegistroFinanciero> repository,
        IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> HandleAsync(RegistroFinanciero registro, CancellationToken ct = default)
    {
        try
        {
            var existing = await _repository.GetByIdAsync(registro.Id, ct);
            if (existing == null)
            {
                return Result.CreateFailure($"Registro con ID {registro.Id} no encontrado");
            }

            await _repository.UpdateAsync(registro, ct);
            await _unitOfWork.SaveChangesAsync();

            return Result.CreateSuccess("Registro actualizado correctamente");
        }
        catch (Exception ex)
        {
            return Result.CreateFailure($"Error actualizando registro: {ex.Message}");
        }
    }
}
