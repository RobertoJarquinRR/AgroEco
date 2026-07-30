using AgroEco.Core.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace AgroEco.Data.Repositories;

public abstract class RepositoryBase<TEntity, TContext> : IRepository<TEntity>
    where TEntity : class, IEntity
    where TContext : Microsoft.EntityFrameworkCore.DbContext
{
    protected readonly TContext _context;
    protected readonly DbSet<TEntity> _dbSet;

    protected RepositoryBase(TContext context)
    {
        _context = context;
        _dbSet = _context.Set<TEntity>();
    }

    public Task<List<TEntity>> GetAllAsync(CancellationToken ct = default)
        => _dbSet.AsNoTracking().ToListAsync(ct);

    public Task<TEntity?> GetByIdAsync(int id, CancellationToken ct = default)
        => _dbSet.FindAsync([id], ct).AsTask();

    public async Task AddAsync(TEntity entity, CancellationToken ct = default)
        => await _dbSet.AddAsync(entity, ct);

    public async Task DeleteAsync(int id, CancellationToken ct = default)
    {
        var existing = await _dbSet.FindAsync([id], ct);
        if (existing is not null)
        {
            _dbSet.Remove(existing);
        }
    }

    public async Task UpdateAsync(TEntity entity, CancellationToken ct = default)
    {
        var existing = await _dbSet.FindAsync([entity.Id], ct);
        if (existing is not null)
        {
            ApplyChanges(existing, entity);
        }
    }

    protected abstract void ApplyChanges(TEntity existingEntity, TEntity newEntity);
}
