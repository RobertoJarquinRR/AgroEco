using System;
using System.Collections.Generic;
using System.Text;
using AgroEco.UI.Handlers;

namespace AgroEco.UI.Mensajeros
{
    public class MsgRouter
    {
        //coneciones con los handlers de cada screen. Si se agrega un nuevo screen, agregarlo acá y en el switch de Enrutar
        private readonly TareasHandler _tareas;
        private readonly PlagasHandler _plagas;
        private readonly FinanzasHandler _finanzas;

        public MsgRouter(Action<string, object> enviar)
        {
            _tareas = new TareasHandler(enviar);
            _plagas = new PlagasHandler(enviar);
            _finanzas = new FinanzasHandler(enviar);
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

                    case "plagas":
                        _plagas.ManejarMensaje(mensaje);
                        return true;

                    case "finanzas":                                 
                        _finanzas.ManejarMensaje(mensaje);
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
