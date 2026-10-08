using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using AgroEco.UI.Clases;
using AgroEco.UI.Cultivos;
using AgroEco.UI.CultivosAfectado;
using AgroEco.UI.Mensajes;
using AgroEco.UI.Mensajeros;
using AgroEco.UI.Servicios;

namespace AgroEco.UI.Handlers
{
    public class PlagasHandler
    {
        private readonly Action<string, object> _enviar;
        private readonly ILogger<PlagasHandler> _logger;
        private readonly List<Plagas> _plagas = new();

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
                case TiposMensaje.ListoPlagas:
                    EnviarPlagas();
                    break;

                case TiposMensaje.ObtenerDetallePlaga:
                    EnviarDetallesPlaga(msg);
                    break;

                case TiposMensaje.ObtenerDetalleCultivo:
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
                desc = plaga.Descripcion,
                favorece = plaga.Favorece
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
                frecuenciaTratamiento = cultivo.FrecuenciaAplicacion,
                prevencion = cultivo.Prevencion
            });
        }

        private void CargarPlagas()
        {
            try
            {
                var lista = JsonContentLoader.Cargar<List<Plagas>>("plagas.json", _logger);
                if (lista is not null)
                {
                    _plagas.AddRange(lista);
                    _logger.LogInformation("Plagas cargadas: {Count}", _plagas.Count);
                }
                else
                {
                    _logger.LogWarning("No se pudieron cargar las plagas desde plagas.json");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error cargando plagas.json");
            }
        }
    }
}