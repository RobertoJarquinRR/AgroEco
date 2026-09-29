namespace AgroEco.Core.Hadware
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
