using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AgroEco.Core.Jobs;
using AgroEco.Core.Jobs.Persistence;
using AgroEco.Core.Triggers;
using AgroEco.Core.Triggers.Implementations;
using AgroEco.Core.Jobs.Actions;
using CoreAction = AgroEco.Core.Jobs.Actions.Action;
using AgroEco.Core.Jobs.Actions.Implementations;
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
        private readonly GetByIdJob _getByIdJob;
        private readonly UpdateJob _updateJob;
        private readonly DeleteJob _deleteJob;
        private readonly JobEngine _jobEngine;
        private readonly ILogger<TareasHandler> _logger;

        public TareasHandler(
            Action<string, object> enviar,
            CreateJob createJob,
            GetAllJob getAllJob,
            GetRunningJobs getRunningJobs,
            GetByIdJob getByIdJob,
            UpdateJob updateJob,
            DeleteJob deleteJob,
            JobEngine jobEngine,
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
            _logger = logger;
        }

        public void ManejarMensaje(Mensaje msg)
        {
            _ = msg.Type switch
            {
                "obtenerTareas" => ObtenerTareasAsync(),
                "crearTarea" => CrearTareaAsync(msg),
                "actualizarTarea" => ActualizarTareaAsync(msg),
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

                var trigger = new DateTimeTrigger($"Trigger_{dto.Nombre}", DateTimeOffset.Parse(dto.FechaLimite));
                var action = new ActionTest($"Accion_{dto.Nombre}", Status.Created);

                var result = await _createJob.HandleAsync(
                    dto.Nombre,
                    dto.Descripcion,
                    dto.Prioridad switch { "alta" => 1, "media" => 2, _ => 3 },
                    new List<CoreAction> { action },
                    trigger);

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
                    DateTime.Parse(dto.FechaLimite));

                if (!updateResult.Success)
                {
                    _enviar("tareaError", new { mensaje = updateResult.Message });
                    return;
                }

                if (job.Trigger is DateTimeTrigger dtTrigger)
                {
                    dtTrigger.TargetTime = DateTimeOffset.Parse(dto.FechaLimite);
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

        private object MappearJobATarea(Job job, bool isRunning)
        {
            var estado = job.Status switch
            {
                Status.Succeeded => "completada",
                Status.Faulted => "vencida",
                Status.Canceled => "cancelada",
                Status.Running => "progreso",
                Status.Enqueued => "pendiente",
                Status.Created => "pendiente",
                _ => "pendiente"
            };

            if (estado == "pendiente" && job.Date.HasValue)
            {
                var dias = (job.Date.Value.Date - DateTime.Today).Days;
                if (dias < 0) estado = "vencida";
                else if (dias <= 2) estado = "porVencer";
            }

            return new
            {
                id = job.Id,
                nombre = job.Name,
                descripcion = job.Description ?? "",
                asignado = "Sistema",
                prioridad = job.Priority switch { 1 => "alta", 2 => "media", _ => "baja" },
                fechaLimite = job.Date?.ToString("dd/MM/yyyy") ?? "",
                fechaLimiteISO = job.Date?.ToString("yyyy-MM-dd") ?? "",
                estado = estado
            };
        }

        private record CrearTareaDto(
            string Nombre,
            string Descripcion,
            string Asignado,
            string Prioridad,
            string FechaLimite,
            string Estado);

        private record ActualizarTareaDto(
            int Id,
            string Nombre,
            string Descripcion,
            string Asignado,
            string Prioridad,
            string FechaLimite,
            string Estado);

        private record EliminarTareaDto(int Id);

        private record EjecutarTareaDto(int Id);
    }
}