using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Text.Json;
using System.Threading.Tasks;
using AgroEco.Core.Interfaces;
using AgroEco.Core.Inventario.Persistence;
using AgroEco.Core.Jobs.Actions;
using AgroEco.Core.Jobs.Actions.Configuration;
using AgroEco.UI.Mensajes;
using AgroEco.UI.Mensajeros;

namespace AgroEco.UI.Handlers
{
    public class SensorReadingHandler
    {
        private readonly Action<string, object> _enviar;
        private readonly ILogger<SensorReadingHandler> _logger;
        private readonly IServiceScopeFactory _scopeFactory;

        public SensorReadingHandler(
            Action<string, object> enviar,
            IServiceScopeFactory scopeFactory,
            ILogger<SensorReadingHandler> logger)
        {
            _enviar = enviar;
            _scopeFactory = scopeFactory;
            _logger = logger;
        }

        public async void ManejarMensaje(Mensaje msg)
        {
            switch (msg.Type)
            {
                case TiposMensaje.ListoSensores:
                    await EnviarConfiguracionSensores();
                    break;

                case TiposMensaje.LecturaSensor:
                    await ProcesarLecturaSensor(msg);
                    break;

                case TiposMensaje.ObtenerAccionesDisponibles:
                    await ObtenerAccionesDisponiblesAsync();
                    break;
            }
        }

        private async Task EnviarConfiguracionSensores()
        {
            var sensores = new[]
            {
                new { tipo = "temperatura_suelo", nombre = "Temperatura Suelo", unidad = "°C", icono = "thermometer" },
                new { tipo = "temperatura_ambiente", nombre = "Temperatura Ambiente", unidad = "°C", icono = "thermometer" },
                new { tipo = "humedad_suelo", nombre = "Humedad Suelo", unidad = "%", icono = "humidity" },
                new { tipo = "humedad_ambiente", nombre = "Humedad Ambiente", unidad = "%", icono = "humidity" },
                new { tipo = "luz", nombre = "Luz Solar", unidad = "lux", icono = "light" }
            };

            _enviar("sensoresConfigurados", sensores);
        }

        private async Task ObtenerAccionesDisponiblesAsync()
        {
            try
            {
                using var scope = _scopeFactory.CreateScope();
                var actionFactory = scope.ServiceProvider.GetRequiredService<IActionFactory>();
                var acciones = actionFactory.GetAvailable();

                var lista = acciones.Select(a => new
                {
                    typeId = a.TypeId,
                    displayName = a.DisplayName,
                    fields = a.Fields.Select(f => new
                    {
                        key = f.Name,
                        label = f.Label,
                        type = f.InputType,
                        f.Required,
                        f.Multiple,
                        choices = f.Choices?.Select(c => new { c.Value, c.Label })
                    })
                }).ToList();

                _enviar("accionesDisponibles", new { acciones = lista });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error obteniendo acciones disponibles");
                _enviar("accionesDisponibles", new { acciones = new List<object>() });
            }
        }

        private async Task ProcesarLecturaSensor(Mensaje msg)
        {
            try
            {
                var dto = msg.LeerPayload<LecturaSensorDto>();
                if (dto == null)
                {
                    _logger.LogWarning("Lectura de sensor inválida");
                    return;
                }

                _logger.LogInformation("Lectura recibida: {Sensor} = {Valor} en finca {Finca}",
                    dto.SensorTipo, dto.Valor, dto.FincaNombre);

                _enviar("lecturaProcesada", new { exito = true, sensor = dto.SensorTipo, valor = dto.Valor });

                // TODO: Implementar evaluación de umbrales via TriggerEngine cuando exista SensorThresholdTrigger
                // Por ahora solo reenviamos la lectura procesada
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error procesando lectura de sensor");
                _enviar("lecturaProcesada", new { exito = false, error = ex.Message });
            }
        }

        public void ProcesarLecturaExterna(LecturaSensorDto dto)
        {
            if (dto == null) return;

            try
            {
                _logger.LogInformation("Lectura externa recibida: {Sensor} = {Valor} en finca {Finca}",
                    dto.SensorTipo, dto.Valor, dto.FincaNombre);

                _enviar("lecturaProcesada", new { exito = true, sensor = dto.SensorTipo, valor = dto.Valor });

                // TODO: Implementar evaluación de umbrales via TriggerEngine cuando exista SensorThresholdTrigger
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error procesando lectura externa de sensor");
                _enviar("lecturaProcesada", new { exito = false, error = ex.Message });
            }
        }

        public record LecturaSensorDto(
            string SensorTipo,
            decimal Valor,
            int? FincaId,
            string FincaNombre,
            string SensorNombre,
            DateTime? Timestamp = null);
    }
}