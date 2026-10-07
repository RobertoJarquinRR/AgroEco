using AgroEco.Core.Alertas;
using AgroEco.Core.Alertas.Persistence;
using AgroEco.Core.Interfaces;
using AgroEco.Core.Jobs;
using AgroEco.Core.Jobs.Actions;
using AgroEco.Core.Jobs.Actions.Configuration;
using AgroEco.Core.Jobs.Persistence;
using AgroEco.Core.Triggers;
using AgroEco.Core.Triggers.Configuration;
using AgroEco.Core.Inventario.Persistence;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace AgroEco.Core.Alertas;

public class AlertEngine
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<AlertEngine> _logger;
        private readonly TimeSpan _evaluationInterval = TimeSpan.FromMinutes(5);

        public AlertEngine(IServiceScopeFactory scopeFactory, ILogger<AlertEngine> logger)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
        }

    public async Task StartAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("AlertEngine iniciado");
        
        while (!cancellationToken.IsCancellationRequested)
        {
            try
            {
                await EvaluarUmbralesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error en evaluación de umbrales");
            }

            try
            {
                await Task.Delay(_evaluationInterval, cancellationToken);
            }
            catch (OperationCanceledException)
            {
                break;
            }
        }
    }

    private async Task EvaluarUmbralesAsync()
    {
        using var scope = _scopeFactory.CreateScope();
        
        var umbralRepo = scope.ServiceProvider.GetRequiredService<IRepository<UmbralSensor>>();
        var alertaRepo = scope.ServiceProvider.GetRequiredService<IRepository<Alerta>>();
        var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

        // Obtener umbrales activos
        var umbralesResult = await umbralRepo.GetAllAsync();
        var umbralesActivos = umbralesResult.Where(u => u.Activo).ToList();

        foreach (var umbral in umbralesActivos)
        {
            // TODO: Obtener última lectura del sensor para esta finca
            // Por ahora simulamos - en producción vendría de SensorReadingHandler
            await EvaluarUmbralAsync(umbral, scope.ServiceProvider);
        }
    }

    private async Task EvaluarUmbralAsync(UmbralSensor umbral, IServiceProvider serviceProvider)
    {
        // En implementación real, aquí obtendríamos la última lectura del sensor
        // del SensorReadingHandler o de una tabla de lecturas
        
        // Por ahora, este método es un placeholder para cuando lleguen las lecturas reales
        await Task.CompletedTask;
    }

    // Método público para evaluar una lectura específica (llamado desde SensorReadingHandler)
    public async Task EvaluarLecturaAsync(
        string sensorTipo,
        decimal valor,
        int? fincaId,
        string fincaNombre,
        string sensorNombre,
        CancellationToken cancellationToken = default)
    {
        using var scope = _scopeFactory.CreateScope();
        
        var umbralRepo = scope.ServiceProvider.GetRequiredService<IRepository<UmbralSensor>>();
        var alertaRepo = scope.ServiceProvider.GetRequiredService<IRepository<Alerta>>();
        var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
        var createAlerta = scope.ServiceProvider.GetRequiredService<CreateAlerta>();
        var createJob = scope.ServiceProvider.GetRequiredService<CreateJob>();
        var triggerFactory = scope.ServiceProvider.GetRequiredService<AgroEco.Core.Triggers.ITriggerFactory>();
        var actionFactory = scope.ServiceProvider.GetRequiredService<IActionFactory>();

        // Buscar umbrales que coincidan
        var umbrales = await umbralRepo.GetAllAsync(cancellationToken);
        var umbralesRelevantes = umbrales
            .Where(u => u.Activo && u.SensorTipo.Equals(sensorTipo, StringComparison.OrdinalIgnoreCase))
            .Where(u => u.FincaId == null || u.FincaId == fincaId)
            .ToList();

        foreach (var umbral in umbralesRelevantes)
        {
            var disparo = false;
            string tipoAlerta = "";
            string severidad = "media";
            decimal umbralValor = 0;

            // Verificar umbral mínimo
            if (umbral.Minimo.HasValue && valor < umbral.Minimo.Value)
            {
                disparo = true;
                tipoAlerta = $"{umbral.SensorTipo}_bajo";
                severidad = umbral.SeveridadMinima;
                umbralValor = umbral.Minimo.Value;
            }
            // Verificar umbral máximo
            else if (umbral.Maximo.HasValue && valor > umbral.Maximo.Value)
            {
                disparo = true;
                tipoAlerta = $"{umbral.SensorTipo}_alto";
                severidad = umbral.SeveridadMaxima;
                umbralValor = umbral.Maximo.Value;
            }

            if (disparo)
            {
                await ProcesarDisparoAsync(
                    umbral, valor, umbralValor, tipoAlerta, severidad,
                    fincaId, fincaId?.ToString() ?? "Sin finca", 
                    sensorNombre ?? umbral.SensorTipo,
                    scope.ServiceProvider, cancellationToken);
            }
        }
    }

    private async Task ProcesarDisparoAsync(
        UmbralSensor umbral,
        decimal valorActual,
        decimal umbralValor,
        string tipoAlerta,
        string severidad,
        int? fincaId,
        string fincaNombre,
        string sensorNombre,
        IServiceProvider serviceProvider,
        CancellationToken ct)
    {
        var alertaRepo = serviceProvider.GetRequiredService<IRepository<Alerta>>();
        var createAlerta = serviceProvider.GetRequiredService<CreateAlerta>();
        var unitOfWork = serviceProvider.GetRequiredService<IUnitOfWork>();
        var createJob = serviceProvider.GetRequiredService<CreateJob>();
        var triggerFactory = serviceProvider.GetRequiredService<AgroEco.Core.Triggers.ITriggerFactory>();
        var actionFactory = serviceProvider.GetRequiredService<IActionFactory>();

        // Verificar si ya hay una alerta activa para este mismo sensor/umbral
        var alertas = await alertaRepo.GetAllAsync(ct);
        var alertaExistente = alertas.FirstOrDefault(a => 
            a.EsActiva && 
            a.SensorTipo == umbral.SensorTipo && 
            a.FincaId == umbral.FincaId &&
            a.Tipo == tipoAlerta);

        if (alertaExistente != null)
        {
            // Actualizar valor actual y fecha de última notificación
            alertaExistente.ValorActual = valorActual;
            alertaExistente.FechaUltimaNotificacion = DateTime.UtcNow;
            // No duplicar alerta
            return;
        }

        // Crear nueva alerta
        var titulo = GenerarTitulo(umbral.SensorTipo, tipoAlerta, fincaNombre);
        var descripcion = GenerarDescripcion(umbral, valorActual, umbralValor);

        var alertaResult = await createAlerta.HandleAsync(
            tipo: tipoAlerta,
            severidad: severidad,
            titulo: titulo,
            descripcion: descripcion,
            fincaId: umbral.FincaId,
            fincaNombre: fincaNombre,
            sensorId: null, // TODO: mapear sensor real
            sensorNombre: sensorNombre,
            sensorTipo: umbral.SensorTipo,
            valorActual: valorActual,
            umbralConfigurado: umbralValor,
            accionSugerida: umbral.AccionSugerida,
            insumoSugeridoId: umbral.InsumoSugeridoId,
            cantidadInsumo: umbral.CantidadInsumoSugerida,
            costoUnitario: umbral.CostoUnitarioSugerido);

        if (!alertaResult.Success)
        {
            // Log error
            return;
        }

        var alerta = alertaResult.Value;

        // Si el umbral tiene GenerarTareaAuto = true, crear tarea automáticamente
        if (umbral.GenerarTareaAuto && umbral.InsumoSugeridoId.HasValue)
        {
            await CrearTareaAutomaticaAsync(alerta, umbral, serviceProvider, ct);
        }
    }

    private async Task CrearTareaAutomaticaAsync(
        Alerta alerta,
        UmbralSensor umbral,
        IServiceProvider serviceProvider,
        CancellationToken ct)
    {
        var triggerFactory = serviceProvider.GetRequiredService<AgroEco.Core.Triggers.ITriggerFactory>();
        var actionFactory = serviceProvider.GetRequiredService<IActionFactory>();
        var createJob = serviceProvider.GetRequiredService<CreateJob>();
        var getByIdInsumo = serviceProvider.GetRequiredService<GetByIdInsumo>();

        try
        {
            // Trigger: ejecutar ahora (datetime = ahora + 1 min)
            var triggerResult = triggerFactory.Create(
                "datetime",
                $"Trigger_Auto_{alerta.Id}",
                new DateTimeTriggerConfiguration(DateTimeOffset.UtcNow.AddMinutes(1)));

            if (!triggerResult.Success || triggerResult.Value == null)
                return;

            // Validar que el insumo existe
            var insumoResult = await getByIdInsumo.HandleAsync(umbral.InsumoSugeridoId.Value);
            if (!insumoResult.Success || insumoResult.Value == null)
                return;

            var insumo = insumoResult.Value;
            var cantidad = umbral.CantidadInsumoSugerida ?? 1;
            var costo = umbral.CostoUnitarioSugerido ?? 0;

            // Validar stock
            if (insumo.Cantidad < cantidad)
            {
                // Log: stock insuficiente para tarea automática
                return;
            }

            // Action: executeTask
            var actionConfig = new ExecuteTaskActionConfiguration
            {
                InsumoId = umbral.InsumoSugeridoId.Value,
                CantidadDescontar = cantidad,
                CostoUnitario = costo,
                Descripcion = $"Auto: {umbral.AccionSugerida} - Alerta: {alerta.Titulo}",
                CategoriaInsumo = umbral.SensorTipo
            };

            var actionResult = actionFactory.Create(
                "executeTask",
                $"Accion_Auto_{alerta.Id}",
                actionConfig);

            if (!actionResult.Success || actionResult.Value == null)
                return;

            // Crear job
            var jobResult = await createJob.HandleAsync(
                name: $"Auto: {alerta.Titulo}",
                description: $"Tarea generada automáticamente por alerta: {alerta.Descripcion}",
                priority: alerta.Severidad switch { "critica" => 1, "alta" => 1, "media" => 2, _ => 3 },
                actions: new List<AgroEco.Core.Jobs.Actions.Action> { actionResult.Value },
                trigger: triggerResult.Value);

            if (jobResult.Success)
            {
                // Marcar alerta como tarea generada
                // Nota: en implementación real necesitaríamos actualizar la alerta
                // con el ID de la tarea generada
            }
        }
        catch (Exception)
        {
            // Log error
        }
    }

    private static string GenerarTitulo(string sensorTipo, string tipoAlerta, string finca)
    {
        var sensorLabel = sensorTipo switch
        {
            "temperatura_suelo" => "Temp. Suelo",
            "temperatura_ambiente" => "Temp. Ambiente",
            "humedad_suelo" => "Hum. Suelo",
            "humedad_ambiente" => "Hum. Ambiente",
            "luz" => "Luz",
            _ => sensorTipo
        };

        var alertaLabel = tipoAlerta.EndsWith("_alto") ? "Alta" : "Baja";
        return $"{sensorLabel} {alertaLabel} - {finca}";
    }

    private static string GenerarDescripcion(UmbralSensor umbral, decimal valor, decimal umbralValor)
    {
        var direccion = valor > umbralValor ? "superó el máximo" : "está por debajo del mínimo";
        return $"El sensor detectó {valor:F1} (umbral: {umbralValor:F1}). {umbral.AccionSugerida}";
    }
}