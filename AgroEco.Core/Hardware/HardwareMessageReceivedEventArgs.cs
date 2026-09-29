namespace AgroEco.Core.Hardware;

public sealed class HardwareMessageReceivedEventArgs(HardwareMessage message) : EventArgs
{
    public HardwareMessage Message { get; } = message;
}
