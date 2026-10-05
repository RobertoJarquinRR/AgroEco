using Microsoft.Extensions.Logging;
using AgroEco.UI.Mensajeros;

namespace AgroEco.UI.Handlers
{
    public class InventarioHandler
    {
        private readonly Action<string, object> _enviar;
        private readonly ILogger<InventarioHandler> _logger;

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
                    EnviarItems();
                    break;
            }
        }

        private void EnviarItems()
        {
            _enviar("inventario_items", new List<object>());
        }
    }
}