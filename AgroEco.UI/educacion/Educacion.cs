using System;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace AgroEco.educacion
{
    public class EducacionBridge
    {
        private string _jsonFilePath;

        public EducacionBridge(string jsonFilePath)
        {
            _jsonFilePath = jsonFilePath;
        }

        public string ObtenerDatosEducacion()
        {
            try
            {
                if (File.Exists(_jsonFilePath))
                {
                    return File.ReadAllText(_jsonFilePath);
                }
                return JsonSerializer.Serialize(new { error = "Archivo JSON no encontrado." });
            }
            catch (Exception ex)
            {
                return JsonSerializer.Serialize(new { error = ex.Message });
            }
        }
    }

    public class ModuloEducacion
    {
        [JsonPropertyName("cultivos")]
        public Cultivo[]? Cultivos { get; set; }

        [JsonPropertyName("podas")]
        public Poda[]? Podas { get; set; }

        [JsonPropertyName("bioinsumos")]
        public Bioinsumo[]? Bioinsumos { get; set; }
    }

    public class Cultivo
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("nombre")]
        public string? Nombre { get; set; }

        [JsonPropertyName("icono")]
        public string? Icono { get; set; }

        [JsonPropertyName("etapas")]
        public Etapa[]? Etapas { get; set; }
    }

    public class Etapa
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("nombre")]
        public string? Nombre { get; set; }

        [JsonPropertyName("duracion")]
        public string? Duracion { get; set; }

        [JsonPropertyName("descripcion")]
        public string? Descripcion { get; set; }

        [JsonPropertyName("insumos")]
        public InsumoEtapa[]? Insumos { get; set; }
    }

    public class InsumoEtapa
    {
        [JsonPropertyName("dia")]
        public int Dia { get; set; }

        [JsonPropertyName("nombre")]
        public string? Nombre { get; set; }

        [JsonPropertyName("dosis")]
        public string? Dosis { get; set; }
    }

    public class Poda
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("cultivoIds")]
        public int[]? CultivoIds { get; set; }

        [JsonPropertyName("tipoId")]
        public int TipoId { get; set; }

        [JsonPropertyName("nombre")]
        public string? Nombre { get; set; }

        [JsonPropertyName("fase")]
        public string? Fase { get; set; }

        [JsonPropertyName("objetivo")]
        public string? Objetivo { get; set; }

        [JsonPropertyName("procedimiento")]
        public string[]? Procedimiento { get; set; }

        [JsonPropertyName("justificacion")]
        public string? Justificacion { get; set; }
    }

    public class Bioinsumo
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("nombre")]
        public string? Nombre { get; set; }

        [JsonPropertyName("categoriaId")]
        public int CategoriaId { get; set; }

        [JsonPropertyName("categoria")]
        public string? Categoria { get; set; }

        [JsonPropertyName("descripcion")]
        public string? Descripcion { get; set; }

        [JsonPropertyName("ingredientes")]
        public Ingrediente[]? Ingredientes { get; set; }

        [JsonPropertyName("procedimiento")]
        public string[]? Procedimiento { get; set; }

        [JsonPropertyName("tiempo")]
        public string? Tiempo { get; set; }

        [JsonPropertyName("dosis")]
        public string? Dosis { get; set; }

        [JsonPropertyName("cultivosIds")]
        public int[]? CultivosIds { get; set; }
    }

    public class Ingrediente
    {
        [JsonPropertyName("ingrediente")]
        public string? NombreIngrediente { get; set; }

        [JsonPropertyName("cantidad")]
        public string? Cantidad { get; set; }
    }
}