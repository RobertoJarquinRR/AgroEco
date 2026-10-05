using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using AgroEco.UI.Clases;
using AgroEco.UI.Mensajeros;

namespace AgroEco.UI.Handlers
{
    public class InventarioHandler
    {
        private readonly Action<string, object> _enviar;
        private readonly ILogger<InventarioHandler> _logger;
        private readonly List<Insumo> _insumos = new();
        private long _siguienteId = 1;

        public InventarioHandler(Action<string, object> enviar, ILogger<InventarioHandler> logger)
        {
            _enviar = enviar;
            _logger = logger;
        }

        public void ManejarMensaje(Mensaje msg)
        {
            switch (msg.Type)
            {
                case "ready_inventario":
                    EnviarFincas();
                    EnviarInsumos();
                    break;

                case "crearInsumo":
                    CrearInsumo(msg);
                    break;

                case "eliminarInsumo":
                    EliminarInsumo(msg);
                    break;
            }
        }

        private void CrearInsumo(Mensaje msg)
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

                var insumo = new Insumo
                {
                    Id = _siguienteId++,
                    Nombre = dto.Nombre,
                    Categoria = dto.Categoria,
                    Cultivo = dto.Cultivo,
                    Cantidad = dto.Cantidad,
                    Unidad = dto.Unidad,
                    StockMin = dto.StockMin,
                    Caducidad = caducidad,
                    Finca = dto.Finca,
                    Descripcion = dto.Descripcion
                };

                _insumos.Add(insumo);
                _logger.LogInformation("Insumo creado: {Nombre} (Id: {Id})", insumo.Nombre, insumo.Id);

                EnviarInsumos();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creando insumo");
                _enviar("inventarioError", new { mensaje = ex.Message });
            }
        }

        private void EliminarInsumo(Mensaje msg)
        {
            try
            {
                var dto = msg.LeerPayload<EliminarInsumoDto>();
                if (dto == null || dto.Id <= 0)
                {
                    _enviar("inventarioError", new { mensaje = "ID inválido" });
                    return;
                }

                var insumo = _insumos.FirstOrDefault(i => i.Id == dto.Id);
                if (insumo == null)
                {
                    _enviar("inventarioError", new { mensaje = "Insumo no encontrado" });
                    return;
                }

                _insumos.Remove(insumo);
                _logger.LogInformation("Insumo eliminado: {Nombre} (Id: {Id})", insumo.Nombre, insumo.Id);

                EnviarInsumos();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error eliminando insumo");
                _enviar("inventarioError", new { mensaje = ex.Message });
            }
        }

        private void EnviarInsumos()
        {
            var lista = _insumos.Select(i => new
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

        private record EliminarInsumoDto(long Id);
    }
}