using AgroEco.Core.Educacion;
using AgroEco.UI.Mensajes;
using AgroEco.UI.Mensajeros;
using AgroEco.UI.Servicios;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.IO;

namespace AgroEco.UI.Handlers
{
    public class EducacionHandler
    {
        private readonly Action<string, object> _enviar;
        private readonly ILogger<EducacionHandler> _logger;
        private EducacionData? _data;

        public EducacionHandler(Action<string, object> enviar, ILogger<EducacionHandler> logger)
        {
            _enviar = enviar;
            _logger = logger;
            CargarDatos();
        }

        public void ManejarMensaje(Mensaje msg)
        {
            switch (msg.Type)
            {
                case TiposMensaje.ListoEducacion:
                    EnviarContenidos();
                    break;

                case TiposMensaje.ObtenerDatosPoda:
                    ObtenerDatosPoda(msg);
                    break;

                case TiposMensaje.ObtenerAlturaMaxima:
                    ObtenerAlturaMaxima(msg);
                    break;

                case TiposMensaje.ObtenerBioinsumos:
                case TiposMensaje.ObtenerDatosCategoriaBioInsumo:
                    ObtenerBioinsumos(msg);
                    break;

                case TiposMensaje.ObtenerDetalleBioinsumo:
                case TiposMensaje.ObtenerDatosBioinsumo:
                    ObtenerDetalleBioinsumo(msg);
                    break;

                case TiposMensaje.ObtenerDatosEtapa:
                    ObtenerDatosEtapa(msg);
                    break;
            }
        }

