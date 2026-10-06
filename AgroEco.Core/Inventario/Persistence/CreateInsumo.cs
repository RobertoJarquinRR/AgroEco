using AgroEco.Core.Interfaces;
using AgroEco.Core.Inventario;
using AgroEco.Core;

namespace AgroEco.Core.Inventario.Persistence;

public class CreateInsumo
{
    private readonly IRepository<Insumo> _repository;
    private readonly IUnitOfWork _unitOfWork;

    public CreateInsumo(
        IRepository<Insumo> repository,
        IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<Insumo>> HandleAsync(
        string nombre,
        string categoria,
        string cultivo,
        decimal cantidad,
        string unidad,
        decimal stockMin,
        DateOnly? caducidad,
        string finca,
        string descripcion)
    {
        if (string.IsNullOrWhiteSpace(nombre))
        {
            return Result<Insumo>.CreateFailure("El nombre del insumo es obligatorio");
        }

        var insumo = new Insumo
        {
            Nombre = nombre.Trim(),
            Categoria = categoria?.Trim() ?? "",
            Cultivo = cultivo?.Trim() ?? "",
            Cantidad = cantidad,
            Unidad = unidad?.Trim() ?? "",
            StockMin = stockMin,
            Caducidad = caducidad,
            Finca = finca?.Trim() ?? "",
            Descripcion = descripcion?.Trim() ?? "",
            FechaCreacion = DateOnly.FromDateTime(DateTime.Today)
        };

        await _repository.AddAsync(insumo);
        await _unitOfWork.SaveChangesAsync();

        return Result<Insumo>.CreateSuccess(insumo, "Insumo creado correctamente");
    }
}
