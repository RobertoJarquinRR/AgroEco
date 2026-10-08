using AgroEco.Core.Alerts;
using AgroEco.Core.Alerts.Persistence;
using AgroEco.Core.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace AgroEco.Core.UnitTests.TestSupport;

public sealed class InMemoryUnitOfWork : IUnitOfWork
{
    public Task<int> SaveChangesAsync(CancellationToken ct = default)
        => Task.FromResult(1);
}

public sealed class InMemoryAlertRepository : IRepository<AlertEntity>
{
    private readonly List<AlertEntity> _alerts = new();
    private int _nextId = 1;

    public Task<List<AlertEntity>> GetAllAsync(CancellationToken ct = default)
        => Task.FromResult(_alerts.ToList());

    public Task<AlertEntity?> GetByIdAsync(int id, CancellationToken ct = default)
        => Task.FromResult(_alerts.FirstOrDefault(a => a.Id == id));

    public Task AddAsync(AlertEntity entity, CancellationToken ct = default)
    {
        entity.Id = _nextId++;
        _alerts.Add(entity);
        return Task.CompletedTask;
    }

    public Task DeleteAsync(int id, CancellationToken ct = default)
    {
        AlertEntity? existing = _alerts.FirstOrDefault(a => a.Id == id);
        if (existing is not null)
        {
            _alerts.Remove(existing);
        }

        return Task.CompletedTask;
    }

    public Task UpdateAsync(AlertEntity entity, CancellationToken ct = default)
    {
        AlertEntity? existing = _alerts.FirstOrDefault(a => a.Id == entity.Id);
        if (existing is not null)
        {
            int index = _alerts.IndexOf(existing);
            _alerts[index] = entity;
        }

        return Task.CompletedTask;
    }
}

public static class AlertTestProvider
{
    public static ServiceProvider Create()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddSingleton<IRepository<AlertEntity>>(new InMemoryAlertRepository());
        services.AddSingleton<IUnitOfWork>(new InMemoryUnitOfWork());
        services.AddScoped<CreateAlert>();
        services.AddScoped<GetAlertById>();
        services.AddScoped<GetAllAlerts>();
        services.AddScoped<UpdateAlert>();
        services.AddScoped<DeleteAlert>();
        services.AddSingleton<AlertEngine>(sp =>
            new AlertEngine(sp.GetRequiredService<IServiceScopeFactory>()));
        return services.BuildServiceProvider();
    }
}
