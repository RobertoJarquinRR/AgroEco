using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AgroEco.Core.Inventario;
using AgroEco.Core.Inventario.Persistence;
using AgroEco.Core.Interfaces;
using AgroEco.UI.Mensajeros;

namespace AgroEco.UI.Handlers
{
    public class InventarioHandler
    {
        private readonly Action<string, object> _enviar;
        private readonly ILogger<InventarioHandler> _logger;
        private readonly GetAllInsumo _getAllInsumo;
        private readonly CreateInsumo _createInsumo;
        private readonly DeleteInsumo _deleteInsumo;
        private readonly GetByIdInsumo _getByIdInsumo;
        private readonly UpdateInsumo _updateInsumo;

        public InventarioHandler(
            Action<string, object> enviar,
            GetAllInsumo getAllInsumo,
            CreateInsumo createInsumo,
            DeleteInsumo deleteInsumo,
            GetByIdInsumo getByIdInsumo,
            UpdateInsumo updateInsumo,
            ILogger<InventarioHandler> logger)
        {
            _enviar = enviar;
            _getAllInsumo = getAllInsumo;
            _createInsumo = createInsumo;
            _deleteInsumo = deleteInsumo;
            _getByIdInsumo = getByIdInsumo;
            _updateInsumo = updateInsumo;
            _logger = logger;
        }

        public async void ManejarMensaje(Mensaje msg)
        {
            switch (msg.Type)
            {
                case "ready_inventario":
                    EnviarFincas();
                    await EnviarInsumosAsync();
                    break;

                case "obtenerInsumos":
                    await EnviarInsumosParaSelectAsync();
                    break;

                case "obtenerFincas":
                    EnviarFincas();
                    break;

                case "crearInsumo":
                    await CrearInsumoAsync(msg);
                    break;

                case "actualizarInsumo":
                    await ActualizarInsumoAsync(msg);
                    break;

                case "eliminarInsumo":
                    await EliminarInsumoAsync(msg);
                    break;
            }
        }

        private async Task EnviarInsumosParaSelectAsync()
        {
            try
            {
                var result = await _getAllInsumo.HandleAsync();
                if (!result.Success || result.Value == null)
                {
                    _enviar("insumos", new { insumos = new List<object>() });
                    return;
                }

                var lista = result.Value.Where(i => i.Cantidad > 0).Select(i => new
                {
                    id = i.Id,
                    nombre = i.Nombre,
                    categoria = i.Categoria,
                    cultivo = i.Cultivo,
                    cantidad = i.Cantidad,
                    unidad = i.Unidad,
                    minimo = i.StockMin,
                    caducidad = i.Caducidad?.ToString("yyyy-MM-dd") ?? "",
                    finca = i.Finca,
                    descripcion = i.Descripcion,
                    fechaCreacion = i.FechaCreacion.ToString("yyyy-MM-dd")
                }).ToList();

                _enviar("insumos", new { insumos = lista });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error enviando insumos para select");
                _enviar("insumos", new { insumos = new List<object>() });
            }
        }

