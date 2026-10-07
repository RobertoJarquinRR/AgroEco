using System;
using AgroEco.UI.Handlers;
using Microsoft.Extensions.Logging;

namespace AgroEco.UI.Mensajeros
{
    public class MsgRouter
    {
        private readonly TareasHandler _tareas;
        private readonly PlagasHandler _plagas;
        private readonly FinanzasHandler _finanzas;
        private readonly DashboardHandler _dashboard;
        private readonly SensoresHandler _sensores;
        private readonly SensorReadingHandler _sensorReading;
        private readonly InventarioHandler _inventario;
        private readonly EducacionHandler _educacion;
        private readonly ILogger<MsgRouter> _logger;

        public MsgRouter(
            TareasHandler tareas,
            PlagasHandler plagas,
            FinanzasHandler finanzas,
            DashboardHandler dashboard,
            SensoresHandler sensores,
            SensorReadingHandler sensorReading,
            InventarioHandler inventario,
            EducacionHandler educacion,
            ILogger<MsgRouter> logger)
        {
            _tareas = tareas;
            _plagas = plagas;
            _finanzas = finanzas;
            _dashboard = dashboard;
            _sensores = sensores;
            _sensorReading = sensorReading;
            _inventario = inventario;
            _educacion = educacion;
            _logger = logger;
        }

        public bool Enrutar(Mensaje mensaje)
        {
            try
            {
                return mensaje.Screen switch
                {
                    "tareas" => HandleScreen(_tareas.ManejarMensaje, mensaje),
                    "plagas" => HandleScreen(_plagas.ManejarMensaje, mensaje),
                    "finanzas" => HandleScreen(_finanzas.ManejarMensaje, mensaje),
                    "dashboard" => HandleScreen(_dashboard.ManejarMensaje, mensaje),
                    "sensores" => HandleScreen(_sensores.ManejarMensaje, mensaje),
                    "sensorReading" => HandleScreen(_sensorReading.ManejarMensaje, mensaje),
                    "inventario" => HandleScreen(_inventario.ManejarMensaje, mensaje),
                    "educacion" => HandleScreen(_educacion.ManejarMensaje, mensaje),
                    _ => false
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error en {Screen}/{Type}", mensaje.Screen, mensaje.Type);
                return true;
            }
        }

        private bool HandleScreen(Action<Mensaje> handler, Mensaje mensaje)
        {
            handler(mensaje);
            return true;
        }
    }
}