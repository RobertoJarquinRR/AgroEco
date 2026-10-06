using AgroEco.Core.Jobs.Actions.Configuration;
using AgroEco.Core.Jobs.Actions.Implementations;
using Microsoft.Extensions.DependencyInjection;

namespace AgroEco.Core.Jobs.Actions.Creators;

public sealed class ExecuteTaskActionCreator : IActionCreator
{
    private readonly IServiceScopeFactory _scopeFactory;

    public ExecuteTaskActionCreator(IServiceScopeFactory scopeFactory)
    {
        _scopeFactory = scopeFactory;
    }

    public ActionDescriptor Descriptor { get; } =
        new("executeTask", "Ejecutar tarea (descuenta inventario y registra gasto)",
        [
            new("insumoId", "ID del insumo a descontar", "number"),
            new("cantidadDescontar", "Cantidad a descontar", "number"),
            new("costoUnitario", "Costo unitario del insumo", "number"),
            new("descripcion", "Descripción del gasto", "text"),
            new("cultivo", "Cultivo asociado (opcional)", "text"),
            new("categoriaInsumo", "Categoría del insumo (opcional)", "text")
        ]);

    public Result<Action> Create(string name, ActionConfiguration configuration)
    {
        if (configuration is not ExecuteTaskActionConfiguration execConfig)
        {
            return Result<Action>.CreateFailure("Invalid configuration for ExecuteTaskAction.");
        }

        var action = new ExecuteTaskAction(name.Trim(), _scopeFactory)
        {
            Config = execConfig
        };

        return Result<Action>.CreateSuccess(action);
    }
}