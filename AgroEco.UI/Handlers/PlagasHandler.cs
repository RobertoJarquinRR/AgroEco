using System;
using System.Collections.Generic;
using System.Text.Json;
using System.IO;
using AgroEco.UI.Clases;
using AgroEco.UI.Mensajeros;
using AgroEco.UI.CultivosAfectado;

namespace AgroEco.UI.Handlers
{
    public class PlagasHandler
    {
        private readonly Action<string, object> _enviar;
        private readonly List<Plagas> _plagas = new();
        //permite que Name == name y no sea case sensitive
        private static readonly JsonSerializerOptions _options = new() {
            PropertyNameCaseInsensitive = true
        };


        //constructor 
        public PlagasHandler(Action<string, object> enviar)
        {
            _enviar = enviar;
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
            string ruta = Path.Combine(AppContext.BaseDirectory,
                "..", "..", "..", "..", "Frontend", "public", "data", "plagas.json");

            if (!File.Exists(ruta))
            {
                System.Diagnostics.Debug.WriteLine($"No se encontró plagas.json en: {ruta}");
                return;
            }

            string texto = File.ReadAllText(ruta);
            var lista = JsonSerializer.Deserialize<List<Plagas>>(texto, _options);

            _plagas.AddRange(lista ?? new());
            System.Diagnostics.Debug.WriteLine($"Plagas cargadas: {_plagas.Count}");
        }
    }


}
