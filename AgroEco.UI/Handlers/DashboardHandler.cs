using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using AgroEco.Core.Alerts;
using AgroEco.Core.Alerts.Persistence;
using AgroEco.Core.Jobs;
using AgroEco.Core.Jobs.Persistence;
using AgroEco.Core.Inventario.Persistence;
using AgroEco.Core.Finanzas.Persistence;
using AgroEco.UI.Mensajes;
using AgroEco.UI.Mensajeros;

namespace AgroEco.UI.Handlers
{
    public class DashboardHandler
    {
        private readonly Action<string, object> _enviar;
        private readonly ILogger<DashboardHandler> _logger;
        private readonly GetAllAlerts _getAllAlerts;
        private readonly GetAllJob _getAllJobs;
        private readonly GetAllInsumo _getAllInsumos;
        private readonly GetAllRegistroFinanciero _getAllRegistros;

        public DashboardHandler(
            Action<string, object> enviar,
            GetAllAlerts getAllAlerts,
            GetAllJob getAllJobs,
            GetAllInsumo getAllInsumos,
            GetAllRegistroFinanciero getAllRegistros,
            ILogger<DashboardHandler> logger)
        {
            _enviar = enviar;
            _getAllAlerts = getAllAlerts;
            _getAllJobs = getAllJobs;
            _getAllInsumos = getAllInsumos;
            _getAllRegistros = getAllRegistros;
            _logger = logger;
        }

        public async void ManejarMensaje(Mensaje msg)
        {
            switch (msg.Type)
            {
                case TiposMensaje.ReadyDashboard:
                case TiposMensaje.Ready:
                    await EnviarDashboardCompletoAsync();
                    break;
            }
        }

        private async Task EnviarDashboardCompletoAsync()
        {
            try
            {
                _enviar("finca", "Finca Principal");

                var alertasTask = _getAllAlerts.HandleAsync(limit: 20, status: AlertStatus.Pending);
                var jobsTask = _getAllJobs.HandleAsync();
                var insumosTask = _getAllInsumos.HandleAsync();
                var registrosTask = _getAllRegistros.HandleAsync();

                await Task.WhenAll(alertasTask, jobsTask, insumosTask, registrosTask);

                var alertas = alertasTask.Result.Success ? alertasTask.Result.Value : new List<Alert>();
                var alertasPendientes = alertas.Where(a => a.Status == AlertStatus.Pending).ToList();
                var alertasParaUI = alertasPendientes.Take(10).Select(a => new
                {
                    titulo = a.Title,
                    tiempo = FormatearTiempo(DateTime.UtcNow - a.CreatedAt),
                    nivel = a.Level.ToString().ToLower()
                }).ToList();

                var jobs = jobsTask.Result.Success ? jobsTask.Result.Value : new List<Job>();
                var tareasPendientes = jobs.Where(j => j.Status == Status.Created || j.Status == Status.Enqueued || j.Status == Status.Running).ToList();
                var tareasCompletadas = jobs.Where(j => j.Status == Status.Succeeded).ToList();
                var tareasParaUI = tareasPendientes.Take(10).Select(j => new
                {
                    titulo = j.Name,
                    cultivo = j.Trigger != null ? "Sensor" : "Manual",
                    lote = "Lote General",
                    estado = j.Status.ToString()
                }).ToList();

                var insumos = insumosTask.Result.Success ? insumosTask.Result.Value : new List<AgroEco.Core.Inventario.Insumo>();
                var itemsBajoMinimo = insumos.Where(i => i.Cantidad < i.StockMin).Count();

                var registros = registrosTask.Result.Success ? registrosTask.Result.Value : new List<AgroEco.Core.Finanzas.RegistroFinanciero>();
                var ingresos = registros.Where(r => r.Tipo == "ingreso").Sum(r => r.Monto);
                var costos = registros.Where(r => r.Tipo == "costo").Sum(r => r.Monto);
                var balance = ingresos - costos;

                _enviar("ambiente", new
                {
                    temperatura = 24.5,
                    notaTemperatura = "Óptimo",
                    humedad = 68,
                    notaHumedad = "Óptimo"
                });

                _enviar("stats", new
                {
                    sensoresActivos = 0,
                    alertas = alertasPendientes.Count,
                    tareasActivas = tareasPendientes.Count,
                    tareasCompletadas = tareasCompletadas.Count,
                    plagas = 0
                });

                _enviar("umbrales", new { umbrales = new List<object>() });

                _enviar("plagas", new
                {
                    hayPlagas = false,
                    mensaje = "Sin plagas reportadas · condiciones óptimas",
                    salud = "saludable"
                });

                _enviar("alertas", alertasParaUI);

                _enviar("tareas", tareasParaUI);

                _logger.LogInformation("Dashboard completo enviado: {Alertas} alertas, {Tareas} tareas",
                    alertasPendientes.Count, tareasPendientes.Count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error enviando dashboard completo");
                EnviarFallback();
            }
        }

        private void EnviarFallback()
        {
            _enviar("ambiente", new { temperatura = 24.5, notaTemperatura = "Óptimo", humedad = 68, notaHumedad = "Óptimo" });
            _enviar("stats", new { sensoresActivos = 0, alertas = 0, tareasActivas = 0, tareasCompletadas = 0, plagas = 0 });
            _enviar("umbrales", new { umbrales = new List<object>() });
            _enviar("plagas", new { hayPlagas = false, mensaje = "Sin plagas reportadas", salud = "saludable" });
            _enviar("alertas", new List<object>());
            _enviar("tareas", new List<object>());
            _enviar("finca", "Finca Principal");
        }

        private static string FormatearTiempo(TimeSpan diff)
        {
            if (diff.TotalMinutes < 1) return "ahora mismo";
            if (diff.TotalMinutes < 60) return $"hace {(int)diff.TotalMinutes} min";
            if (diff.TotalHours < 24) return $"hace {(int)diff.TotalHours} h";
            if (diff.TotalDays < 30) return $"hace {(int)diff.TotalDays} d";
            return "hace 1+ mes";
        }
    }
}