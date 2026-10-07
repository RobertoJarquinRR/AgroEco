using AgroEco.Core.Interfaces;
using AgroEco.Core.Alertas;
using AgroEco.Core;

namespace AgroEco.Core.Alertas.Persistence;

public class CreateAlerta
{
    private readonly IRepository<Alerta> _repository;
    private readonly IUnitOfWork _unitOfWork;

    public CreateAlerta(IRepository<Alerta> repository, IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<Alerta>> HandleAsync(
        string tipo,
        string severidad,
        string titulo,
        string descripcion,
        int? fincaId,
        string fincaNombre,
        int? sensorId,
        string sensorNombre,
        string sensorTipo,
        decimal valorActual,
        decimal umbralConfigurado,
        string accionSugerida = "",
        int? insumoSugeridoId = null,
        decimal? cantidadInsumo = null,
        decimal? costoUnitario = null)
    {
        if (string.IsNullOrWhiteSpace(tipo))
            return Result<Alerta>.CreateFailure("Tipo de alerta es obligatorio");

        var alerta = new Alerta
        {
            Tipo = tipo,
            Severidad = string.IsNullOrWhiteSpace(severidad) ? "media" : severidad,
            Titulo = titulo ?? "",
            Descripcion = descripcion ?? "",
            FincaId = fincaId,
            FincaNombre = fincaNombre ?? "",
            SensorId = sensorId,
            SensorNombre = sensorNombre ?? "",
            SensorTipo = sensorTipo ?? "",
            ValorActual = valorActual,
            UmbralConfigurado = umbralConfigurado,
            AccionSugerida = accionSugerida ?? "",
            InsumoSugeridoId = insumoSugeridoId,
            CantidadInsumoSugerida = cantidadInsumo,
            CostoUnitarioSugerido = costoUnitario,
            FechaCreacion = DateTime.UtcNow
        };

        await _repository.AddAsync(alerta);
        await _unitOfWork.SaveChangesAsync();

        return Result<Alerta>.CreateSuccess(alerta, "Alerta creada");
    }
}

public class GetAllAlertas
{
    private readonly IRepository<Alerta> _repository;

    public GetAllAlertas(IRepository<Alerta> repository)
    {
        _repository = repository;
    }

    public async Task<Result<List<Alerta>>> HandleAsync(bool soloActivas = true, CancellationToken ct = default)
    {
        try
        {
            var alertas = await _repository.GetAllAsync(ct);
            if (soloActivas)
                alertas = alertas.Where(a => a.EsActiva).ToList();
            return Result<List<Alerta>>.CreateSuccess(alertas.OrderByDescending(a => a.FechaCreacion).ToList());
        }
        catch (Exception ex)
        {
            return Result<List<Alerta>>.CreateFailure($"Error obteniendo alertas: {ex.Message}");
        }
    }
}

public class GetByIdAlerta
{
    private readonly IRepository<Alerta> _repository;

    public GetByIdAlerta(IRepository<Alerta> repository)
    {
        _repository = repository;
    }

    public async Task<Result<Alerta>> HandleAsync(int id, CancellationToken ct = default)
    {
        try
        {
            var alerta = await _repository.GetByIdAsync(id, ct);
            if (alerta == null)
                return Result<Alerta>.CreateFailure($"Alerta con ID {id} no encontrada");
            return Result<Alerta>.CreateSuccess(alerta);
        }
        catch (Exception ex)
        {
            return Result<Alerta>.CreateFailure($"Error obteniendo alerta: {ex.Message}");
        }
    }
}

public class UpdateAlerta
{
    private readonly IRepository<Alerta> _repository;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateAlerta(IRepository<Alerta> repository, IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> HandleAsync(Alerta alerta, CancellationToken ct = default)
    {
        try
        {
            var existing = await _repository.GetByIdAsync(alerta.Id, ct);
            if (existing == null)
                return Result.CreateFailure($"Alerta con ID {alerta.Id} no encontrada");

            await _repository.UpdateAsync(alerta, ct);
            await _unitOfWork.SaveChangesAsync();

            return Result.CreateSuccess("Alerta actualizada");
        }
        catch (Exception ex)
        {
            return Result.CreateFailure($"Error actualizando alerta: {ex.Message}");
        }
    }
}

public class ResolverAlerta
{
    private readonly IRepository<Alerta> _repository;
    private readonly IUnitOfWork _unitOfWork;

    public ResolverAlerta(IRepository<Alerta> repository, IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> HandleAsync(int id, int? tareaGeneradaId = null, CancellationToken ct = default)
    {
        try
        {
            var alerta = await _repository.GetByIdAsync(id, ct);
            if (alerta == null)
                return Result.CreateFailure($"Alerta con ID {id} no encontrada");

            alerta.EsActiva = false;
            alerta.FechaResuelta = DateTime.UtcNow;
            if (tareaGeneradaId.HasValue)
                alerta.TareaGeneradaId = tareaGeneradaId;

            await _repository.UpdateAsync(alerta, ct);
            await _unitOfWork.SaveChangesAsync();

            return Result.CreateSuccess("Alerta resuelta");
        }
        catch (Exception ex)
        {
            return Result.CreateFailure($"Error resolviendo alerta: {ex.Message}");
        }
    }
}