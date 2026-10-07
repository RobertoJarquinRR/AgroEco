namespace AgroEco.UI.Events;

public sealed record TaskCompletedEvent(
    int TaskId,
    string TaskName,
    long InsumoId,
    decimal CantidadDescontar,
    decimal CostoUnitario,
    string Descripcion);