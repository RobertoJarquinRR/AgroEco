using AgroEco.Core.Interfaces;
using AgroEco.Core.Finanzas;
using AgroEco.Core;

namespace AgroEco.Core.Finanzas.Persistence;

public class DeleteRegistroFinanciero
{
    private readonly IRepository<RegistroFinanciero> _repository;
    private readonly IUnitOfWork _unitOfWork;

    public DeleteRegistroFinanciero(
        IRepository<RegistroFinanciero> repository,
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
                return Result.CreateFailure($"Registro con ID {id} no encontrado");
            }

            await _repository.DeleteAsync(id, ct);
            await _unitOfWork.SaveChangesAsync();

            return Result.CreateSuccess("Registro eliminado correctamente");
        }
        catch (Exception ex)
        {
            return Result.CreateFailure($"Error eliminando registro: {ex.Message}");
        }
    }
}
