using Microsoft.Extensions.Logging;
using AgroEco.UI.Mensajeros;

namespace AgroEco.UI.Handlers
{
    public class SensoresHandler
    {
        private readonly Action<string, object> _enviar;
        private readonly ILogger<SensoresHandler> _logger;

        public SensoresHandler(Action<string, object> enviar, ILogger<SensoresHandler> logger)
        {
            _enviar = enviar;
            _logger = logger;
        }

        public void ManejarMensaje(Mensaje msg)
        {
            switch (msg.Type)
            {
                case "ready_sensores":
                    EnviarLecturas();
                    break;
            }
        }

        private void EnviarLecturas()
        {
            _enviar("sensores_lecturas", new List<object>());
        }
    }
}