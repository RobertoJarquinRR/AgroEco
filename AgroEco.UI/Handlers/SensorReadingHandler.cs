using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using AgroEco.Core.Alertas;
using AgroEco.Core.Alertas.Persistence;
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
        private readonly CreateUmbralSensor _createUmbral;
        private readonly GetAllUmbralesSensor _getAllUmbrales;
        private readonly GetByIdUmbralSensor _getByIdUmbral;
        private readonly UpdateUmbralSensor _updateUmbral;
        private readonly DeleteUmbralSensor _deleteUmbral;
        private readonly IServiceScopeFactory _scopeFactory;

        public SensorReadingHandler(
            Action<string, object> enviar,
            CreateUmbralSensor createUmbral,
            GetAllUmbralesSensor getAllUmbrales,
            GetByIdUmbralSensor getByIdUmbral,
            UpdateUmbralSensor updateUmbral,
            DeleteUmbralSensor deleteUmbral,
            IServiceScopeFactory scopeFactory,
            ILogger<SensorReadingHandler> logger)
        {
            _enviar = enviar;
            _createUmbral = createUmbral;
            _getAllUmbrales = getAllUmbrales;
            _getByIdUmbral = getByIdUmbral;
            _updateUmbral = updateUmbral;
            _deleteUmbral = deleteUmbral;
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

                case TiposMensaje.ObtenerUmbrales:
                    await ObtenerUmbralesAsync();
                    break;

                case TiposMensaje.CrearUmbral:
                    await CrearUmbralAsync(msg);
                    break;

                case TiposMensaje.ActualizarUmbral:
                    await ActualizarUmbralAsync(msg);
                    break;

                case TiposMensaje.EliminarUmbral:
                    await EliminarUmbralAsync(msg);
                    break;

                case TiposMensaje.ObtenerLecturas:
                    await ObtenerLecturasAsync();
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

                await EvaluarUmbralesAsync(dto);
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

                _ = EvaluarUmbralesAsync(dto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error procesando lectura externa de sensor");
                _enviar("lecturaProcesada", new { exito = false, error = ex.Message });
            }
        }

        private async Task EvaluarUmbralesAsync(LecturaSensorDto dto)
        {
            try
            {
                var result = await _getAllUmbrales.HandleAsync(true);
                if (!result.Success || result.Value == null) return;

                var umbrales = result.Value
                    .Where(u => u.Activo 
                             && u.SensorTipo == dto.SensorTipo
                             && (u.FincaId == null || u.FincaId == dto.FincaId))
                    .ToList();

                foreach (var umbral in umbrales)
                {
                    bool cruzaMinimo = umbral.Minimo.HasValue && dto.Valor < umbral.Minimo.Value;
                    bool cruzaMaximo = umbral.Maximo.HasValue && dto.Valor > umbral.Maximo.Value;
                    
                    if (!cruzaMinimo && !cruzaMaximo) continue;

                    if (umbral.UltimoDisparo.HasValue 
                        && DateTime.UtcNow < umbral.UltimoDisparo.Value.AddMinutes(umbral.CooldownMinutos))
                    {
                        _logger.LogInformation("Umbral {Id} en cooldown, saltando", umbral.Id);
                        continue;
                    }

                    if (!string.IsNullOrEmpty(umbral.AccionTipo))
                    {
                        await EjecutarAccionUmbralAsync(umbral, dto, cruzaMinimo ? "min" : "max");
                    }

                    umbral.UltimoDisparo = DateTime.UtcNow;
                    await _updateUmbral.HandleAsync(umbral);

                    if (umbral.GenerarTareaAuto)
                    {
                        _logger.LogInformation("Generando tarea automática para umbral {Id}", umbral.Id);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error evaluando umbrales para sensor {Sensor}", dto.SensorTipo);
            }
        }

        private async Task EjecutarAccionUmbralAsync(UmbralSensor umbral, LecturaSensorDto dto, string direccion)
        {
            try
            {
                using var scope = _scopeFactory.CreateScope();
                var actionFactory = scope.ServiceProvider.GetRequiredService<IActionFactory>();

                if (string.IsNullOrWhiteSpace(umbral.AccionConfigJson))
                {
                    _logger.LogWarning("Umbral {Id} no tiene configuración para la acción {Tipo}; no se ejecuta", umbral.Id, umbral.AccionTipo);
                    return;
                }

                ActionConfiguration config;
                try
                {
                    using var document = JsonDocument.Parse(umbral.AccionConfigJson);
                    config = ActionConfigurationParser.Parse(umbral.AccionTipo, document.RootElement);
                }
                catch (JsonException ex)
                {
                    _logger.LogError(ex, "Configuración de acción inválida para umbral {Id}", umbral.Id);
                    return;
                }

                var createResult = actionFactory.Create(umbral.AccionTipo!, $"Umbral_{umbral.Id}_{direccion}", config);
                if (!createResult.Success)
                {
                    _logger.LogError("Error creando acción {Tipo} para umbral {Id}: {Error}", 
                        umbral.AccionTipo, umbral.Id, createResult.Message);
                    return;
                }

                createResult.Value.AttachServices(scope.ServiceProvider);
                
                var execResult = await createResult.Value.Execute();
                
                if (execResult.Success)
                {
                    _logger.LogInformation("Acción {Tipo} ejecutada para umbral {Id}: {Msg}", 
                        umbral.AccionTipo, umbral.Id, execResult.Message);
                }
                else
                {
                    _logger.LogWarning("Acción {Tipo} falló para umbral {Id}: {Error}", 
                        umbral.AccionTipo, umbral.Id, execResult.Message);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error ejecutando acción {Tipo} para umbral {Id}", umbral.AccionTipo, umbral.Id);
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