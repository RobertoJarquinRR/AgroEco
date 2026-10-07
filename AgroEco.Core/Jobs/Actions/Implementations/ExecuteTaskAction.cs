using AgroEco.Core.Inventario;
using AgroEco.Core.Finanzas;
using AgroEco.Core.Interfaces;
using AgroEco.Core.Jobs.Actions.Configuration;
using AgroEco.Core;
using Microsoft.Extensions.DependencyInjection;

namespace AgroEco.Core.Jobs.Actions.Implementations;

public class ExecuteTaskAction : Action
{
    private IServiceScopeFactory _scopeFactory = null!;
    private ExecuteTaskActionConfiguration _config = null!;

    public ExecuteTaskAction(string name, IServiceScopeFactory scopeFactory) : base(name)
    {
        _scopeFactory = scopeFactory;
    }

    private ExecuteTaskAction() : base(string.Empty)
    {
    }

    public ExecuteTaskActionConfiguration Config
    {
        get => _config;
        init => _config = value ?? throw new ArgumentNullException(nameof(value));
    }

    internal override void AttachServices(IServiceProvider services)
    {
        _scopeFactory = services.GetRequiredService<IServiceScopeFactory>();
    }

    public override async Task<Result> Execute()
    {
        using var scope = _scopeFactory.CreateScope();
        
        var insumoRepo = scope.ServiceProvider.GetRequiredService<IRepository<Insumo>>();
        var registroRepo = scope.ServiceProvider.GetRequiredService<IRepository<RegistroFinanciero>>();
        var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

        // Obtener el insumo
        var insumo = await insumoRepo.GetByIdAsync(_config.InsumoId);
        if (insumo == null)
        {
            return Result.CreateFailure($"Insumo con ID {_config.InsumoId} no encontrado");
        }

        // Validar stock suficiente
        if (insumo.Cantidad < _config.CantidadDescontar)
        {
            return Result.CreateFailure(
                $"Stock insuficiente para '{insumo.Nombre}'. Disponible: {insumo.Cantidad} {insumo.Unidad}, Requerido: {_config.CantidadDescontar} {insumo.Unidad}");
        }

        // Descontar del inventario
        insumo.Cantidad -= _config.CantidadDescontar;
        insumo.FechaActualizacion = DateOnly.FromDateTime(DateTime.Today);
        await insumoRepo.UpdateAsync(insumo);

        // Crear registro financiero (costo)
        var montoTotal = _config.CostoUnitario * _config.CantidadDescontar;
        var descripcion = string.IsNullOrWhiteSpace(_config.Descripcion)
            ? $"Gasto por tarea: {Name} - {insumo.Nombre}"
            : _config.Descripcion;

        var registro = new RegistroFinanciero
        {
            Tipo = "costo",
            Cultivo = string.IsNullOrWhiteSpace(_config.Cultivo) ? insumo.Cultivo : _config.Cultivo,
            Categoria = _config.CategoriaInsumo ?? insumo.Categoria,
            Monto = montoTotal,
            Fecha = DateOnly.FromDateTime(DateTime.Today),
            Descripcion = descripcion,
            TaskId = null, // Se podría pasar el JobId si está disponible
            FechaCreacion = DateOnly.FromDateTime(DateTime.Today)
        };

        await registroRepo.AddAsync(registro);
        await unitOfWork.SaveChangesAsync();

        return Result.CreateSuccess(
            $"Tarea ejecutada: Descontado {_config.CantidadDescontar} {insumo.Unidad} de {insumo.Nombre}. " +
            $"Registrado gasto de {montoTotal:C}");
    }
}
