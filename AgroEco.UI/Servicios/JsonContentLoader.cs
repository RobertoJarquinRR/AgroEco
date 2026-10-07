using Microsoft.Extensions.Logging;
using System.IO;
using System.Text.Json;

namespace AgroEco.UI.Servicios;

public static class JsonContentLoader
{
    private static readonly JsonSerializerOptions Opciones = new()
    {
        PropertyNameCaseInsensitive = true,
        ReadCommentHandling = JsonCommentHandling.Skip
    };

    private static string CarpetaDatos =>
        Path.Combine(AppContext.BaseDirectory, "Data");

    public static T? Cargar<T>(string archivo, ILogger logger)
    {
        try
        {
            var ruta = Path.Combine(CarpetaDatos, archivo);
            if (!File.Exists(ruta))
            {
                logger.LogWarning("Archivo no encontrado: {Ruta}", ruta);
                return default;
            }

            var json = File.ReadAllText(ruta);
            return JsonSerializer.Deserialize<T>(json, Opciones);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error cargando {Archivo}", archivo);
            return default;
        }
    }
}