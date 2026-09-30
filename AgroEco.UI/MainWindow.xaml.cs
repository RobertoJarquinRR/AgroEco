using Microsoft.Web.WebView2.Core;
using System;
using System.IO;
using System.Windows;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Data.Common;

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

                case "obtenerDatosPoda":
                    int idPoda = 1;
                    if(mensaje.Payload.ValueKind == JsonValueKind.Number)
                    {
                        idPoda = mensaje.Payload.GetInt32();
                    }
                    else if (mensaje.Payload.ValueKind == JsonValueKind.String && int.TryParse(mensaje.Payload.GetString(), out int idParseado))
                    {
                        idPoda = idParseado;
                    }
                    EnviarInfoPoda(idPoda);
                break;

                case "obetenerAlturaMaxima":
                double distancia = 5 ;
                if (mensaje.Payload.ValueKind == JsonValueKind.Number)
                    {
                        distancia = mensaje.Payload.GetDouble();
                    }
                    EnviarAlturaMaxima(distancia);
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

        //metodo de enviarinfopoda
        public void EnviarInfoPoda(int idpoda)
        {
            //datos mapeados de las propiedades que usa la funcion mostrarinfopoda en el js
            
            var datospoda = new
            {
                id = idpoda,
                nombre = idpoda == 1 ? "poda de formacion" : "poda de matenimiento",
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
            EnviarAJS("infopoda", datospoda);
        }
        //metodo de enviaralturamaxima
        public void EnviarAlturaMaxima(Double distancia)
        {
            //calculos de altura maxima basada en la distancia de siembra
            double alturaCalculada = Math.Round(distancia * 0.65,2);
            EnviarAJS("alturaMaxima", alturaCalculada);
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