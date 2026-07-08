namespace AgroEco.Core.Interfaces;

public interface IRepository<T>
{
    Task<List<T>> GetAllAsync(CancellationToken ct = default);
    Task<T?> GetByIdAsync(int id, CancellationToken ct = default);

    Task AddAsync(T entity, CancellationToken ct = default);
    Task DeleteAsync(int id, CancellationToken ct = default);
    Task UpdateAsync(T entity, CancellationToken ct = default);
}
