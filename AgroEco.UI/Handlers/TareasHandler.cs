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
        private readonly TriggerEngine _triggerEngine;
        private readonly ITriggerFactory _triggerFactory;
        private readonly IActionFactory _actionFactory;
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
            TriggerEngine triggerEngine,
            ITriggerFactory triggerFactory,
            IActionFactory actionFactory,
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
            _triggerEngine = triggerEngine;
            _triggerFactory = triggerFactory;
            _actionFactory = actionFactory;
            _logger = logger;
            _triggerEngine.JobExecutionCompleted += OnJobExecutionCompleted;
        }

        private void OnJobExecutionCompleted(Job job)
        {
            _ = ObtenerTareasAsync();
        }

        public void Dispose()
        {
            _triggerEngine.JobExecutionCompleted -= OnJobExecutionCompleted;
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

                if (job.Trigger is not DateTimeTrigger dateTimeTrigger
                    || !TryReadTargetTime(
                        dto.TriggerConfig,
                        out DateTimeOffset targetTime))
                {
                    _enviar(
                        "tareaError",
                        new
                        {
                            mensaje = "The 'targetTime' configuration value must be a valid date and time."
                        });
                    return;
                }

                Result triggerUpdateResult = dateTimeTrigger.UpdateConfiguration(
                    new DateTimeTriggerConfiguration(targetTime));
                if (!triggerUpdateResult.Success)
                {
                    _enviar("tareaError", new { mensaje = triggerUpdateResult.Message });
                    return;
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
                    DateTimeStyles.AssumeUniversal,
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
            => new NoOpActionConfiguration();

        private sealed record InvalidTriggerConfiguration
            : TriggerConfiguration;
    }
}