using Microsoft.Web.WebView2.Core;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Windows;
using System.Text.Json;
using System.Text.Json.Serialization;
using AgroEco.UI.Handlers;
using AgroEco.UI.Alerts;
using AgroEco.UI.Mensajeros;

namespace AgroEco.UI
{
    internal static class JsonOptions
    {
        public static readonly JsonSerializerOptions CamelCase = new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        };
    }

    public partial class MainWindow : Window
    {
        private readonly MsgRouter _router;
        private readonly ILogger<MainWindow> _logger;

        public MainWindow(
            Func<Action<string, object>, TareasHandler> tareasFactory,
            Func<Action<string, object>, PlagasHandler> plagasFactory,
            Func<Action<string, object>, FinanzasHandler> finanzasFactory,
            Func<Action<string, object>, DashboardHandler> dashboardFactory,
            Func<Action<string, object>, SensoresHandler> sensoresFactory,
            Func<Action<string, object>, SensorReadingHandler> sensorReadingFactory,
            Func<Action<string, object>, InventarioHandler> inventarioFactory,
            Func<Action<string, object>, EducacionHandler> educacionFactory,
            Func<Action<string, object>, AlertHandler> alertasFactory,
            ILogger<MainWindow> logger)
        {
            InitializeComponent();
            _logger = logger;

            var tareasHandler = tareasFactory(EnviarAJS);
            var plagasHandler = plagasFactory(EnviarAJS);
            var finanzasHandler = finanzasFactory(EnviarAJS);
            var dashboardHandler = dashboardFactory(EnviarAJS);
            var sensoresHandler = sensoresFactory(EnviarAJS);
            var sensorReadingHandler = sensorReadingFactory(EnviarAJS);
            var inventarioHandler = inventarioFactory(EnviarAJS);
            var educacionHandler = educacionFactory(EnviarAJS);
            var alertasHandler = alertasFactory(EnviarAJS);

            _router = new MsgRouter(
                tareasHandler, plagasHandler, finanzasHandler,
                dashboardHandler, sensoresHandler, sensorReadingHandler, inventarioHandler, educacionHandler, alertasHandler,
                App.ServiceProvider.GetRequiredService<ILogger<MsgRouter>>());

            InitializeAsync();
        }

        private async void InitializeAsync()
        {
            try
            {
                await webView.EnsureCoreWebView2Async(null);

                webView.CoreWebView2.WebMessageReceived += OnWebMessageReceived;

                string webRootFolder = Path.Combine(AppContext.BaseDirectory, "Frontend");
#if DEBUG
                webView.Source = new Uri("http://localhost:5173/");
#else
                webView.CoreWebView2.SetVirtualHostNameToFolderMapping(
                    "app.AgroEco",
                    webRootFolder,
                    CoreWebView2HostResourceAccessKind.Allow);
                webView.Source = new Uri("http://app.AgroEco/index.html");
#endif
                WindowState = WindowState.Maximized;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error inicializando WebView2");
            }
        }

        private void OnWebMessageReceived(object? sender, CoreWebView2WebMessageReceivedEventArgs e)
        {
            try
            {
                string json = e.WebMessageAsJson;
                var opciones = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                var mensaje = JsonSerializer.Deserialize<Mensaje>(json, opciones);

                if (mensaje is null || string.IsNullOrEmpty(mensaje.Type))
                {
                    return;
                }

                if (_router.Enrutar(mensaje)) return;

                _logger.LogWarning("Tipo de mensaje sin manejar: {Type}", mensaje.Type);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error procesando mensaje WebView2");
            }
        }

        private void EnviarAJS(string type, object? payload)
        {
            if (!Dispatcher.CheckAccess())
            {
                _ = Dispatcher.BeginInvoke(
                    new Action(() => EnviarAJS(type, payload)));
                return;
            }

            try
            {
                var msg = new { type, payload };
                string json = JsonSerializer.Serialize(msg, JsonOptions.CamelCase);
                webView.CoreWebView2.PostWebMessageAsJson(json);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error enviando mensaje a JS: {Type}", type);
            }
        }

        public void EnviarTareas() { }
        public void EnviarInfoPoda(int idpoda) { }
        public void EnviarAlturaMaxima(double distancia) { }
    }
}