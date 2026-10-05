using System;
using System.Collections.Generic;
using System.Text.Json;

namespace AgroEco.UI.Clases
{
    public class RegistroFinanciero
    {
        public long Id { get; set; }
        public string? Tipo { get; set; } //ingreso o gasto
        public string? Cultivo { get; set; }
        public string? Categoria { get; set; } = "";
        public decimal Monto { get; set; }
        public DateOnly Fecha { get; set; }
        public string Descripcion { get; set; } = "";


    }
}
