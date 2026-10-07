using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AgroEco.Core.Alertas;
using AgroEco.Core.Alertas.Persistence;
using AgroEco.Core.Interfaces;
using AgroEco.Core.Inventario.Persistence;
using AgroEco.UI.Mensajeros;

namespace AgroEco.UI.Handlers
{
    public class SensorReadingHandler
    {
        private readonly Action<string, object> _enviar;
        private readonly ILogger<SensorReadingHandler> _logger;
        private readonly CreateUmbralSensor _createUmbral;
        private readonly GetAllUmbralesSensor _getAllUmbrales;
        private readonly GetByIdUmbralSensor _getByIdUmbral;
        private readonly UpdateUmbralSensor _updateUmbral;
        private readonly DeleteUmbralSensor _deleteUmbral;

        public SensorReadingHandler(
            Action<string, object> enviar,
            CreateUmbralSensor createUmbral,
            GetAllUmbralesSensor getAllUmbrales,
            GetByIdUmbralSensor getByIdUmbral,
            UpdateUmbralSensor updateUmbral,
            DeleteUmbralSensor deleteUmbral,
            ILogger<SensorReadingHandler> logger)
        {
            _enviar = enviar;
            _createUmbral = createUmbral;
            _getAllUmbrales = getAllUmbrales;
            _getByIdUmbral = getByIdUmbral;
            _updateUmbral = updateUmbral;
            _deleteUmbral = deleteUmbral;
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

                case "obtenerUmbrales":
                    await ObtenerUmbralesAsync();
                    break;

                case "crearUmbral":
                    await CrearUmbralAsync(msg);
                    break;

                case "actualizarUmbral":
                    await ActualizarUmbralAsync(msg);
                    break;

                case "eliminarUmbral":
                    await EliminarUmbralAsync(msg);
                    break;

                case "obtenerLecturas":
                    await ObtenerLecturasAsync();
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

        private async Task ObtenerUmbralesAsync()
        {
            try
            {
                var result = await _getAllUmbrales.HandleAsync(true);
                if (result.Success)
                {
                    _enviar("umbrales", new { umbrales = result.Value });
                }
                else
                {
                    _enviar("umbrales", new { umbrales = new List<object>() });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error obteniendo umbrales");
                _enviar("umbrales", new { umbrales = new List<object>() });
            }
        }

        private async Task CrearUmbralAsync(Mensaje msg)
{
                try
                {
                    var dto = msg.LeerPayload<CrearUmbralDto>();
                    if (dto == null)
                    {
                        _enviar("umbralError", new { mensaje = "Datos inválidos" });
                        return;
                    }

                    var result = await _createUmbral.HandleAsync(
                    dto.SensorTipo,
                    dto.FincaId,
                    dto.FincaNombre,
                    dto.Minimo,
                    dto.Maximo,
                    dto.SeveridadMinima,
                    dto.SeveridadMaxima,
                    dto.GenerarTareaAuto,
                    dto.AccionSugerida,
                    dto.InsumoSugeridoId,
                    dto.CantidadInsumoSugerida,
                    dto.CostoUnitarioSugerido,
                    dto.Activo,
                    dto.AccionTipo,
                    dto.AccionConfigJson,
                    dto.CooldownMinutos);

                if (result.Success)
                {
                    _logger.LogInformation("Umbral creado: {SensorTipo} para {Finca}", result.Value.SensorTipo, result.Value.FincaNombre);
                    _enviar("umbralCreado", new { success = true, umbral = result.Value });
                }
                else
                {
                    _enviar("umbralError", new { mensaje = result.Message });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creando umbral");
                _enviar("umbralError", new { mensaje = ex.Message });
            }
        }

        private async Task ActualizarUmbralAsync(Mensaje msg)
        {
            try
            {
                var dto = msg.LeerPayload<ActualizarUmbralDto>();
                if (dto == null || dto.Id <= 0)
                {
                    _enviar("umbralError", new { mensaje = "ID inválido" });
                    return;
                }

                var existingResult = await _getByIdUmbral.HandleAsync(dto.Id);
                if (!existingResult.Success || existingResult.Value == null)
                {
                    _enviar("umbralError", new { mensaje = "Umbral no encontrado" });
                    return;
                }

                var umbral = existingResult.Value;
                umbral.SensorTipo = dto.SensorTipo;
                umbral.FincaId = dto.FincaId;
                umbral.FincaNombre = dto.FincaNombre;
                umbral.Minimo = dto.Minimo;
                umbral.Maximo = dto.Maximo;
                umbral.SeveridadMinima = dto.SeveridadMinima;
                umbral.SeveridadMaxima = dto.SeveridadMaxima;
                umbral.Activo = dto.Activo;
                umbral.GenerarTareaAuto = dto.GenerarTareaAuto;
                umbral.AccionSugerida = dto.AccionSugerida;
                umbral.InsumoSugeridoId = dto.InsumoSugeridoId;
                umbral.CantidadInsumoSugerida = dto.CantidadInsumoSugerida;
                umbral.CostoUnitarioSugerido = dto.CostoUnitarioSugerido;
                umbral.AccionTipo = dto.AccionTipo;
                umbral.AccionConfigJson = dto.AccionConfigJson;
                umbral.CooldownMinutos = dto.CooldownMinutos;
                umbral.FechaActualizacion = DateTime.UtcNow;

                var result = await _updateUmbral.HandleAsync(umbral);
                if (result.Success)
                {
                    _logger.LogInformation("Umbral actualizado: {SensorTipo} para {Finca}", umbral.SensorTipo, umbral.FincaNombre);
                    _enviar("umbralActualizado", new { success = true, umbral });
                }
                else
                {
                    _enviar("umbralError", new { mensaje = result.Message });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error actualizando umbral");
                _enviar("umbralError", new { mensaje = ex.Message });
            }
        }

        private async Task EliminarUmbralAsync(Mensaje msg)
        {
            try
            {
                var dto = msg.LeerPayload<EliminarUmbralDto>();
                if (dto == null || dto.Id <= 0)
                {
                    _enviar("umbralError", new { mensaje = "ID inválido" });
                    return;
                }

                var result = await _deleteUmbral.HandleAsync(dto.Id);
                if (result.Success)
                {
                    _logger.LogInformation("Umbral eliminado (Id: {Id})", dto.Id);
                    _enviar("umbralEliminado", new { success = true });
                }
                else
                {
                    _enviar("umbralError", new { mensaje = result.Message });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error eliminando umbral");
                _enviar("umbralError", new { mensaje = ex.Message });
            }
        }

        private async Task ObtenerLecturasAsync()
        {
            // Placeholder - en implementación real leería de una tabla de lecturas de sensores
            try
            {
                _enviar("lecturas", new { lecturas = new List<object>() });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error obteniendo lecturas");
                _enviar("lecturas", new { lecturas = new List<object>() });
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
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error procesando lectura de sensor");
                _enviar("lecturaProcesada", new { exito = false, error = ex.Message });
            }
        }

        // Método público para procesar lecturas desde el serial (hardware)
        public void ProcesarLecturaExterna(LecturaSensorDto dto)
        {
            if (dto == null) return;
            
            try
            {
                _logger.LogInformation("Lectura externa recibida: {Sensor} = {Valor} en finca {Finca}", 
                    dto.SensorTipo, dto.Valor, dto.FincaNombre);

                _enviar("lecturaProcesada", new { exito = true, sensor = dto.SensorTipo, valor = dto.Valor });
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

        private record CrearUmbralDto(
            string SensorTipo,
            int? FincaId,
            string FincaNombre,
            decimal? Minimo,
            decimal? Maximo,
            string SeveridadMinima,
            string SeveridadMaxima,
            bool GenerarTareaAuto,
            string AccionSugerida,
            int? InsumoSugeridoId,
            decimal? CantidadInsumoSugerida,
            decimal? CostoUnitarioSugerido,
            bool Activo = true,
            string? AccionTipo = null,
            string? AccionConfigJson = null,
            int CooldownMinutos = 30);

        private record ActualizarUmbralDto(
            int Id,
            string SensorTipo,
            int? FincaId,
            string FincaNombre,
            decimal? Minimo,
            decimal? Maximo,
            string SeveridadMinima,
            string SeveridadMaxima,
            bool Activo,
            bool GenerarTareaAuto,
            string AccionSugerida,
            int? InsumoSugeridoId,
            decimal? CantidadInsumoSugerida,
            decimal? CostoUnitarioSugerido,
            string? AccionTipo = null,
            string? AccionConfigJson = null,
            int CooldownMinutos = 30);

        private record EliminarUmbralDto(int Id);
    }
}