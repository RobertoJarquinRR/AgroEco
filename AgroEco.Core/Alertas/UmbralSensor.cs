using System;
using System.ComponentModel.DataAnnotations;
using AgroEco.Core.Interfaces;

namespace AgroEco.Core.Alertas;

public class UmbralSensor : IEntity
{
    public int Id { get; set; }
    
    [Required]
    [MaxLength(50)]
    public string SensorTipo { get; set; } = ""; // "temperatura_suelo", "temperatura_ambiente", "humedad_suelo", "humedad_ambiente", "luz"
    
    public int? FincaId { get; set; }
    
    [MaxLength(50)]
    public string FincaNombre { get; set; } = "";
    
    public decimal? Minimo { get; set; } // null = no hay límite inferior
    
    public decimal? Maximo { get; set; } // null = no hay límite superior
    
    [MaxLength(20)]
    public string SeveridadMinima { get; set; } = "media"; // severidad si baja del mínimo
    
    [MaxLength(20)]
    public string SeveridadMaxima { get; set; } = "media"; // severidad si sube del máximo
    
    public bool Activo { get; set; } = true;
    
    public bool GenerarTareaAuto { get; set; } = true; // si true, crea tarea al disparar
    
    [MaxLength(500)]
    public string AccionSugerida { get; set; } = ""; // descripción de qué hacer
    
    public int? InsumoSugeridoId { get; set; } // insumo a usar si genera tarea
    
    public decimal? CantidadInsumoSugerida { get; set; }
    
    public decimal? CostoUnitarioSugerido { get; set; }
    
    public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;
    
    public DateTime? FechaActualizacion { get; set; }
}
