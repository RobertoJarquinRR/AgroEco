namespace AgroEco.Core.Jobs.Actions.Configuration;

public sealed record NoOpActionConfiguration : ActionConfiguration
{
    public long InsumoId { get; init; }
    public decimal CantidadDescontar { get; init; }
    public decimal CostoUnitario { get; init; }
    public string? Descripcion { get; init; }
}
