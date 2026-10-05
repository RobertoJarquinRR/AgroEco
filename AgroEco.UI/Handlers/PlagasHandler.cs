using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using AgroEco.UI.Clases;
using AgroEco.UI.CultivosAfectado;
using AgroEco.UI.Mensajeros;

namespace AgroEco.UI.Handlers
{
    public class PlagasHandler
    {
        private readonly Action<string, object> _enviar;
        private readonly ILogger<PlagasHandler> _logger;
        private readonly List<Plagas> _plagas = new();
        private static readonly JsonSerializerOptions _options = new() { PropertyNameCaseInsensitive = true };

        public PlagasHandler(Action<string, object> enviar, ILogger<PlagasHandler> logger)
        {
            _enviar = enviar;
            _logger = logger;
            CargarPlagas();
        }

        public void ManejarMensaje(Mensaje msg)
        {
            switch (msg.Type)
            {
                case "ready_plagas":
                    EnviarPlagas();
                    break;

                case "obtenerDetallesPlaga":
                    EnviarDetallesPlaga(msg);
                    break;

                case "obtenerDetallesCultivo":
                    EnviarDetallesCultivo(msg);
                    break;
            }
        }

        private void EnviarPlagas()
        {
            var lista = _plagas.Select(p => new
            {
                id = p.Id,
                nombre = p.Nombre,
                cientifico = p.NameCientifico
            }).ToList();

            _enviar("cargar_plagas", lista);
        }

        private void EnviarDetallesPlaga(Mensaje msg)
        {
            int idPlaga = msg.LeerPayload<int>();

            var plaga = _plagas.FirstOrDefault(p => p.Id == idPlaga);
            if (plaga is null)
            {
                return;
            }

            _enviar("cargar_detalles_plaga", new
            {
                id = plaga.Id,
                nombre = plaga.Nombre,
                cientifico = plaga.NameCientifico,
                riesgo = plaga.Riesgo,
                desc = plaga.Descripcion
            });

            var cultivos = plaga.CultivosAfectados.Select(c => new
            {
                idCultivo = c.Id,
                nombreCultivo = c.NombreCultivo
            }).ToList();

            _enviar("cargar_cultivos", cultivos);
        }

        private void EnviarDetallesCultivo(Mensaje msg)
        {
            var pedido = msg.LeerPayload<DetallesCultivosAfectados>();
            if (pedido is null) return;

            var plaga = _plagas.FirstOrDefault(p => p.Id == pedido.IdPlaga);
            var cultivo = plaga?.CultivosAfectados.FirstOrDefault(c => c.Id == pedido.IdCultivo);

            if (cultivo is null)
            {
                return;
            }

            _enviar("cargar_detalles_cultivo", new
            {
                idCultivo = cultivo.Id,
                nombreCultivo = cultivo.NombreCultivo,
                comoIdentificar = cultivo.ComoIdentificarPlaga,
                pasosIdentificacion = cultivo.PasosIdentificacion,
                formulaTratamiento = cultivo.FormulaTratamiento,
                dosisPor20Litros = cultivo.DosisRecomendada,
                frecuenciaTratamiento = cultivo.FrecuenciaAplicacion
            });
        }

        private void CargarPlagas()
        {
            try
            {
                string ruta = Path.Combine(AppContext.BaseDirectory,
                    "..", "..", "..", "..", "Frontend", "public", "data", "plagas.json");

                if (!File.Exists(ruta))
                {
                    _logger.LogWarning("No se encontró plagas.json en: {Ruta}", ruta);
                    return;
                }

                string texto = File.ReadAllText(ruta);
                var lista = JsonSerializer.Deserialize<List<Plagas>>(texto, _options);

                _plagas.AddRange(lista ?? new());
                _logger.LogInformation("Plagas cargadas: {Count}", _plagas.Count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error cargando plagas.json");
            }
        }
    }
}