        private void CargarDatos()
        {
            try
            {
                _data = JsonContentLoader.Cargar<EducacionData>("educacion.json", _logger);
                if (_data is not null)
                {
                    _logger.LogInformation("Datos de educación cargados: {Cultivos} cultivos, {Podas} podas, {Etapas} etapas, {Bioinsumos} bioinsumos",
                        _data.Cultivos.Count, _data.Podas.Count, _data.Etapas.Count, _data.Bioinsumos.Count);
                }
                else
                {
                    _logger.LogWarning("No se pudieron cargar los datos de educacion.json");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error cargando educacion.json");
            }
        }
        private static readonly string RutaJson = Path.Combine(AppContext.BaseDirectory, "Data", "educacion.json");

       private void EnviarContenidos()
{
    try
    {
        if (!File.Exists(RutaJson))
        {
            _logger.LogError("No se encontró {Ruta}", RutaJson);
            return;
        }

        var contenido = JsonSerializer.Deserialize<JsonElement>(File.ReadAllText(RutaJson));
        _enviar("educacion_contenidos", contenido);
        }
        catch (Exception ex)
        {
        _logger.LogError(ex, "Error al enviar contenidos de educación");
        }
    }

        private void ObtenerDatosPoda(Mensaje msg)
        {
            int idPoda = 1;
            if (msg.Payload.ValueKind == JsonValueKind.Number)
            {
                idPoda = msg.Payload.GetInt32();
            }
            else if (msg.Payload.ValueKind == JsonValueKind.String && int.TryParse(msg.Payload.GetString(), out int idParseado))
            {
                idPoda = idParseado;
            }
            EnviarInfoPoda(idPoda);
        }

        private void EnviarInfoPoda(int idPoda)
        {
            if (_data is null) return;

            var poda = _data.Podas.FirstOrDefault(p => p.Id == idPoda);
            if (poda is null) return;

            var tipoPoda = _data.TiposPoda.FirstOrDefault(t => t.Id == poda.TipoId);
            var cultivos = _data.Cultivos.Where(c => poda.CultivoIds.Contains(c.Id)).ToList();

            var datosPoda = new
            {
                id = poda.Id,
                nombre = poda.Nombre,
                tipo = tipoPoda?.Nombre ?? "",
                fase = poda.Fase,
                objetivo = poda.Objetivo,
                justificacion = poda.Justificacion,
                procedimiento = poda.Procedimiento,
                cultivos = cultivos.Select(c => new { c.Id, c.Nombre, c.Cientifico })
            };

            _enviar("infoPoda", datosPoda);
        }

        private void ObtenerAlturaMaxima(Mensaje msg)
        {
            double distancia = 5;
            if (msg.Payload.ValueKind == JsonValueKind.Number)
            {
                distancia = msg.Payload.GetDouble();
            }
            else if (msg.Payload.ValueKind == JsonValueKind.String && double.TryParse(msg.Payload.GetString(), out double distParseada))
            {
                distancia = distParseada;
            }
            EnviarAlturaMaxima(distancia);
        }

        private void EnviarAlturaMaxima(double distancia)
        {
            double alturaCalculada = Math.Round(distancia * 0.65, 2);
            _enviar("alturaMaxima", alturaCalculada);
        }

        private void ObtenerBioinsumos(Mensaje msg)
        {
            if (_data is null) return;

            int? categoriaId = null;
            if (msg.Payload.ValueKind == JsonValueKind.Number)
            {
                categoriaId = msg.Payload.GetInt32();
            }
            else if (msg.Payload.ValueKind == JsonValueKind.String && int.TryParse(msg.Payload.GetString(), out int catParseado))
            {
                categoriaId = catParseado;
            }

            var bioinsumos = _data.Bioinsumos.AsEnumerable();
            if (categoriaId.HasValue)
            {
                bioinsumos = bioinsumos.Where(b => b.CategoriaId == categoriaId.Value);
            }

            var lista = bioinsumos.Select(b => new
            {
                b.Id,
                b.Nombre,
                b.Categoria,
                b.CategoriaId,
                b.Descripcion,
                b.Tiempo,
                b.Dosis,
                b.CultivoIds
            }).ToList();

            _enviar("listaBioinsumos", lista);
        }

        private void ObtenerDetalleBioinsumo(Mensaje msg)
        {
            if (_data is null) return;

            int idBioinsumo = 0;
            if (msg.Payload.ValueKind == JsonValueKind.Number)
            {
                idBioinsumo = msg.Payload.GetInt32();
            }
            else if (msg.Payload.ValueKind == JsonValueKind.String && int.TryParse(msg.Payload.GetString(), out int idParseado))
            {
                idBioinsumo = idParseado;
            }

            var bioinsumo = _data.Bioinsumos.FirstOrDefault(b => b.Id == idBioinsumo);
            if (bioinsumo is null) return;

            var cultivos = _data.Cultivos.Where(c => bioinsumo.CultivoIds.Contains(c.Id)).ToList();

            var detalle = new
            {
                bioinsumo.Id,
                bioinsumo.Nombre,
                bioinsumo.Categoria,
                bioinsumo.CategoriaId,
                bioinsumo.Descripcion,
                bioinsumo.Ingredientes,
                bioinsumo.Procedimiento,
                bioinsumo.Tiempo,
                bioinsumo.Dosis,
                Cultivos = cultivos.Select(c => new { c.Id, c.Nombre, c.Cientifico })
            };

            _enviar("cargar_detalle_bioinsumo", detalle);
        }

        private void ObtenerDatosEtapa(Mensaje msg)
        {
            if (_data is null) return;

            int idEtapa = 0;
            int? cultivoId = null;

            if (msg.Payload.ValueKind == JsonValueKind.Object)
            {
                var payload = msg.Payload;
                if (payload.TryGetProperty("idEtapa", out var idProp) && idProp.ValueKind == JsonValueKind.Number)
                {
                    idEtapa = idProp.GetInt32();
                }
                if (payload.TryGetProperty("cultivoId", out var cultivoProp) && cultivoProp.ValueKind == JsonValueKind.Number)
                {
                    cultivoId = cultivoProp.GetInt32();
                }
            }

            var etapas = _data.Etapas.AsEnumerable();
            if (idEtapa > 0)
            {
                etapas = etapas.Where(e => e.Id == idEtapa);
            }
            if (cultivoId.HasValue)
            {
                etapas = etapas.Where(e => e.CultivoIds.Contains(cultivoId.Value));
            }

            var lista = etapas.Select(e => new
            {
                e.Id,
                e.Nombre,
                e.Duracion,
                e.Descripcion,
                e.CultivoIds,
                e.Insumos
            }).ToList();

            _enviar("cargar_etapas", lista);
        }
    }
}