using AgroEco.Core.Hadware;

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
