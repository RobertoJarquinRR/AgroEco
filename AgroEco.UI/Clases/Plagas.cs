using System;
using System.Collections.Generic;
using System.Text;
using AgroEco.UI.Cultivos;
using System.Text.Json.Serialization;

namespace AgroEco.UI.Clases
{
    /* 
     * NOTA: LOS JSONPROPERTY NAME SON PARA QUE COINCIDA CON EL JSON QUE MANDA LA GABRIELA 
     * */
    public class Plagas
    {
        public int Id { get; set; }
        public string? Nombre { get; set; } = "";
        [JsonPropertyName("cientifico")]
        public string? NameCientifico { get; set; } = "";
        public string? Riesgo { get; set; } = "Medio"; //Bajo, Medio, Alto"
        [JsonPropertyName("desc")]
        public string? Descripcion { get; set; } = "";
        [JsonPropertyName("favorece")]
        public string? Favorece { get; set; } = "";
        [JsonPropertyName("cultivos")]
        public List<CultivoAfectado> CultivosAfectados { get; set; } = new();
    }
}
