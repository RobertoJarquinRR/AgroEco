namespace AgroEco.Core.Jobs.Actions.Configuration;

public sealed record ExecuteTaskActionConfiguration : ActionConfiguration
{
    public int InsumoId { get; init; }
    public decimal CantidadDescontar { get; init; }
    public decimal CostoUnitario { get; init; }
    public string? Descripcion { get; init; }
    public string? Cultivo { get; init; }
    public string? CategoriaInsumo { get; init; }
}