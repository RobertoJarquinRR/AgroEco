using Microsoft.Extensions.Logging;
using AgroEco.UI.Mensajeros;

namespace AgroEco.UI.Handlers
{
    public class DashboardHandler
    {
        private readonly Action<string, object> _enviar;
        private readonly ILogger<DashboardHandler> _logger;

        public DashboardHandler(Action<string, object> enviar, ILogger<DashboardHandler> logger)
        {
            _enviar = enviar;
            _logger = logger;
        }

        public void ManejarMensaje(Mensaje msg)
        {
            switch (msg.Type)
            {
                case "ready_dashboard":
                    EnviarResumen();
                    break;
            }
        }

        private void EnviarResumen()
        {
            _enviar("dashboard_resumen", new
            {
                tareasPendientes = 0,
                tareasVencidas = 0,
                alertasPlagas = 0,
                balanceFinanciero = 0m,
                sensoresFueraRango = 0,
                itemsBajoMinimo = 0
            });
        }
    }
}