        private async Task CrearInsumoAsync(Mensaje msg)
        {
            try
            {
                var dto = msg.LeerPayload<CrearInsumoDto>();
                if (dto == null || string.IsNullOrWhiteSpace(dto.Nombre))
                {
                    _enviar("inventarioError", new { mensaje = "Nombre requerido" });
                    return;
                }

                DateOnly? caducidad = null;
                if (!string.IsNullOrWhiteSpace(dto.Caducidad) && DateOnly.TryParse(dto.Caducidad, out var parsed))
                {
                    caducidad = parsed;
                }

                var result = await _createInsumo.HandleAsync(
                    dto.Nombre,
                    dto.Categoria,
                    dto.Cultivo,
                    dto.Cantidad,
                    dto.Unidad,
                    dto.StockMin,
                    caducidad,
                    dto.Finca,
                    dto.Descripcion);

                if (result.Success)
                {
                    _logger.LogInformation("Insumo creado: {Nombre} (Id: {Id})", result.Value.Nombre, result.Value.Id);
                    await EnviarInsumosAsync();
                }
                else
                {
                    _enviar("inventarioError", new { mensaje = result.Message });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creando insumo");
                _enviar("inventarioError", new { mensaje = ex.Message });
            }
        }

        private async Task ActualizarInsumoAsync(Mensaje msg)
        {
            try
            {
                var dto = msg.LeerPayload<ActualizarInsumoDto>();
                if (dto == null || dto.Id <= 0)
                {
                    _enviar("inventarioError", new { mensaje = "ID inválido" });
                    return;
                }

                var existingResult = await _getByIdInsumo.HandleAsync(dto.Id);
                if (!existingResult.Success || existingResult.Value == null)
                {
                    _enviar("inventarioError", new { mensaje = "Insumo no encontrado" });
                    return;
                }

                DateOnly? caducidad = null;
                if (!string.IsNullOrWhiteSpace(dto.Caducidad) && DateOnly.TryParse(dto.Caducidad, out var parsed))
                {
                    caducidad = parsed;
                }

                var insumo = existingResult.Value;
                insumo.Nombre = dto.Nombre;
                insumo.Categoria = dto.Categoria;
                insumo.Cultivo = dto.Cultivo;
                insumo.Cantidad = dto.Cantidad;
                insumo.Unidad = dto.Unidad;
                insumo.StockMin = dto.StockMin;
                insumo.Caducidad = caducidad;
                insumo.Finca = dto.Finca;
                insumo.Descripcion = dto.Descripcion;
                insumo.FechaActualizacion = DateOnly.FromDateTime(DateTime.Today);

                var result = await _updateInsumo.HandleAsync(insumo);
                if (result.Success)
                {
                    _logger.LogInformation("Insumo actualizado: {Nombre} (Id: {Id})", insumo.Nombre, insumo.Id);
                    await EnviarInsumosAsync();
                }
                else
                {
                    _enviar("inventarioError", new { mensaje = result.Message });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error actualizando insumo");
                _enviar("inventarioError", new { mensaje = ex.Message });
            }
        }

        private async Task EliminarInsumoAsync(Mensaje msg)
        {
            try
            {
                var dto = msg.LeerPayload<EliminarInsumoDto>();
                if (dto == null || dto.Id <= 0)
                {
                    _enviar("inventarioError", new { mensaje = "ID inválido" });
                    return;
                }

                var result = await _deleteInsumo.HandleAsync(dto.Id);
                if (result.Success)
                {
                    _logger.LogInformation("Insumo eliminado (Id: {Id})", dto.Id);
                    await EnviarInsumosAsync();
                }
                else
                {
                    _enviar("inventarioError", new { mensaje = result.Message });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error eliminando insumo");
                _enviar("inventarioError", new { mensaje = ex.Message });
            }
        }

        private async Task EnviarInsumosAsync()
        {
            try
            {
                var result = await _getAllInsumo.HandleAsync();
                if (!result.Success || result.Value == null)
                {
                    _enviar("listaInsumos", new List<object>());
                    return;
                }

                var lista = result.Value.Select(i => new
                {
                    id = i.Id,
                    nombre = i.Nombre,
                    categoria = i.Categoria,
                    cultivo = i.Cultivo,
                    cantidad = i.Cantidad,
                    unidad = i.Unidad,
                    minimo = i.StockMin,
                    caducidad = i.Caducidad?.ToString("yyyy-MM-dd") ?? "",
                    finca = i.Finca,
                    descripcion = i.Descripcion,
                    fechaCreacion = i.FechaCreacion.ToString("yyyy-MM-dd")
                }).ToList();

                _enviar("listaInsumos", lista);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error enviando insumos");
                _enviar("listaInsumos", new List<object>());
            }
        }

        private void EnviarFincas()
        {
            var fincas = new[]
            {
                new { nombre = "Finca El Paraíso" },
                new { nombre = "Finca La Esperanza" },
                new { nombre = "Finca San José" },
                new { nombre = "Finca Los Laureles" }
            };
            _enviar("listaFincas", fincas);
        }

        private record CrearInsumoDto(
            string Nombre,
            string Categoria,
            string Cultivo,
            decimal Cantidad,
            string Unidad,
            decimal StockMin,
            string Caducidad,
            string Finca,
            string Descripcion);

        private record ActualizarInsumoDto(
            int Id,
            string Nombre,
            string Categoria,
            string Cultivo,
            decimal Cantidad,
            string Unidad,
            decimal StockMin,
            string Caducidad,
            string Finca,
            string Descripcion);

        private record EliminarInsumoDto(int Id);
    }
}