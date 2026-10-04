using System;
using System.Collections.Generic;
using System.Linq;
using AgroEco.UI.Clases;

namespace AgroEco.UI.Mensajeros
{
    public class TareasHandler
    {
        private readonly Action<string, object> _payloadEnviar;
        private readonly List<Tareas> _Tarea = new();
        private long _idTarea = 1;

        public TareasHandler(Action<string, object> payloadEnviar)
        {
            _payloadEnviar = payloadEnviar;

            _Tarea.Add(new Tareas
            {
                Id = _idTarea++,
                Nombre = "Tarea ramdomsitaxd",
                Descripcion = "Descripcion de la tarea ramdomsitaxd",
                FechaVencimiento = DateOnly.FromDateTime(DateTime.Today.AddDays(7)),
                Estado = "pendiente",
                Prioridad = "baja",
                Responsable = "Usuario1",
            });
        }

        public void ManejarMensaje(Mensaje msg)
        {
            switch (msg.Type)
            {
                case "obtenerTareas":
                    EnviarLista();
                    break;
            }
        }

        private void EnviarLista()
        {
            var lista = _Tarea.Select(t => new
            {
                id = t.Id,
                nombre = t.Nombre,
                descripcion = t.Descripcion,
                asignado = t.Responsable,
                prioridad = t.Prioridad,
                fechaLimite = t.FechaVencimiento.ToString("dd/MM/yyyy"),
                fechaLimiteISO = t.FechaVencimiento.ToString("yyyy-MM-dd"),
                estado = CalcularEstado(t)
            }).ToList();

            _payloadEnviar("tareasCargadas", lista);
        }

        private static string? CalcularEstado(Tareas t)
        {
            if (t.Estado == "completada") return "completada";
            int dias = t.FechaVencimiento.DayNumber - DateOnly.FromDateTime(DateTime.Today).DayNumber;
            if (dias < 0) return "vencida";
            if (dias <= 2) return "porVencer";
            return t.Estado;
        }
    }
}