using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace AgroEco.Core.Educacion;

public sealed class EducacionData
{
    [JsonPropertyName("tiposPoda")]
    public List<TipoPoda> TiposPoda { get; set; } = new();

    [JsonPropertyName("cultivos")]
    public List<Cultivo> Cultivos { get; set; } = new();

    [JsonPropertyName("podas")]
    public List<Poda> Podas { get; set; } = new();

    [JsonPropertyName("etapas")]
    public List<Etapa> Etapas { get; set; } = new();

    [JsonPropertyName("bioinsumos")]
    public List<Bioinsumo> Bioinsumos { get; set; } = new();
}

public sealed class TipoPoda
{
    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("nombre")]
    public string Nombre { get; set; } = "";
}

public sealed class Cultivo
{
    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("nombre")]
    public string Nombre { get; set; } = "";

    [JsonPropertyName("cientifico")]
    public string Cientifico { get; set; } = "";

    [JsonPropertyName("icono")]
    public string Icono { get; set; } = "";
}

public sealed class Poda
{
    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("cultivoIds")]
    public List<int> CultivoIds { get; set; } = new();

    [JsonPropertyName("tipoId")]
    public int TipoId { get; set; }

    [JsonPropertyName("nombre")]
    public string Nombre { get; set; } = "";

    [JsonPropertyName("fase")]
    public string Fase { get; set; } = "";

    [JsonPropertyName("objetivo")]
    public string Objetivo { get; set; } = "";

    [JsonPropertyName("procedimiento")]
    public List<string> Procedimiento { get; set; } = new();

    [JsonPropertyName("justificacion")]
    public string Justificacion { get; set; } = "";
}

public sealed class Etapa
{
    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("cultivoIds")]
    public List<int> CultivoIds { get; set; } = new();

    [JsonPropertyName("nombre")]
    public string Nombre { get; set; } = "";

    [JsonPropertyName("duracion")]
    public string Duracion { get; set; } = "";

    [JsonPropertyName("descripcion")]
    public string Descripcion { get; set; } = "";

    [JsonPropertyName("insumos")]
    public List<InsumoEtapa> Insumos { get; set; } = new();
}

public sealed class InsumoEtapa
{
    [JsonPropertyName("dia")]
    public int Dia { get; set; }

    [JsonPropertyName("nombre")]
    public string Nombre { get; set; } = "";

    [JsonPropertyName("dosis")]
    public string Dosis { get; set; } = "";
}

public sealed class Bioinsumo
{
    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("categoriaId")]
    public int CategoriaId { get; set; }

    [JsonPropertyName("categoria")]
    public string Categoria { get; set; } = "";

    [JsonPropertyName("nombre")]
    public string Nombre { get; set; } = "";

    [JsonPropertyName("descripcion")]
    public string Descripcion { get; set; } = "";

    [JsonPropertyName("ingredientes")]
    public List<IngredienteBioinsumo> Ingredientes { get; set; } = new();

    [JsonPropertyName("procedimiento")]
    public List<string> Procedimiento { get; set; } = new();

    [JsonPropertyName("tiempo")]
    public string Tiempo { get; set; } = "";

    [JsonPropertyName("dosis")]
    public string Dosis { get; set; } = "";

    [JsonPropertyName("cultivoIds")]
    public List<int> CultivoIds { get; set; } = new();
}

public sealed class IngredienteBioinsumo
{
    [JsonPropertyName("ingrediente")]
    public string Ingrediente { get; set; } = "";

    [JsonPropertyName("cantidad")]
    public string Cantidad { get; set; } = "";
}