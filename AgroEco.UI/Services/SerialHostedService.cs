using AgroEco.Hardware;
using AgroEco.Core.Hardware;
using AgroEco.UI.Handlers;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.DependencyInjection;
using System.Text.Json;
using Windows.ApplicationModel;
using Windows.Devices.Sensors;

namespace AgroEco.UI.Services;

public class SerialHostedService : BackgroundService
{
    private SerialConnection? _serialConnection;
    public static event Action<string, object > MensajeRecibido;
    private readonly ILogger<SerialHostedService> _logger;
    private readonly IServiceProvider _serviceProvider;
    private readonly SerialSettings _settings;

    public SerialHostedService(
        ILogger<SerialHostedService> logger,
        IServiceProvider serviceProvider,
        IOptions<SerialSettings> settings)
    {
        _logger = logger;
        _serviceProvider = serviceProvider;
        _settings = settings.Value;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (!_settings.AutoConnect || string.IsNullOrWhiteSpace(_settings.PortName))
        {
            _logger.LogInformation("Conexión serial automática deshabilitada");
            return;
        }

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                if (_serialConnection == null || !_serialConnection.Connected)
                {
                    await ConectarAsync(stoppingToken);
                }

                if (_serialConnection != null && _serialConnection.Connected)
                {
                    var listenResult = _serialConnection.StartListening();
                    if (listenResult.Success)
                    {
                        _logger.LogInformation("Escucha serial iniciada en {Port}", _settings.PortName);
                        
                        // Mantener la conexión viva
                        while (_serialConnection.Connected && !stoppingToken.IsCancellationRequested)
                        {
                            await Task.Delay(5000, stoppingToken);
                        }
                    }
                    else
                    {
                        _logger.LogWarning("Error iniciando escucha: {Error}", listenResult.Message);
                        await Task.Delay(_settings.ReconnectDelayMs, stoppingToken);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error en SerialHostedService");
                await Task.Delay(_settings.ReconnectDelayMs, stoppingToken);
            }
        }
    }

    private async Task ConectarAsync(CancellationToken ct)
    {
        var connResult = SerialConnection.OpenSerialConnection(_settings.PortName);
        if (!connResult.Success)
        {
            _logger.LogWarning("Error conectando a {Port}: {Error}", _settings.PortName, connResult.Message);
            return;
        }

        _serialConnection = connResult.Value;
        _serialConnection.MessageReceived += OnHardwareMessageReceived;
        
        _logger.LogInformation("Conectado a puerto serial {Port}", _settings.PortName);
    }

    private void OnHardwareMessageReceived(object? sender, HardwareMessageReceivedEventArgs e)
    {
        var message = e.Message;

        if (message.Type != "sensor_reading") return;
        if (message.Value is { } v && v.TryGetDecimal(out var decimalValue))
        {
            MensajeRecibido?.Invoke(message.ComponentId, decimalValue);
        }
    }

    public override void Dispose()
    {
        _serialConnection?.Dispose();
        base.Dispose();
    }

    public class SerialSettings
    {
        public string PortName { get; set; } = "COM3";
        public int BaudRate { get; set; } = 115200;
        public bool AutoConnect { get; set; } = true;
        public int ReconnectDelayMs { get; set; } = 5000;
    }
}