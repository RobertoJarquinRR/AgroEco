using System;
using System.Collections.Generic;
using System.Text;
using AgroEco.UI.Cultivos;

namespace AgroEco.UI.Clases
{
    public class Plagas
    {
        public int Id { get; set; }
        public string? Nombre { get; set; } = "";
        public string? NameCientifico { get; set; } = "";
        public string? Riesgo { get; set; } = "Medio"; //Bajo, Medio, Alto"
        public string? Descripcion { get; set; } = "";
        public List<CultivoAfectado> CultivosAfectados { get; set; } = new();
    }
}
