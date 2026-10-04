using System;
using System.Collections.Generic;
using System.Text;

namespace AgroEco.UI.Mensajeros
{
    public class MsgRouter
    {
        private readonly TareasHandler _tareas;

        public MsgRouter(Action<string, object> enviar)
        {
            _tareas = new TareasHandler(enviar);
        }

        // true = un handler lo atendió. false = que siga el switch viejo
        public bool Enrutar(Mensaje mensaje)
        {
            try
            {
                switch (mensaje.Screen)
                {
                    case "tareas":
                        _tareas.ManejarMensaje(mensaje);
                        return true;

                    default:
                        return false;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error en {mensaje.Screen}/{mensaje.Type}: {ex}");
                return true;
            }
        }
    }
}
