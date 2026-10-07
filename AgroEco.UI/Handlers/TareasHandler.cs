using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.Json;
using AgroEco.Core;
using System.Threading.Tasks;
using AgroEco.Core.Jobs;
using AgroEco.Core.Jobs.Persistence;
using AgroEco.Core.Triggers;
using AgroEco.Core.Triggers.Configuration;
using AgroEco.Core.Triggers.Implementations;
using AgroEco.Core.Jobs.Actions;
using AgroEco.Core.Jobs.Actions.Configuration;
using AgroEco.Core.Inventario.Persistence;
using AgroEco.Core.Reportes;
using CoreAction = AgroEco.Core.Jobs.Actions.Action;
using AgroEco.UI.Mensajeros;
using Microsoft.Extensions.Logging;

namespace AgroEco.UI.Handlers
{
    public class TareasHandler
    {
        private readonly Action<string, object> _enviar;
        private readonly CreateJob _createJob;
        private readonly GetAllJob _getAllJob;
        private readonly GetRunningJobs _getRunningJobs;
        private readonly GetByIdJobWithDetails _getByIdJob;
        private readonly UpdateJob _updateJob;
        private readonly DeleteJob _deleteJob;
        private readonly JobEngine _jobEngine;
        private readonly ITriggerFactory _triggerFactory;
        private readonly IActionFactory _actionFactory;
        private readonly GetByIdInsumo _getByIdInsumo;
        private readonly IExportService _exportService;
        private readonly ILogger<TareasHandler> _logger;

        public TareasHandler(
            Action<string, object> enviar,
            CreateJob createJob,
            GetAllJob getAllJob,
            GetRunningJobs getRunningJobs,
            GetByIdJobWithDetails getByIdJob,
            UpdateJob updateJob,
            DeleteJob deleteJob,
            JobEngine jobEngine,
            ITriggerFactory triggerFactory,
            IActionFactory actionFactory,
            GetByIdInsumo getByIdInsumo,
            IExportService exportService,
            ILogger<TareasHandler> logger)
        {
            _enviar = enviar;
            _createJob = createJob;
            _getAllJob = getAllJob;
            _getRunningJobs = getRunningJobs;
            _getByIdJob = getByIdJob;
            _updateJob = updateJob;
            _deleteJob = deleteJob;
            _jobEngine = jobEngine;
            _triggerFactory = triggerFactory;
            _actionFactory = actionFactory;
            _getByIdInsumo = getByIdInsumo;
            _exportService = exportService;
            _logger = logger;
            _jobEngine.JobExecutionCompleted += OnJobExecutionCompleted;
        }

        private void OnJobExecutionCompleted(Job job)
        {
            if (job.Status == Status.Faulted)
            {
                string? failureMessage = job.Results
                    .Where(result => !result.Success)
                    .Select(result => result.Message)
                    .FirstOrDefault(message => !string.IsNullOrWhiteSpace(message));

                _enviar(
                    "tareaError",
                    new
                    {
                        mensaje = failureMessage
                            ?? $"La tarea '{job.Name}' falló durante la ejecución."
                    });
            }
            else if (job.Status == Status.Succeeded || job.Status == Status.CompletedWithErrors)
            {
                // La acción ExecuteTaskAction ya hace el trabajo (descuenta inventario y registra gasto)
                // Solo refrescamos la lista
            }

            _ = ObtenerTareasAsync();
        }

        public void Dispose()
        {
            _jobEngine.JobExecutionCompleted -= OnJobExecutionCompleted;
        }

        public void ManejarMensaje(Mensaje msg)
        {
            _ = msg.Type switch
            {
                "obtenerTareas" => ObtenerTareasAsync(),
                "obtenerTriggersDisponibles" => EnviarTriggersDisponibles(),
                "obtenerActionsDisponibles" => EnviarActionsDisponibles(),
                "crearTarea" => CrearTareaAsync(msg),
                "actualizarTarea" => ActualizarTareaAsync(msg),
                "actualizarEstadoTarea" => ActualizarEstadoTareaAsync(msg),
                "eliminarTarea" => EliminarTareaAsync(msg),
                "ejecutarTarea" => EjecutarTareaAsync(msg),
                "obtenerTareaDetalle" => ObtenerTareaDetalleAsync(msg),
                "obtenerHistorialTarea" => ObtenerHistorialTareaAsync(msg),
                "exportarTareasCsv" => ExportarTareasCsvAsync(msg),
                _ => Task.CompletedTask
            };
        }

