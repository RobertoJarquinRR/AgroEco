using AgroEco.Core;
using AgroEco.Core.Interfaces;
using System.Diagnostics;
using System.IO.Ports;
using System.Text;
using System.Text.Json;
using HardwareMessage = AgroEco.Core.Hardware.HardwareMessage;
using HardwareMessageReceivedEventArgs = AgroEco.Core.Hardware.HardwareMessageReceivedEventArgs;

namespace AgroEco.Hardware;

public sealed class SerialConnection : ISerialConnection, IDisposable
{
    private const string ConnectionChallenge = "ECOAGRO/1 CHALLENGE";
    private const string ConnectionAccept = "ECOAGRO/1 ACCEPT";
    private const string ConnectionReady = "ECOAGRO/1 READY";
    private const string MessageStart = "@{";
    private const string MessageEnd = "}*";
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    private readonly SerialPort _serialPort;
    private readonly object _bufferLock = new();
    private readonly StringBuilder _messageBuffer = new();
    private bool _isListening;

    private SerialConnection(SerialPort serialPort)
    {
        _serialPort = serialPort;
        Connected = serialPort.IsOpen;
    }

    public bool Connected { get; private set; }
    public event EventHandler<HardwareMessageReceivedEventArgs>? MessageReceived;

    public static Result<string[]> GetCompatibleSerialPorts()
    {
        try
        {
            string[] ports = SerialPort.GetPortNames()
                .Select(NormalizePortName)
                .Where(port => !string.IsNullOrWhiteSpace(port))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .OrderBy(port => port, StringComparer.OrdinalIgnoreCase)
                .ToArray();

            return Result<string[]>.CreateSuccess(ports);
        }
        catch (IOException exception)
        {
            return Result<string[]>.CreateFailure(
                "Could not retrieve the available serial ports.",
                exception);
        }
        catch (UnauthorizedAccessException exception)
        {
            return Result<string[]>.CreateFailure(
                "Access to the available serial ports was denied.",
                exception);
        }
    }

    public static Result<SerialConnection> OpenSerialConnection(string portName)
    {
        string normalizedPortName = NormalizePortName(portName);
        if (string.IsNullOrWhiteSpace(normalizedPortName))
        {
            return Result<SerialConnection>.CreateFailure(
                "A serial port name is required.");
        }

        SerialPort? serialPort = null;

        try
        {
            serialPort = new SerialPort(normalizedPortName, 115200, Parity.None)
            {
                DtrEnable = true,
                ReadTimeout = 2000
            };
            serialPort.Open();

            if (!CompleteHandshake(serialPort))
            {
                serialPort.Dispose();
                return Result<SerialConnection>.CreateFailure(
                    $"The device connected to '{normalizedPortName}' did not respond to the serial handshake.");
            }

            return Result<SerialConnection>.CreateSuccess(
                new SerialConnection(serialPort),
                $"Port '{normalizedPortName}' connected successfully.");
        }
        catch (ArgumentException exception)
        {
            serialPort?.Dispose();
            return Result<SerialConnection>.CreateFailure(
                $"The serial port '{portName}' is invalid.",
                exception);
        }
        catch (InvalidOperationException exception)
        {
            serialPort?.Dispose();
            return Result<SerialConnection>.CreateFailure(
                $"The serial port '{normalizedPortName}' could not be opened.",
                exception);
        }
        catch (IOException exception)
        {
            serialPort?.Dispose();
            return Result<SerialConnection>.CreateFailure(
                $"The serial port '{normalizedPortName}' could not be opened.",
                exception);
        }
        catch (UnauthorizedAccessException exception)
        {
            serialPort?.Dispose();
            return Result<SerialConnection>.CreateFailure(
                $"Access to the serial port '{normalizedPortName}' was denied.",
                exception);
        }
    }

    public Task<Result> CloseConnection()
    {
        if (!Connected)
        {
            return Task.FromResult(
                Result.CreateFailure("No active serial connection to close"));
        }

        StopListening();
        _serialPort.Close();
        Connected = false;
        return Task.FromResult(
            Result.CreateSuccess("Serial connection closed successfully"));
    }

