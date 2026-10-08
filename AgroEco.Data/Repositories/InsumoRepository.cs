using AgroEco.Core.Inventario;
using AgroEco.Data;
using AgroEco.Data.Repositories;

namespace AgroEco.Data.Repositories;

public class InsumoRepository : RepositoryBase<Insumo, DataContext>
{
    public InsumoRepository(DataContext context) : base(context) { }

    protected override void ApplyChanges(Insumo existing, Insumo next)
    {
        existing.Nombre = next.Nombre;
        existing.Categoria = next.Categoria;
        existing.Cultivo = next.Cultivo;
        existing.Cantidad = next.Cantidad;
        existing.Unidad = next.Unidad;
        existing.StockMin = next.StockMin;
        existing.Caducidad = next.Caducidad;
        existing.Finca = next.Finca;
        existing.Descripcion = next.Descripcion;
        existing.FechaActualizacion = DateOnly.FromDateTime(DateTime.Today);
    }
}