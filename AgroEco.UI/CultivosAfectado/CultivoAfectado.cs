using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace AgroEco.UI.Cultivos
{
    public class CultivoAfectado
    {
        [JsonPropertyName("idCultivo")]
        public int Id { get; set; }

        [JsonPropertyName("nombreCultivo")]
        public string? NombreCultivo { get; set; }

        [JsonPropertyName("comoIdentificar")]
        public string? ComoIdentificarPlaga { get; set; }

        [JsonPropertyName("pasosIdentificacion")]
        public List<string> PasosIdentificacion { get; set; } = new();

        [JsonPropertyName("formulaTratamiento")]
        public string? FormulaTratamiento { get; set; }

        [JsonPropertyName("dosisPor20Litros")]
        public string? DosisRecomendada { get; set; }

        [JsonPropertyName("frecuenciaTratamiento")]
        public string? FrecuenciaAplicacion { get; set; }

        [JsonPropertyName("prevencion")]
        public List<string> Prevencion { get; set; } = new();
    }
}
