using AgroEco.Core;
using AgroEco.Core.Interfaces;
using System.IO.Ports;
using System.Text;


namespace AgroEco.Hardware
{

    record struct H{

    }

    
    public class SerialConexion : ISerialConexion

    {
        
        private static readonly object _bloquer = new();
        private static SerialPort? _puerto;

        public bool connected { get; private set; } = false;

        public async Task<Result> OpenConnexion()
        {
            string[] ports = SerialPort.GetPortNames();

            foreach (string port in ports)
            {
                SerialPort s = new(port, 115200, Parity.None);
                bool found = false;

                try
                {
                    s.DtrEnable = true;
                    s.Open();
                    s.ReadTimeout = 2000;

                    string data = s.ReadExisting();

                    if (data.Trim().Contains("canYouconectect?"))
                    {
                        s.WriteLine("y");

                        found = true;
                        _puerto = s;
                        connected = true;
                        
                        return Result.CreateSuccess("connected"); 

                    }

                }

                catch (Exception)
                {
                     
                }
                finally
                {
                    if (!found && s != null && s.IsOpen)
                    {
                        s.Close();
                    }

                }



            }
            return Result.CreateFailure("The connection Failed");

        }


        public async Task<Result> CloseConexion()
        {
            if(_puerto == null)
            {
                return Result.CreateFailure("There Not Connection to close");

            }

            _puerto.Close();
            if(connected == true){
                connected = false;
            }
            return Result.CreateSuccess("Connection Closed");

        }

        public async Task<Result> star()
        {

            if(_puerto == null)
            {
                return Result.CreateFailure("There Not Ports connected");
            }


            StringBuilder resultados = new();
            List<string> acumulador = new();

            _puerto.DataReceived += (sender, e) =>
            {
                SerialPort port = (SerialPort)sender;

                string data = port.ReadExisting();
                acumulador.Add(data);
                string textoTemporal = string.Concat(acumulador);
                if (textoTemporal.EndsWith("end"))
                {
                    string completo = string.Concat(acumulador);

                  
                        resultados.Append(completo);
                    
                    acumulador.Clear();
                }
            };

            string r = resultados.ToString();

            return Result.CreateFailure("hola");
           
        }

    }

}