        private async Task ObtenerTareasAsync()
        {
            try
            {
                var allResult = await _getAllJob.HandleAsync();
                var runningResult = await _getRunningJobs.HandleAsync();

                var allJobs = allResult.Value ?? new List<Job>();
                var runningJobs = runningResult.Value ?? new List<Job>();
                var runningIds = runningJobs.Select(j => j.Id).ToHashSet();

                var tareas = allJobs.Select(j => MappearJobATarea(j, runningIds.Contains(j.Id))).ToList();
                _enviar("tareasCargadas", tareas);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error obteniendo tareas");
                _enviar("tareasCargadas", new List<object>());
            }
        }

        private async Task ObtenerTareaDetalleAsync(Mensaje msg)
        {
            try
            {
                var dto = msg.LeerPayload<ObtenerTareaDetalleDto>();
                if (dto == null || dto.Id <= 0)
                {
                    _enviar("tareaError", new { mensaje = "ID inválido" });
                    return;
                }

                var jobResult = await _getByIdJob.HandleAsync(dto.Id);
                if (!jobResult.Success || jobResult.Value == null)
                {
                    _enviar("tareaError", new { mensaje = "Tarea no encontrada" });
                    return;
                }

                var job = jobResult.Value;
                var detail = MappearJobADetalle(job);
                _enviar("tareaDetalle", detail);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error obteniendo detalle de tarea");
                _enviar("tareaError", new { mensaje = ex.Message });
            }
        }

        private object MappearJobADetalle(Job job)
        {
            object? triggerConfig = job.Trigger switch
            {
                DateTimeTrigger dt => new { type = "datetime", targetTime = dt.TargetTime },
                CronTrigger ct => new { type = "cron", cronExpression = ct.Config.CronExpression, timeZone = ct.Config.TimeZone, startDate = ct.Config.StartDate, endDate = ct.Config.EndDate },
                _ => null
            };

            var actionConfig = job.Actions.Count > 0 ? job.Actions[0].Configuration : null;

            return new
            {
                id = job.Id,
                nombre = job.Name,
                descripcion = job.Description ?? "",
                prioridad = job.Priority switch { 1 => "alta", 2 => "media", _ => "baja" },
                estado = job.Status.ToString().ToLower(),
                trigger = triggerConfig,
                action = actionConfig != null ? new
                {
                    typeId = job.Actions[0].GetType().Name == "ExecuteTaskAction" ? "executeTask" : "noop",
                    config = actionConfig
                } : null
            };
        }

        private record ObtenerTareaDetalleDto(int Id);

        private async Task ObtenerHistorialTareaAsync(Mensaje msg)
        {
            try
            {
                var dto = msg.LeerPayload<ObtenerHistorialTareaDto>();
                if (dto == null || dto.Id <= 0)
                {
                    _enviar("tareaError", new { mensaje = "ID inválido" });
                    return;
                }

                var jobResult = await _getByIdJob.HandleAsync(dto.Id);
                if (!jobResult.Success || jobResult.Value == null)
                {
                    _enviar("tareaError", new { mensaje = "Tarea no encontrada" });
                    return;
                }

                var job = jobResult.Value;
                var historial = new
                {
                    ejecuciones = job.Results.Select((r, index) => new
                    {
                        ejecutadoEn = job.Date?.ToString("o") ?? DateTime.Now.ToString("o"),
                        estado = job.Status.ToString(),
                        mensaje = r.Message,
                        acciones = job.Actions.Select(a => new
                        {
                            nombre = a.Name,
                            estado = a.Status.ToString()
                        }).ToList()
                    }).ToList()
                };

                _enviar("historialTarea", historial);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error obteniendo historial de tarea");
                _enviar("tareaError", new { mensaje = ex.Message });
            }
        }

        private record ObtenerHistorialTareaDto(int Id);

