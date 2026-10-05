using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Text.Json;
using AgroEco.UI.Mensajeros;

namespace AgroEco.UI.Handlers
{
    public class EducacionHandler
    {
        private readonly Action<string, object> _enviar;
        private readonly ILogger<EducacionHandler> _logger;

        public EducacionHandler(Action<string, object> enviar, ILogger<EducacionHandler> logger)
        {
            _enviar = enviar;
            _logger = logger;
        }

        public void ManejarMensaje(Mensaje msg)
        {
            switch (msg.Type)
            {
                case "ready_educacion":
                    EnviarContenidos();
                    break;

                case "obtenerDatosPoda":
                    ObtenerDatosPoda(msg);
                    break;

                case "obtenerAlturaMaxima":
                    ObtenerAlturaMaxima(msg);
                    break;
            }
        }

        private void EnviarContenidos()
        {
            _enviar("educacion_contenidos", new List<object>());
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
            var datosPoda = new
            {
                id = idPoda,
                nombre = idPoda == 1 ? "poda de formacion" : "poda de mantenimiento",
                edad = "30-45 dias",
                objetivo = "Orientar el crecimiento de la planta y establecer una estructura adecuada para su desarrollo.",
                justificacion = "La poda de formación permite distribuir mejor el crecimiento de la planta, facilitar el manejo del cultivo y mejorar la exposición de las hojas a la luz.",
                procedimiento = new[]
                {
                    "Identificar el tallo principal y las ramas que se conservarán.",
                    "Eliminar los brotes que crecen hacia el interior de la planta.",
                    "Retirar ramas débiles o mal ubicadas.",
                    "Realizar cortes limpios en ángulo de 45° con una herramienta desinfectada."
                }
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
    }
}