    public Result StartListening()
    {
        if (!Connected)
        {
            return Result.CreateFailure("No active serial connection to listen");
        }

        if (_isListening)
        {
            return Result.CreateSuccess("Serial listener is already running");
        }

        _serialPort.DataReceived += OnDataReceived;
        _isListening = true;
        return Result.CreateSuccess("Serial listener started successfully");
    }

    public Result StopListening()
    {
        if (!_isListening)
        {
            return Result.CreateSuccess("Serial listener is already stopped");
        }

        _serialPort.DataReceived -= OnDataReceived;
        _isListening = false;
        return Result.CreateSuccess("Serial listener stopped successfully");
    }

    private void OnDataReceived(object? sender, SerialDataReceivedEventArgs args)
    {
        string data = _serialPort.ReadExisting();
        List<HardwareMessage> messages;

        lock (_bufferLock)
        {
            _messageBuffer.Append(data);
            messages = ParseMessages();
        }

        foreach (HardwareMessage message in messages)
        {
            MessageReceived?.Invoke(
                this,
                new HardwareMessageReceivedEventArgs(message));
        }
    }

    private List<HardwareMessage> ParseMessages()
    {
        List<HardwareMessage> messages = new();

        while (true)
        {
            int startIndex = _messageBuffer.ToString().IndexOf(
                MessageStart,
                StringComparison.Ordinal);

            if (startIndex < 0)
            {
                _messageBuffer.Clear();
                break;
            }

            if (startIndex > 0)
            {
                _messageBuffer.Remove(0, startIndex);
            }

            int endIndex = _messageBuffer.ToString().IndexOf(
                MessageEnd,
                MessageStart.Length,
                StringComparison.Ordinal);

            if (endIndex < 0)
            {
                break;
            }

            int jsonStart = MessageStart.Length;
            string json = _messageBuffer.ToString(
                jsonStart,
                endIndex - jsonStart);

            _messageBuffer.Remove(
                0,
                endIndex + MessageEnd.Length);

            try
            {
                HardwareMessage? message =
                    JsonSerializer.Deserialize<HardwareMessage>(json, JsonOptions);

                if (message is not null
                    && !string.IsNullOrWhiteSpace(message.Type)
                    && !string.IsNullOrWhiteSpace(message.ComponentId))
                {
                    messages.Add(message);
                }
            }
            catch (JsonException)
            {
                // Ignore malformed frames and continue processing later frames.
            }
        }

        return messages;
    }

    public void Dispose()
    {
        StopListening();

        if (_serialPort.IsOpen)
        {
            _serialPort.Close();
        }

        _serialPort.Dispose();
        Connected = false;
    }

    private static string NormalizePortName(string? portName)
    {
        string normalizedPortName = portName?.Trim() ?? string.Empty;

        if (normalizedPortName.StartsWith(@"\\.\", StringComparison.OrdinalIgnoreCase))
        {
            normalizedPortName = normalizedPortName[4..];
        }

        if (normalizedPortName.StartsWith("COM", StringComparison.OrdinalIgnoreCase)
            && int.TryParse(normalizedPortName[3..], out int portNumber)
            && portNumber > 0)
        {
            return $"COM{portNumber}";
        }

        return normalizedPortName;
    }

    private static bool CompleteHandshake(SerialPort serialPort)
    {
        StringBuilder incomingData = new();
        Stopwatch timeout = Stopwatch.StartNew();

        while (timeout.Elapsed < TimeSpan.FromSeconds(2))
        {
            incomingData.Append(serialPort.ReadExisting());

            if (incomingData.ToString().Contains(
                ConnectionChallenge,
                StringComparison.Ordinal))
            {
                serialPort.WriteLine(ConnectionAccept);
                incomingData.Clear();
            }

            if (incomingData.ToString().Contains(
                ConnectionReady,
                StringComparison.Ordinal))
            {
                return true;
            }

            Thread.Sleep(50);
        }

        return false;
    }
}
