using Microsoft.Extensions.Logging;
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
            }
        }

        private void EnviarContenidos()
        {
            _enviar("educacion_contenidos", new List<object>());
        }
    }
}