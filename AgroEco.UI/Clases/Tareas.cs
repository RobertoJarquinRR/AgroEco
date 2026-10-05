using System;
using System.Collections.Generic;
using System.Text;

namespace AgroEco.UI.Clases
{
    public class Tareas
    {
        public long Id { get; set; } //para que coincida con el js ya quie crea id's en ese cantidad de bytes 8
        public string? Nombre { get; set; }
        public string? Descripcion { get; set; }
        public DateOnly FechaCreada { get; set; } = DateOnly.FromDateTime(DateTime.Today);
        public DateOnly FechaVencimiento { get; set; }
       
        public string? Estado { get; set; } //pendiente, en progreso, completada los demas estado como por vencer se calcularan en base a la fecha que vence
        public string Prioridad { get; set; } = "Baja"; //baja, media, alta 
        public string? Responsable { get; set; } //el que se encarga de la tarea
       
    }
}
