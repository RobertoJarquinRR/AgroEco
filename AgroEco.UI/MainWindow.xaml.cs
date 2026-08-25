using Microsoft.Web.WebView2.Core;
using System;
using System.IO;
using System.Windows;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace AgroEco.UI
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            InitializeAsync();
        }

        private async void InitializeAsync()
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
                CoreWebView2HostResourceAccessKind.Allow
            );
            webView.Source = new Uri("http://app.AgroEco/index.html");
#endif
            this.WindowState = WindowState.Maximized;
        }
        

        private void OnWebMessageReceived(object? sender, CoreWebView2WebMessageReceivedEventArgs e)
        {
            string json = e.WebMessageAsJson;
            var opciones = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            var mensaje = JsonSerializer.Deserialize<Mensaje>(json, opciones);

            if (mensaje is null || string.IsNullOrEmpty(mensaje.Type))
            {
                return;
            }

            switch (mensaje.Type)
            {
                case "obtenerTareas":
                    EnviarTareas();
                    break;

                default:
                    System.Diagnostics.Debug.WriteLine($"Tipo de mensaje sin manejar: {mensaje.Type}");
                    break;
            }

        }
        public void EnviarTareas()
        {
            var tareas = new[]
            {
                new
                {
                    id = 1,
                    nombre = "Regar el cultivo de café",
                    descripcion = "Riego por goteo en el lote 3, revisar presión de mangueras.",
                    asignado = "Juan Pérez",
                    prioridad = "alta",
                    fechaLimite = "23 de ago de 2026",
                    estado = "vencida"

                }
            };
            EnviarAJS("tareasCargadas", tareas);
        }

        private void EnviarAJS(string type, object payload)
        {
            var msg = new { type, payload };
            string json = JsonSerializer.Serialize(msg);
            webView.CoreWebView2.PostWebMessageAsJson(json);
        }
    }

    //aqui la voy a poner ojo es de prueba despues e acomoda
    public class Mensaje
    {
        
        [JsonPropertyName("type")]
        public string Type { get; set; } = string.Empty;
        [JsonPropertyName("payload")]
        public JsonElement Payload { get; set; }

        
    }
}