using AgroEco.Core.Hardware;

namespace AgroEco.Core.Interfaces
{
    public interface ISerialConnection
    {
        bool Connected { get; }
        event EventHandler<HardwareMessageReceivedEventArgs>? MessageReceived;
        Result StartListening();
        Result StopListening();
        Task<Result> CloseConnection();
    }
}
