using System;

namespace AgroEco.UI.Clases
{
    public class Insumo
    {
        public long Id { get; set; }
        public string Nombre { get; set; } = "";
        public string Categoria { get; set; } = "";
        public string Cultivo { get; set; } = "";
        public decimal Cantidad { get; set; }
        public string Unidad { get; set; } = "";
        public decimal StockMin { get; set; }
        public DateOnly? Caducidad { get; set; }
        public string Finca { get; set; } = "";
        public string Descripcion { get; set; } = "";
        public DateOnly FechaCreacion { get; set; } = DateOnly.FromDateTime(DateTime.Today);
    }
}