using AgroEco.Core.Interfaces;
using AgroEco.Core.Finanzas;
using AgroEco.Core;

namespace AgroEco.Core.Finanzas.Persistence;

public class CreateRegistroFinanciero
{
    private readonly IRepository<RegistroFinanciero> _repository;
    private readonly IUnitOfWork _unitOfWork;

    public CreateRegistroFinanciero(
        IRepository<RegistroFinanciero> repository,
        IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<RegistroFinanciero>> HandleAsync(
        string tipo,
        string? cultivo,
        string categoria,
        decimal monto,
        DateOnly fecha,
        string descripcion,
        int? taskId = null)
    {
        if (string.IsNullOrWhiteSpace(tipo) || (tipo != "ingreso" && tipo != "costo"))
        {
            return Result<RegistroFinanciero>.CreateFailure("Tipo debe ser 'ingreso' o 'costo'");
        }

        if (monto <= 0)
        {
            return Result<RegistroFinanciero>.CreateFailure("El monto debe ser mayor a cero");
        }

        var registro = new RegistroFinanciero
        {
            Tipo = tipo,
            Cultivo = cultivo?.Trim(),
            Categoria = categoria?.Trim() ?? "",
            Monto = monto,
            Fecha = fecha,
            Descripcion = descripcion?.Trim() ?? "",
            TaskId = taskId,
            FechaCreacion = DateOnly.FromDateTime(DateTime.Today)
        };

        await _repository.AddAsync(registro);
        await _unitOfWork.SaveChangesAsync();

        return Result<RegistroFinanciero>.CreateSuccess(registro, "Registro financiero creado correctamente");
    }
}
