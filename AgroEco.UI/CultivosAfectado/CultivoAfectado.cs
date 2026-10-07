using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace AgroEco.UI.Cultivos
{
    //esta clase es para el tratamiento de cultivos me refiero en el apartado
    //de plagas donde sale el cultivo con sus plagas y como enfrentarlas

    public class CultivoAfectado
    {
        [JsonPropertyName("idCultivo")]
        public int Id { get; set; }
        //este no tiene pero por si acaso dejo nota este tiene diferencia de mayuscula en caso de error
        public string? NombreCultivo { get; set; }
        [JsonPropertyName("comoIdentificar")]
        public string? ComoIdentificarPlaga { get; set; }

        public List<string> PasosIdentificacion { get; set; } = new List<string>(); //lo crea lisrto para usar el list

        public string? FormulaTratamiento { get; set; }
        [JsonPropertyName("dosisPor20Litros")]
        public string? DosisRecomendada { get; set; }
[JsonPropertyName("frecuenciaTratamiento")]
        public string? FrecuenciaAplicacion { get; set; }
        [JsonPropertyName("prevencion")]
        public string? Prevencion { get; set; } = "";
    }
}
