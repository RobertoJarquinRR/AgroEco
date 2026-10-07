using AgroEco.Core.Finanzas;
using AgroEco.Data;
using AgroEco.Data.Repositories;

namespace AgroEco.Data.Repositories;

public class RegistroFinancieroRepository : RepositoryBase<RegistroFinanciero, DataContext>
{
    public RegistroFinancieroRepository(DataContext context) : base(context) { }

    protected override void ApplyChanges(RegistroFinanciero existing, RegistroFinanciero next)
    {
        existing.Tipo = next.Tipo;
        existing.Cultivo = next.Cultivo;
        existing.Categoria = next.Categoria;
        existing.Monto = next.Monto;
        existing.Fecha = next.Fecha;
        existing.Descripcion = next.Descripcion;
        existing.TaskId = next.TaskId;
    }
}