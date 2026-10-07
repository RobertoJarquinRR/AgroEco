using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AgroEco.Core.Alertas;
using AgroEco.UI.Mensajeros;

namespace AgroEco.UI.Handlers
{
    public class SensorReadingHandler
    {
        private readonly Action<string, object> _enviar;
        private readonly ILogger<SensorReadingHandler> _logger;
        private readonly AlertEngine _alertEngine;

        public SensorReadingHandler(
            Action<string, object> enviar,
            AlertEngine alertEngine,
            ILogger<SensorReadingHandler> logger)
        {
            _enviar = enviar;
            _alertEngine = alertEngine;
            _logger = logger;
        }

        public async void ManejarMensaje(Mensaje msg)
        {
            switch (msg.Type)
            {
                case "ready_sensores":
                    await EnviarConfiguracionSensores();
                    break;

                case "lecturaSensor":
                    await ProcesarLecturaSensor(msg);
                    break;

                case "configurarSensor":
                    await ConfigurarSensor(msg);
                    break;
            }
        }

        private async Task EnviarConfiguracionSensores()
        {
            // Enviar lista de sensores disponibles y sus tipos
            var sensores = new[]
            {
                new { tipo = "temperatura_suelo", nombre = "Temperatura Suelo", unidad = "°C", icono = "🌡️" },
                new { tipo = "temperatura_ambiente", nombre = "Temperatura Ambiente", unidad = "°C", icono = "🌡️" },
                new { tipo = "humedad_suelo", nombre = "Humedad Suelo", unidad = "%", icono = "💧" },
                new { tipo = "humedad_ambiente", nombre = "Humedad Ambiente", unidad = "%", icono = "💧" },
                new { tipo = "luz", nombre = "Luz Solar", unidad = "lux", icono = "☀️" }
            };

            _enviar("sensoresConfigurados", sensores);
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

                // Evaluar contra umbrales
                await _alertEngine.EvaluarLecturaAsync(
                    sensorTipo: dto.SensorTipo,
                    valor: dto.Valor,
                    fincaId: dto.FincaId,
                    fincaNombre: dto.FincaNombre,
                    sensorNombre: dto.SensorNombre,
                    CancellationToken.None);

                // Enviar confirmación al frontend
                _enviar("lecturaProcesada", new { exito = true, sensor = dto.SensorTipo, valor = dto.Valor });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error procesando lectura de sensor");
                _enviar("lecturaProcesada", new { exito = false, error = ex.Message });
            }
        }

        private async Task ConfigurarSensor(Mensaje msg)
        {
            // Placeholder para configurar sensor vía serial
            await Task.CompletedTask;
        }

        private record LecturaSensorDto(
            string SensorTipo,
            decimal Valor,
            int? FincaId,
            string FincaNombre,
            string SensorNombre,
            DateTime? Timestamp = null);
    }
}