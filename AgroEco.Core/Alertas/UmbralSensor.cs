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
    
    [Required]
    [MaxLength(20)]
    public string SeveridadMinima { get; set; } = "media"; // severidad si baja del mínimo
    
    [Required]
    [MaxLength(20)]
    public string SeveridadMaxima { get; set; } = "media"; // severidad si sube del máximo
    
    public bool Activo { get; set; } = true;
    
    public bool GenerarTareaAuto { get; set; } = true; // si true, crea tarea al disparar
    
    [Required]
    [MaxLength(500)]
    public string AccionSugerida { get; set; } = ""; // descripción de qué hacer
    
    public int? InsumoSugeridoId { get; set; } // insumo a usar si genera tarea
    
    public decimal? CantidadInsumoSugerida { get; set; }
    
    public decimal? CostoUnitarioSugerido { get; set; }
    
    // Nuevos campos para ejecución de acciones automáticas
    [MaxLength(50)]
    public string? AccionTipo { get; set; } // "sendAlert", "activateHardware", "executeTask", null = ninguna
    
    public string? AccionConfigJson { get; set; } // JSON con configuración de la acción
    
    public int CooldownMinutos { get; set; } = 30; // minutos de espera entre disparos
    
    public DateTime? UltimoDisparo { get; set; } // último disparo exitoso
    
    public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;
    
    public DateTime? FechaActualizacion { get; set; }
}
