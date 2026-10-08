using AgroEco.Core.Jobs;
using AgroEco.Core.Jobs.Actions;
using AgroEco.Core.Inventario;
using AgroEco.Core.Finanzas;
using AgroEco.Core.Interfaces;
using AgroEco.Core.Triggers;
using AgroEco.Core.Triggers.Implementations;
using System.Text;

namespace AgroEco.Core.Reportes;

public interface IExportService
{
    Task<string> ExportJobsToCsvAsync(IRepository<Job> jobRepository, CancellationToken ct = default);
    Task<string> ExportInsumosToCsvAsync(IRepository<Insumo> insumoRepository, CancellationToken ct = default);
    Task<string> ExportRegistrosFinancierosToCsvAsync(IRepository<RegistroFinanciero> registroRepository, CancellationToken ct = default);
    Task<string> ExportTareasConInsumosToCsvAsync(IRepository<Job> jobRepository, IRepository<Insumo> insumoRepository, CancellationToken ct = default);
}

public class ExportService : IExportService
{
    public async Task<string> ExportJobsToCsvAsync(IRepository<Job> jobRepository, CancellationToken ct = default)
    {
        var jobs = await jobRepository.GetAllAsync(ct);
        var sb = new StringBuilder();
        
        sb.AppendLine("Id,Nombre,Descripcion,Prioridad,Estado,FechaCreacion,TriggerTipo,TriggerConfig");
        
        foreach (var job in jobs)
        {
            var triggerInfo = job.Trigger switch
            {
                DateTimeTrigger dt => $"datetime|{dt.TargetTime:o}",
                CronTrigger ctr => $"cron|{ctr.Config.CronExpression}|{ctr.Config.TimeZone}|{ctr.Config.StartDate}|{ctr.Config.EndDate}",
                _ => "unknown"
            };
            
            sb.AppendLine($"{job.Id},\"{EscapeCsv(job.Name)}\",\"{EscapeCsv(job.Description ?? "")}\",{job.Priority},{job.Status},{job.Date:o},{triggerInfo}");
        }
        
        return sb.ToString();
    }

    public async Task<string> ExportInsumosToCsvAsync(IRepository<Insumo> insumoRepository, CancellationToken ct = default)
    {
        var insumos = await insumoRepository.GetAllAsync(ct);
        var sb = new StringBuilder();
        
        sb.AppendLine("Id,Nombre,Categoria,Cultivo,Cantidad,Unidad,StockMinimo,Caducidad,Finca,Descripcion,FechaCreacion,FechaActualizacion");
        
        foreach (var i in insumos)
        {
            sb.AppendLine($"{i.Id},\"{EscapeCsv(i.Nombre)}\",\"{EscapeCsv(i.Categoria)}\",\"{EscapeCsv(i.Cultivo)}\",{i.Cantidad},\"{EscapeCsv(i.Unidad)}\",{i.StockMin},{i.Caducidad:o},\"{EscapeCsv(i.Finca)}\",\"{EscapeCsv(i.Descripcion)}\",{i.FechaCreacion:o},{i.FechaActualizacion:o}");
        }
        
        return sb.ToString();
    }

    public async Task<string> ExportRegistrosFinancierosToCsvAsync(IRepository<RegistroFinanciero> registroRepository, CancellationToken ct = default)
    {
        var registros = await registroRepository.GetAllAsync(ct);
        var sb = new StringBuilder();
        
        sb.AppendLine("Id,Tipo,Cultivo,Categoria,Monto,Fecha,Descripcion,TaskId,FechaCreacion");
        
        foreach (var r in registros)
        {
            sb.AppendLine($"{r.Id},\"{EscapeCsv(r.Tipo)}\",\"{EscapeCsv(r.Cultivo ?? "")}\",\"{EscapeCsv(r.Categoria)}\",{r.Monto},{r.Fecha:o},\"{EscapeCsv(r.Descripcion)}\",{r.TaskId},{r.FechaCreacion:o}");
        }
        
        return sb.ToString();
    }

    public async Task<string> ExportTareasConInsumosToCsvAsync(IRepository<Job> jobRepository, IRepository<Insumo> insumoRepository, CancellationToken ct = default)
    {
        var jobs = await jobRepository.GetAllAsync(ct);
        var insumos = await insumoRepository.GetAllAsync(ct);
        var insumoDict = insumos.ToDictionary(i => i.Id);
        
        var sb = new StringBuilder();
        sb.AppendLine("TareaId,TareaNombre,TareaDescripcion,TareaPrioridad,TareaEstado,FechaCreacion,TriggerTipo,TriggerConfig,AccionTipo,InsumoId,InsumoNombre,InsumoCategoria,CantidadDescontar,CostoUnitario,DescripcionGasto,Cultivo");
        
        foreach (var job in jobs)
        {
            foreach (var action in job.Actions)
            {
                var config = action.Configuration as dynamic;
                int insumoId = config?.InsumoId ?? 0;
                insumoDict.TryGetValue(insumoId, out var insumo);
                
var triggerInfo = job.Trigger switch
            {
                DateTimeTrigger dt => $"datetime|{dt.TargetTime:o}",
                CronTrigger ctr => $"cron|{ctr.Config.CronExpression}|{ctr.Config.TimeZone}",
                _ => "unknown"
            };
                
                sb.AppendLine($"{job.Id},\"{EscapeCsv(job.Name)}\",\"{EscapeCsv(job.Description ?? "")}\",{job.Priority},{job.Status},{job.Date:o},{triggerInfo},{action.GetType().Name},{insumoId},\"{EscapeCsv(insumo?.Nombre ?? "")}\",\"{EscapeCsv(insumo?.Categoria ?? "")}\",{config?.CantidadDescontar ?? 0},{config?.CostoUnitario ?? 0},\"{EscapeCsv(config?.Descripcion ?? "")}\",\"{EscapeCsv(config?.Cultivo ?? "")}\"");
            }
        }
        
        return sb.ToString();
    }

    private static string EscapeCsv(string value)
    {
        if (string.IsNullOrEmpty(value)) return "";
        return value.Replace("\"", "\"\"");
    }
}