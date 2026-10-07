using AgroEco.Core.Alertas;
using AgroEco.Data;
using AgroEco.Data.Repositories;

namespace AgroEco.Data.Repositories;

public class UmbralSensorRepository : RepositoryBase<UmbralSensor, DataContext>
{
    public UmbralSensorRepository(DataContext context) : base(context) { }

    protected override void ApplyChanges(UmbralSensor existing, UmbralSensor next)
    {
        existing.SensorTipo = next.SensorTipo;
        existing.FincaId = next.FincaId;
        existing.FincaNombre = next.FincaNombre;
        existing.Minimo = next.Minimo;
        existing.Maximo = next.Maximo;
        existing.SeveridadMinima = next.SeveridadMinima;
        existing.SeveridadMaxima = next.SeveridadMaxima;
        existing.Activo = next.Activo;
        existing.GenerarTareaAuto = next.GenerarTareaAuto;
        existing.AccionSugerida = next.AccionSugerida;
        existing.InsumoSugeridoId = next.InsumoSugeridoId;
        existing.CantidadInsumoSugerida = next.CantidadInsumoSugerida;
        existing.CostoUnitarioSugerido = next.CostoUnitarioSugerido;
        existing.FechaActualizacion = DateTime.UtcNow;
    }
}