        private async Task CrearTareaAsync(Mensaje msg)
        {
            try
            {
                var dto = msg.LeerPayload<CrearTareaDto>();
                if (dto == null)
                {
                    _enviar("tareaError", new { mensaje = "Datos inválidos" });
                    return;
                }

                // Validar acción executeTask
                if (dto.ActionTypeId == "executeTask")
                {
                    var validationResult = await ValidarConfiguracionExecuteTask(dto.ActionConfig);
                    if (!validationResult.Success)
                    {
                        _enviar("tareaError", new { mensaje = validationResult.Message });
                        return;
                    }
                }

                string triggerTypeId = string.IsNullOrWhiteSpace(dto.TriggerTypeId)
                    ? "datetime"
                    : dto.TriggerTypeId;
                if (dto.TriggerConfig.ValueKind != JsonValueKind.Object)
                {
                    _enviar("tareaError", new { mensaje = "La configuración del trigger es obligatoria." });
                    return;
                }
                Result<Trigger> triggerResult = _triggerFactory.Create(
                    triggerTypeId,
                    $"Trigger_{dto.Nombre}",
                    ParseTriggerConfiguration(dto.TriggerConfig));
                if (!triggerResult.Success || triggerResult.Value is null)
                {
                    _enviar("tareaError", new { mensaje = triggerResult.Message });
                    return;
                }

                if (string.IsNullOrWhiteSpace(dto.ActionTypeId))
                {
                    _enviar("tareaError", new { mensaje = "El tipo de acción es obligatorio." });
                    return;
                }

                string actionTypeId = dto.ActionTypeId;
                JsonElement actionConfig = dto.ActionConfig.ValueKind == JsonValueKind.Object
                    ? dto.ActionConfig
                    : JsonSerializer.SerializeToElement(new { });
                Result<CoreAction> actionResult = _actionFactory.Create(
                    actionTypeId,
                    $"Accion_{dto.Nombre}",
                    ParseActionConfiguration(actionConfig));
                if (!actionResult.Success || actionResult.Value is null)
                {
                    _enviar("tareaError", new { mensaje = actionResult.Message });
                    return;
                }

                var result = await _createJob.HandleAsync(
                    dto.Nombre,
                    dto.Descripcion,
                    dto.Prioridad switch { "alta" => 1, "media" => 2, _ => 3 },
                    new List<CoreAction> { actionResult.Value },
                    triggerResult.Value);

                if (result.Success)
                {
                    _enviar("tareaCreada", new { mensaje = "Tarea creada correctamente" });
                    await ObtenerTareasAsync();
                }
                else
                {
                    _enviar("tareaError", new { mensaje = result.Message });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creando tarea");
                _enviar("tareaError", new { mensaje = ex.Message });
            }
        }

        private async Task<Result> ValidarConfiguracionExecuteTask(JsonElement actionConfig)
        {
            if (!actionConfig.TryGetProperty("insumoId", out var insumoIdEl) || insumoIdEl.ValueKind != JsonValueKind.Number)
            {
                return Result.CreateFailure("insumoId es obligatorio y debe ser un número");
            }

            int insumoId = insumoIdEl.GetInt32();
            if (insumoId <= 0)
            {
                return Result.CreateFailure("insumoId debe ser mayor a 0");
            }

            // Verificar que el insumo existe
            var insumoResult = await _getByIdInsumo.HandleAsync(insumoId);
            if (!insumoResult.Success || insumoResult.Value == null)
            {
                return Result.CreateFailure($"Insumo con ID {insumoId} no encontrado");
            }

            var insumo = insumoResult.Value;

            if (!actionConfig.TryGetProperty("cantidadDescontar", out var cantidadEl) || cantidadEl.ValueKind != JsonValueKind.Number)
            {
                return Result.CreateFailure("cantidadDescontar es obligatoria y debe ser un número");
            }

            decimal cantidadDescontar = cantidadEl.GetDecimal();
            if (cantidadDescontar <= 0)
            {
                return Result.CreateFailure("cantidadDescontar debe ser mayor a 0");
            }

            // Validar stock suficiente
            if (insumo.Cantidad < cantidadDescontar)
            {
                return Result.CreateFailure(
                    $"Stock insuficiente para '{insumo.Nombre}'. Disponible: {insumo.Cantidad} {insumo.Unidad}, Requerido: {cantidadDescontar} {insumo.Unidad}");
            }

            if (!actionConfig.TryGetProperty("costoUnitario", out var costoEl) || costoEl.ValueKind != JsonValueKind.Number)
            {
                return Result.CreateFailure("costoUnitario es obligatorio y debe ser un número");
            }

            decimal costoUnitario = costoEl.GetDecimal();
            if (costoUnitario < 0)
            {
                return Result.CreateFailure("costoUnitario no puede ser negativo");
            }

            return Result.CreateSuccess();
        }

        private async Task ActualizarTareaAsync(Mensaje msg)
        {
            try
            {
                var dto = msg.LeerPayload<ActualizarTareaDto>();
                if (dto == null || dto.Id <= 0)
                {
                    _enviar("tareaError", new { mensaje = "ID inválido" });
                    return;
                }

                var jobResult = await _getByIdJob.HandleAsync(dto.Id);
                if (!jobResult.Success || jobResult.Value == null)
                {
                    _enviar("tareaError", new { mensaje = "Tarea no encontrada" });
                    return;
                }

                var job = jobResult.Value;
                var updateResult = job.UpdateDetails(
                    dto.Nombre,
                    dto.Descripcion,
                    dto.Prioridad switch { "alta" => 1, "media" => 2, _ => 3 },
                    job.Date);

                if (!updateResult.Success)
                {
                    _enviar("tareaError", new { mensaje = updateResult.Message });
                    return;
                }

                if (dto.TriggerConfig.ValueKind != JsonValueKind.Object)
                {
                    _enviar("tareaError", new { mensaje = "La configuración del trigger es obligatoria." });
                    return;
                }

                // Handle different trigger types
                if (job.Trigger is DateTimeTrigger dateTimeTrigger)
                {
                    if (!TryReadTargetTime(dto.TriggerConfig, out DateTimeOffset targetTime))
                    {
                        _enviar("tareaError", new { mensaje = "La hora objetivo debe ser una fecha y hora válida." });
                        return;
                    }

                    if (targetTime <= DateTimeOffset.UtcNow)
                    {
                        _enviar("tareaError", new { mensaje = "La hora objetivo debe estar en el futuro." });
                        return;
                    }

                    Result triggerUpdateResult = dateTimeTrigger.UpdateConfiguration(new DateTimeTriggerConfiguration(targetTime));
                    if (!triggerUpdateResult.Success)
                    {
                        _enviar("tareaError", new { mensaje = triggerUpdateResult.Message });
                        return;
                    }
                }
                else if (job.Trigger is CronTrigger cronTrigger)
                {
                    // Handle CronTrigger update
                    var cronConfig = ParseCronTriggerConfiguration(dto.TriggerConfig);
                    Result triggerUpdateResult = cronTrigger.UpdateConfiguration(cronConfig);
                    if (!triggerUpdateResult.Success)
                    {
                        _enviar("tareaError", new { mensaje = triggerUpdateResult.Message });
                        return;
                    }
                }

                // Handle action configuration update
                if (dto.ActionConfig.ValueKind == JsonValueKind.Object)
                {
                    var actionConfigResult = ParseActionConfiguration(dto.ActionConfig);
                    if (job.Actions.Count > 0)
                    {
                        var action = job.Actions[0];
                        action.Configuration = actionConfigResult;
                    }
                }

                var result = await _updateJob.HandleAsync(job);
                if (result.Success)
                {
                    _enviar("tareaActualizada", new { mensaje = "Tarea actualizada correctamente" });
                    await ObtenerTareasAsync();
                }
                else
                {
                    _enviar("tareaError", new { mensaje = result.Message });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error actualizando tarea");
                _enviar("tareaError", new { mensaje = ex.Message });
            }
        }

        private CronTriggerConfiguration ParseCronTriggerConfiguration(JsonElement config)
        {
            string cronExpression = "";
            string timeZone = "UTC";
            DateOnly? startDate = null;
            DateOnly? endDate = null;

            if (config.TryGetProperty("cronExpression", out var cronEl) && cronEl.ValueKind == JsonValueKind.String)
            {
                cronExpression = cronEl.GetString() ?? "";
            }
            if (config.TryGetProperty("timeZone", out var tzEl) && tzEl.ValueKind == JsonValueKind.String)
            {
                timeZone = tzEl.GetString() ?? "UTC";
            }
            if (config.TryGetProperty("startDate", out var startEl) && startEl.ValueKind == JsonValueKind.String)
            {
                if (DateOnly.TryParse(startEl.GetString(), out var parsed))
                    startDate = parsed;
            }
            if (config.TryGetProperty("endDate", out var endEl) && endEl.ValueKind == JsonValueKind.String)
            {
                if (DateOnly.TryParse(endEl.GetString(), out var parsed))
                    endDate = parsed;
            }

            return new CronTriggerConfiguration
            {
                CronExpression = cronExpression,
                TimeZone = timeZone,
                StartDate = startDate,
                EndDate = endDate
            };
        }

        private async Task EliminarTareaAsync(Mensaje msg)
        {
            try
            {
                var dto = msg.LeerPayload<EliminarTareaDto>();
                if (dto == null || dto.Id <= 0)
                {
                    _enviar("tareaError", new { mensaje = "ID inválido" });
                    return;
                }

                var result = await _deleteJob.HandleAsync(dto.Id);
                if (result.Success)
                {
                    _enviar("tareaEliminada", new { mensaje = "Tarea eliminada correctamente" });
                    await ObtenerTareasAsync();
                }
                else
                {
                    _enviar("tareaError", new { mensaje = result.Message });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error eliminando tarea");
                _enviar("tareaError", new { mensaje = ex.Message });
            }
        }

        private async Task EjecutarTareaAsync(Mensaje msg)
        {
            try
            {
                var dto = msg.LeerPayload<EjecutarTareaDto>();
                if (dto == null || dto.Id <= 0)
                {
                    _enviar("tareaError", new { mensaje = "ID inválido" });
                    return;
                }

                var result = await _jobEngine.RunJob(dto.Id);
                if (result.Success)
                {
                    _enviar("tareaEjecutada", new { mensaje = result.Message });
                    await ObtenerTareasAsync();
                }
                else
                {
                    _enviar("tareaError", new { mensaje = result.Message });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error ejecutando tarea");
                _enviar("tareaError", new { mensaje = ex.Message });
            }
        }

        private async Task ActualizarEstadoTareaAsync(Mensaje msg)
        {
            try
            {
                var dto = msg.LeerPayload<ActualizarEstadoTareaDto>();
                if (dto == null || dto.Id <= 0)
                {
                    _enviar("tareaError", new { mensaje = "ID inválido" });
                    return;
                }

                var jobResult = await _getByIdJob.HandleAsync(dto.Id);
                if (!jobResult.Success || jobResult.Value == null)
                {
                    _enviar("tareaError", new { mensaje = "Tarea no encontrada" });
                    return;
                }

                var job = jobResult.Value;
                var status = dto.Estado switch
                {
                    "completada" => Status.Succeeded,
                    "completada_con_errores" => Status.CompletedWithErrors,
                    "progreso" => Status.Running,
                    "pendiente" => Status.Enqueued,
                    "cancelada" => Status.Canceled,
                    _ => Status.Enqueued
                };

                var statusResult = job.ChangeStatus(status);
                if (!statusResult.Success)
                {
                    _enviar("tareaError", new { mensaje = statusResult.Message });
                    return;
                }

                var updateResult = await _updateJob.HandleAsync(job);
                if (updateResult.Success)
                {
                    _enviar("tareaActualizada", new { mensaje = "Estado actualizado correctamente" });
                    await ObtenerTareasAsync();
                }
                else
                {
                    _enviar("tareaError", new { mensaje = updateResult.Message });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error actualizando estado de tarea");
                _enviar("tareaError", new { mensaje = ex.Message });
            }
        }

        private Task EnviarTriggersDisponibles()
        {
            _enviar("triggersDisponibles", _triggerFactory.GetAvailable());
            return Task.CompletedTask;
        }

        private Task EnviarActionsDisponibles()
        {
            _enviar("actionsDisponibles", _actionFactory.GetAvailable());
            return Task.CompletedTask;
        }

        private object MappearJobATarea(Job job, bool isRunning)
        {
            var estado = job.Status switch
            {
                Status.Succeeded => "completada",
                Status.CompletedWithErrors => "completada_con_errores",
                Status.Faulted => "vencida",
                Status.Canceled => "cancelada",
                Status.Running => "progreso",
                Status.Enqueued => "pendiente",
                Status.Created => "pendiente",
                _ => "pendiente"
            };

            if (isRunning)
            {
                estado = "progreso";
            }

            return new
            {
                id = job.Id,
                nombre = job.Name,
                descripcion = job.Description ?? "",
                asignado = "Sistema",
                prioridad = job.Priority switch { 1 => "alta", 2 => "media", _ => "baja" },
                estado = estado,
                triggerTypeId = job.Trigger is DateTimeTrigger ? "datetime" : null,
                triggerConfig = job.Trigger is DateTimeTrigger dateTimeTrigger
                    ? new { targetTime = dateTimeTrigger.TargetTime }
                    : null
            };
        }

        private record CrearTareaDto(
            string Nombre,
            string Descripcion,
            string Asignado,
            string Prioridad,
            string Estado,
            string? TriggerTypeId,
            JsonElement TriggerConfig,
            string? ActionTypeId,
            JsonElement ActionConfig);

        private record ActualizarTareaDto(
            int Id,
            string Nombre,
            string Descripcion,
            string Asignado,
            string Prioridad,
            string Estado,
            string? TriggerTypeId,
            JsonElement TriggerConfig,
            string? ActionTypeId,
            JsonElement ActionConfig);

        private record EliminarTareaDto(int Id);

        private record EjecutarTareaDto(int Id);

        private record ActualizarEstadoTareaDto(int Id, string Estado);

        private static bool TryReadTargetTime(
            JsonElement config,
            out DateTimeOffset targetTime)
        {
            targetTime = default;
            return config.TryGetProperty("targetTime", out JsonElement element)
                && element.ValueKind == JsonValueKind.String
                && DateTimeOffset.TryParse(
                    element.GetString(),
                    CultureInfo.InvariantCulture,
                    DateTimeStyles.AssumeLocal,
                    out targetTime);
        }

        private static TriggerConfiguration ParseTriggerConfiguration(
            JsonElement config)
        {
            return TryReadTargetTime(config, out DateTimeOffset targetTime)
                ? new DateTimeTriggerConfiguration(targetTime)
                : new InvalidTriggerConfiguration();
        }

        private static ActionConfiguration ParseActionConfiguration(
            JsonElement config)
        {
            int insumoId = 0;
            decimal cantidadDescontar = 0;
            decimal costoUnitario = 0;
            string? descripcion = null;
            string? categoriaInsumo = null;

            if (config.TryGetProperty("insumoId", out var insumoIdEl) && insumoIdEl.ValueKind == JsonValueKind.Number)
            {
                insumoId = insumoIdEl.GetInt32();
            }
            if (config.TryGetProperty("cantidadDescontar", out var cantidadEl) && cantidadEl.ValueKind == JsonValueKind.Number)
            {
                cantidadDescontar = cantidadEl.GetDecimal();
            }
            if (config.TryGetProperty("costoUnitario", out var costoEl) && costoEl.ValueKind == JsonValueKind.Number)
            {
                costoUnitario = costoEl.GetDecimal();
            }
            if (config.TryGetProperty("descripcion", out var descEl) && descEl.ValueKind == JsonValueKind.String)
            {
                descripcion = descEl.GetString();
            }
            if (config.TryGetProperty("categoriaInsumo", out var catEl) && catEl.ValueKind == JsonValueKind.String)
            {
                categoriaInsumo = catEl.GetString();
            }

            return new ExecuteTaskActionConfiguration
            {
                InsumoId = insumoId,
                CantidadDescontar = cantidadDescontar,
                CostoUnitario = costoUnitario,
                Descripcion = descripcion,
                CategoriaInsumo = categoriaInsumo
            };
        }

        private sealed record InvalidTriggerConfiguration
            : TriggerConfiguration;

        private async Task ExportarTareasCsvAsync(Mensaje msg)
        {
            try
            {
                var csv = await _exportService.ExportJobsToCsvAsync(_getAllJob.GetType().GetProperty("Repository")?.GetValue(_getAllJob) as dynamic ?? 
                    (await _getAllJob.HandleAsync()).Value?.FirstOrDefault()?.GetType().Assembly.GetTypes()
                    .FirstOrDefault(t => t.Name == "JobRepository")?.GetProperty("Context")?.GetValue(null), 
                    CancellationToken.None);
                
                _enviar("exportarCsv", new { contenido = csv, nombreArchivo = $"tareas_{DateTime.Now:yyyyMMdd_HHmmss}.csv" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error exportando tareas a CSV");
                _enviar("tareaError", new { mensaje = "Error al exportar: " + ex.Message });
            }
        }
    }
}