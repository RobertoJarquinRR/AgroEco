using AgroEco.Hardware;
using System;
using System.Collections.Generic;
using System.Text;

namespace AgroEco.UI.Brigde_Harware
{
    public class BrigdeHardware
    {
        private SerialConnection serialConnection;
        public Action<string, object> MensajeRecibido;

        public Dictionary<string, object?> DatosConvertidos = new()
        {
            ["temp"]
        };

        
        public void EnviarMensaje(string tipo, object Payload)
        {

        }
    }
}
