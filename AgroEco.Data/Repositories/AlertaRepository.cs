using AgroEco.Core.Alertas;
using AgroEco.Data;
using AgroEco.Data.Repositories;

namespace AgroEco.Data.Repositories;

public class AlertaRepository : RepositoryBase<Alerta, DataContext>
{
    public AlertaRepository(DataContext context) : base(context) { }

    protected override void ApplyChanges(Alerta existing, Alerta next)
    {
        existing.Tipo = next.Tipo;
        existing.Severidad = next.Severidad;
        existing.Titulo = next.Titulo;
        existing.Descripcion = next.Descripcion;
        existing.FincaId = next.FincaId;
        existing.FincaNombre = next.FincaNombre;
        existing.SensorId = next.SensorId;
        existing.SensorNombre = next.SensorNombre;
        existing.SensorTipo = next.SensorTipo;
        existing.ValorActual = next.ValorActual;
        existing.UmbralConfigurado = next.UmbralConfigurado;
        existing.EsActiva = next.EsActiva;
        existing.TareaGenerada = next.TareaGenerada;
        existing.TareaGeneradaId = next.TareaGeneradaId;
        existing.FechaResuelta = next.FechaResuelta;
        existing.FechaUltimaNotificacion = next.FechaUltimaNotificacion;
    }
}