using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Markup;

namespace AgroEco.UI.Cultivos
{
    //esta clase es para el tratamiento de cultivos me refiero en el apartado
    //de plagas donde sale el cultivo con sus plagas y como enfrentarlas

    public class CultivoAfectado
    {
        public int Id { get; set; }
        public string? NombreCultivo { get; set; }
        public string? ComoIdentificarPlaga { get; set; }
        public List<string> PasosIdentificacion { get; set; } = new List<string>(); //lo crea lisrto para usar el list
        public string? FormulaTratamiento { get; set; }
        public string? DosisRecomendada { get; set; }
        public string? FrecuenciaAplicacion { get; set; }

        public CultivoAfectado(int id, string? nombreCultivo, string? comoIdentificarPlaga, List<string> pasosIdentificacion, string? formulaTratamiento, string? dosisRecomendada, string? frecuenciaAplicacion)
        {
            Id = id;
            NombreCultivo = nombreCultivo;
            ComoIdentificarPlaga = comoIdentificarPlaga;
            PasosIdentificacion = pasosIdentificacion;
            FormulaTratamiento = formulaTratamiento;
            DosisRecomendada = dosisRecomendada;
            FrecuenciaAplicacion = frecuenciaAplicacion;
        }

    }
}
