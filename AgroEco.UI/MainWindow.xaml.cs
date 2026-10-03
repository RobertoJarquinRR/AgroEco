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

                case "ready_plagas":
                    enviarPlagas();
                    break;

                case "obtenerDetallesPlaga":
                    EnviarDatosPlaga(mensaje.Payload);
                    EnviarCultivosPlaga(mensaje.Payload);
                    break;

                case "obtenerDetallesCultivo":
                    EnviarDetallesCultivoPlaga(mensaje.Payload);
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


        //datos del json de plagas
        private JsonDocument? CargarPlagasJson()
        {
            string rutaJson = Path.Combine(
                AppContext.BaseDirectory,
                "..",
                "..",
                "..",
                "..",
                "Frontend",
                "public",
                "data",
                "plagas.json"
            );

            if (!File.Exists(rutaJson))
            {
                return null;
            }

            string json = File.ReadAllText(rutaJson);

            return JsonDocument.Parse(json);
        }

        //enviar datos de modulo plagas
        private void enviarPlagas()
        {
            using JsonDocument? documento = CargarPlagasJson();

            if (documento == null)
            {
                EnviarAJS("cargar_plagas", Array.Empty<object>());
                return;
            }

            var plagas = documento.RootElement
                .EnumerateArray()
                .Select(plaga => new
                {
                    id = plaga.GetProperty("id").GetInt32(),
                    nombre = plaga.GetProperty("nombre").GetString(),
                    cientifico = plaga.GetProperty("cientifico").GetString()
                })
                .ToList();

            EnviarAJS("cargar_plagas", plagas);
        }

        private void EnviarDatosPlaga(JsonElement payload)
        {
            int idPlaga = payload.GetInt32();

            using JsonDocument? documento = CargarPlagasJson();

            if (documento == null)
            {
                EnviarAJS("cargar_detalles_plaga", Array.Empty<object>());
                return;
            }

            JsonElement plagaEncontrada = documento.RootElement
                .EnumerateArray()
                .FirstOrDefault(plaga =>
                    plaga.GetProperty("id").GetInt32() == idPlaga
                );

            if (plagaEncontrada.ValueKind == JsonValueKind.Undefined)
            {
                EnviarAJS("cargar_detalles_plaga", null);
                return;
            }

            var datos = new
            {
                id = plagaEncontrada.GetProperty("id").GetInt32(),
                nombre = plagaEncontrada.GetProperty("nombre").GetString(),
                cientifico = plagaEncontrada.GetProperty("cientifico").GetString(),
                riesgo = plagaEncontrada.GetProperty("riesgo").GetString(),
                desc = plagaEncontrada.GetProperty("desc").GetString()
            };

            EnviarAJS("cargar_detalles_plaga", datos);
        }

        private void EnviarCultivosPlaga(JsonElement payload)
        {
            int idPlaga = payload.GetInt32();

            using JsonDocument? documento = CargarPlagasJson();

            if (documento == null)
            {
                EnviarAJS("cargar_cultivos", Array.Empty<object>());
                return;
            }

            JsonElement plagaEncontrada = documento.RootElement
                .EnumerateArray()
                .FirstOrDefault(plaga =>
                    plaga.GetProperty("id").GetInt32() == idPlaga
                );

            if (plagaEncontrada.ValueKind == JsonValueKind.Undefined)
            {
                EnviarAJS("cargar_cultivos", Array.Empty<object>());
                return;
            }

            var cultivos = plagaEncontrada
                .GetProperty("cultivos")
                .EnumerateArray()
                .Select(cultivo => new
                {
                    idCultivo = cultivo.GetProperty("idCultivo").GetInt32(),
                    nombreCultivo = cultivo.GetProperty("nombreCultivo").GetString()
                })
                .ToList();

            EnviarAJS("cargar_cultivos", cultivos);
        }

        private void EnviarDetallesCultivoPlaga(JsonElement payload)
        {
            int idPlaga = payload.GetProperty("idPlaga").GetInt32();
            int idCultivo = payload.GetProperty("idCultivo").GetInt32();

            using JsonDocument? documento = CargarPlagasJson();

            if (documento == null)
            {
                EnviarAJS("cargar_detalles_cultivo", Array.Empty<object>());
                return;
            }

            JsonElement plagaEncontrada = documento.RootElement
                .EnumerateArray()
                .FirstOrDefault(plaga =>
                    plaga.GetProperty("id").GetInt32() == idPlaga
                );

            if (plagaEncontrada.ValueKind == JsonValueKind.Undefined)
            {
                EnviarAJS("cargar_detalles_cultivo", null);
                return;
            }

            JsonElement cultivoEncontrado = plagaEncontrada
                .GetProperty("cultivos")
                .EnumerateArray()
                .FirstOrDefault(cultivo =>
                    cultivo.GetProperty("idCultivo").GetInt32() == idCultivo
                );

            if (cultivoEncontrado.ValueKind == JsonValueKind.Undefined)
            {
                EnviarAJS("cargar_detalles_cultivo", null);
                return;
            }

            var datos = new
            {
                idCultivo = cultivoEncontrado.GetProperty("idCultivo").GetInt32(),
                nombreCultivo = cultivoEncontrado.GetProperty("nombreCultivo").GetString(),
                comoIdentificar = cultivoEncontrado.GetProperty("comoIdentificar").GetString(),
                pasosIdentificacion = cultivoEncontrado
                    .GetProperty("pasosIdentificacion")
                    .EnumerateArray()
                    .Select(paso => paso.GetString())
                    .ToList(),
                formulaTratamiento = cultivoEncontrado.GetProperty("formulaTratamiento").GetString(),
                dosisPor20Litros = cultivoEncontrado.GetProperty("dosisPor20Litros").GetString(),
                frecuenciaTratamiento = cultivoEncontrado.GetProperty("frecuenciaTratamiento").GetString()
            };

            EnviarAJS("cargar_detalles_cultivo", datos);
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