using AgroEco.Hardware;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Text.Json;

namespace AgroEco.UI.Services;

public class SerialHostedService : BackgroundService
{
    private readonly SerialConnection _serialConnection;
    private readonly ILogger<SerialHostedService> _logger;
    private readonly IServiceProvider _serviceProvider;
    private readonly SerialSettings _settings;

    public SerialHostedService(
        SerialConnection serialConnection,
        ILogger<SerialHostedService> logger,
        IServiceProvider serviceProvider,
        IOptions<SerialSettings> settings)
    {
        _serialConnection = serialConnection;
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
                if (!_serialConnection.Connected)
                {
                    await ConectarAsync(stoppingToken);
                }

                if (_serialConnection.Connected)
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

        var conn = connResult.Value;
        _serialConnection = conn;
        conn.MessageReceived += OnHardwareMessageReceived;
        
        _logger.LogInformation("Conectado a puerto serial {Port}", _settings.PortName);
    }

    private void OnHardwareMessageReceived(object? sender, HardwareMessageReceivedEventArgs e)
    {
        var message = e.Message;
        
        try
        {
            if (message.Type.Equals("sensor_reading", StringComparison.OrdinalIgnoreCase) ||
                message.Type.Equals("sensor_data", StringComparison.OrdinalIgnoreCase))
            {
                var dto = JsonSerializer.Deserialize<SensorReadingDto>(JsonSerializer.Serialize(message.Payload));
                if (dto != null)
                {
                    // Usar el service provider para obtener el handler y enviar la lectura
                    using var scope = _serviceProvider.CreateScope();
                    var handler = scope.ServiceProvider.GetRequiredService<SensorReadingHandler>();
                    handler.ProcesarLecturaExterna(dto);
                }
            }
        }
        catch (Exception ex)
        {
            // Log error
        }
    }

    public override void Dispose()
    {
        _serialConnection?.Dispose();
        base.Dispose();
    }

    private record SensorReadingDto(
        string SensorTipo,
        decimal Valor,
        int? FincaId,
        string FincaNombre,
        string SensorNombre,
        DateTime? Timestamp = null);
}

public class SerialSettings
{
    public string PortName { get; set; } = "COM3";
    public int BaudRate { get; set; } = 115200;
    public bool AutoConnect { get; set; } = true;
    public int ReconnectDelayMs { get; set; } = 5000;
}