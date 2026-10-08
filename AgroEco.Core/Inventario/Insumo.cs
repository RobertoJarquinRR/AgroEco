using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using AgroEco.Core.Interfaces;

namespace AgroEco.Core.Inventario;

public class Insumo : IEntity
{
    public int Id { get; set; }
    
    [Required]
    [MaxLength(100)]
    public string Nombre { get; set; } = "";
    
    [MaxLength(50)]
    public string Categoria { get; set; } = "";
    
    [MaxLength(50)]
    public string Cultivo { get; set; } = "";
    
    [Column(TypeName = "decimal(18,2)")]
    public decimal Cantidad { get; set; }
    
    [MaxLength(20)]
    public string Unidad { get; set; } = "";
    
    [Column(TypeName = "decimal(18,2)")]
    public decimal StockMin { get; set; }
    
    public DateOnly? Caducidad { get; set; }
    
    [MaxLength(50)]
    public string Finca { get; set; } = "";
    
    [MaxLength(500)]
    public string Descripcion { get; set; } = "";
    
    public DateOnly FechaCreacion { get; set; } = DateOnly.FromDateTime(DateTime.Today);
    
    public DateOnly? FechaActualizacion { get; set; }
}