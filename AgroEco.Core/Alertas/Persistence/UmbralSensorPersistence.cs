using AgroEco.Core.Interfaces;
using AgroEco.Core.Alertas;
using AgroEco.Core;

namespace AgroEco.Core.Alertas.Persistence;

public class CreateUmbralSensor
{
    private readonly IRepository<UmbralSensor> _repository;
    private readonly IUnitOfWork _unitOfWork;

    public CreateUmbralSensor(IRepository<UmbralSensor> repository, IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<UmbralSensor>> HandleAsync(
        string sensorTipo,
        int? fincaId,
        string fincaNombre,
        decimal? minimo,
        decimal? maximo,
        string severidadMinima,
        string severidadMaxima,
        bool generarTareaAuto,
        string accionSugerida,
        int? insumoSugeridoId,
        decimal? cantidadInsumoSugerida,
        decimal? costoUnitarioSugerido,
        bool activo = true)
    {
        if (string.IsNullOrWhiteSpace(sensorTipo))
            return Result<UmbralSensor>.CreateFailure("Tipo de sensor es obligatorio");

        if (!minimo.HasValue && !maximo.HasValue)
            return Result<UmbralSensor>.CreateFailure("Debe configurar al menos un umbral (mínimo o máximo)");

        var umbral = new UmbralSensor
        {
            SensorTipo = sensorTipo,
            FincaId = fincaId,
            FincaNombre = fincaNombre ?? "",
            Minimo = minimo,
            Maximo = maximo,
            SeveridadMinima = string.IsNullOrWhiteSpace(severidadMinima) ? "media" : severidadMinima,
            SeveridadMaxima = string.IsNullOrWhiteSpace(severidadMaxima) ? "media" : severidadMaxima,
            Activo = activo,
            GenerarTareaAuto = generarTareaAuto,
            AccionSugerida = accionSugerida ?? "",
            InsumoSugeridoId = insumoSugeridoId,
            CantidadInsumoSugerida = cantidadInsumoSugerida,
            CostoUnitarioSugerido = costoUnitarioSugerido,
            FechaCreacion = DateTime.UtcNow
        };

        await _repository.AddAsync(umbral);
        await _unitOfWork.SaveChangesAsync();

        return Result<UmbralSensor>.CreateSuccess(umbral, "Umbral creado");
    }
}

public class GetAllUmbralesSensor
{
    private readonly IRepository<UmbralSensor> _repository;

    public GetAllUmbralesSensor(IRepository<UmbralSensor> repository)
    {
        _repository = repository;
    }

    public async Task<Result<List<UmbralSensor>>> HandleAsync(bool soloActivos = true, CancellationToken ct = default)
    {
        try
        {
            var umbrales = await _repository.GetAllAsync(ct);
            if (soloActivos)
                umbrales = umbrales.Where(u => u.Activo).ToList();
            return Result<List<UmbralSensor>>.CreateSuccess(umbrales.OrderBy(u => u.SensorTipo).ThenBy(u => u.FincaNombre).ToList());
        }
        catch (Exception ex)
        {
            return Result<List<UmbralSensor>>.CreateFailure($"Error obteniendo umbrales: {ex.Message}");
        }
    }
}

public class GetByIdUmbralSensor
{
    private readonly IRepository<UmbralSensor> _repository;

    public GetByIdUmbralSensor(IRepository<UmbralSensor> repository)
    {
        _repository = repository;
    }

    public async Task<Result<UmbralSensor>> HandleAsync(int id, CancellationToken ct = default)
    {
        try
        {
            var umbral = await _repository.GetByIdAsync(id, ct);
            if (umbral == null)
                return Result<UmbralSensor>.CreateFailure($"Umbral con ID {id} no encontrado");
            return Result<UmbralSensor>.CreateSuccess(umbral);
        }
        catch (Exception ex)
        {
            return Result<UmbralSensor>.CreateFailure($"Error obteniendo umbral: {ex.Message}");
        }
    }
}

public class UpdateUmbralSensor
{
    private readonly IRepository<UmbralSensor> _repository;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateUmbralSensor(IRepository<UmbralSensor> repository, IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> HandleAsync(UmbralSensor umbral, CancellationToken ct = default)
    {
        try
        {
            var existing = await _repository.GetByIdAsync(umbral.Id, ct);
            if (existing == null)
                return Result.CreateFailure($"Umbral con ID {umbral.Id} no encontrado");

            if (!umbral.Minimo.HasValue && !umbral.Maximo.HasValue)
                return Result.CreateFailure("Debe configurar al menos un umbral (mínimo o máximo)");

            await _repository.UpdateAsync(umbral, ct);
            await _unitOfWork.SaveChangesAsync();

            return Result.CreateSuccess("Umbral actualizado");
        }
        catch (Exception ex)
        {
            return Result.CreateFailure($"Error actualizando umbral: {ex.Message}");
        }
    }
}

public class DeleteUmbralSensor
{
    private readonly IRepository<UmbralSensor> _repository;
    private readonly IUnitOfWork _unitOfWork;

    public DeleteUmbralSensor(IRepository<UmbralSensor> repository, IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> HandleAsync(int id, CancellationToken ct = default)
    {
        try
        {
            var existing = await _repository.GetByIdAsync(id, ct);
            if (existing == null)
                return Result.CreateFailure($"Umbral con ID {id} no encontrado");

            await _repository.DeleteAsync(id, ct);
            await _unitOfWork.SaveChangesAsync();

            return Result.CreateSuccess("Umbral eliminado");
        }
        catch (Exception ex)
        {
            return Result.CreateFailure($"Error eliminando umbral: {ex.Message}");
        }
    }
}