using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using AgroEco.Core.Interfaces;

namespace AgroEco.Core.Finanzas;

public class RegistroFinanciero : IEntity
{
    public int Id { get; set; }
    
    [Required]
    [MaxLength(20)]
    public string Tipo { get; set; } = "costo"; // "ingreso" o "costo"
    
    [MaxLength(50)]
    public string? Cultivo { get; set; }
    
    [MaxLength(50)]
    public string Categoria { get; set; } = "";
    
    [Column(TypeName = "decimal(18,2)")]
    public decimal Monto { get; set; }
    
    public DateOnly Fecha { get; set; } = DateOnly.FromDateTime(DateTime.Today);
    
    [MaxLength(500)]
    public string Descripcion { get; set; } = "";
    
    public int? TaskId { get; set; } // Referencia opcional a la tarea que generó este gasto
    
    public DateOnly FechaCreacion { get; set; } = DateOnly.FromDateTime(DateTime.Today);
}