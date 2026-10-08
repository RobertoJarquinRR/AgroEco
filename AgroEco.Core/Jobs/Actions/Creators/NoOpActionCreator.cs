using AgroEco.Core.Jobs.Actions.Configuration;
using AgroEco.Core.Jobs.Actions.Implementations;

namespace AgroEco.Core.Jobs.Actions.Creators;

public sealed class NoOpActionCreator : IActionCreator
{
    public ActionDescriptor Descriptor { get; } =
        new("noop", "Acción de demostración (sin operación)",
        [
            new("insumoId", "ID del insumo a descontar", "number"),
            new("cantidadDescontar", "Cantidad a descontar", "number"),
            new("costoUnitario", "Costo unitario del insumo", "number"),
            new("descripcion", "Descripción del gasto", "text")
        ]);

    public Result<Action> Create(string name, ActionConfiguration configuration)
    {
        if (configuration is not NoOpActionConfiguration noOpConfig)
        {
            return Result<Action>.CreateFailure("Invalid configuration for NoOpAction.");
        }

        return Result<Action>.CreateSuccess(
            new NoOpAction(name.Trim())
            {
                Configuration = noOpConfig
            });
    }
}
