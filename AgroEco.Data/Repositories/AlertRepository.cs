using AgroEco.Core.Alerts;
using AgroEco.Data;
using AgroEco.Data.Repositories;
using Microsoft.EntityFrameworkCore;

namespace AgroEco.Data.Repositories;

public class AlertRepository : RepositoryBase<AlertEntity, DataContext>, IAlertRepository
{
    public AlertRepository(DataContext context) : base(context) { }

    protected override void ApplyChanges(AlertEntity existing, AlertEntity next)
    {
        existing.Title = next.Title;
        existing.Message = next.Message;
        existing.Level = next.Level;
        existing.Status = next.Status;
        existing.ConfigurationJson = next.ConfigurationJson;
        existing.DeliveredAt = next.DeliveredAt;
        existing.MetadataJson = next.MetadataJson;

        foreach (AlertDeliveryEntity delivery in next.Deliveries)
        {
            if (delivery.Id == 0)
            {
                existing.Deliveries.Add(delivery);
            }
        }
    }

    public async Task<AlertEntity?> GetByIdWithDeliveriesAsync(int id, CancellationToken ct = default)
    {
        return await _dbSet
            .Include(a => a.Deliveries)
            .FirstOrDefaultAsync(a => a.Id == id, ct);
    }

    public async Task<List<AlertEntity>> GetAllWithDeliveriesAsync(
        int? limit = null,
        int? offset = null,
        AlertLevel? level = null,
        AlertStatus? status = null,
        DateTime? fromDate = null,
        DateTime? toDate = null,
        CancellationToken ct = default)
    {
        var query = _dbSet
            .Include(a => a.Deliveries)
            .AsNoTracking()
            .AsQueryable();

        if (level.HasValue)
            query = query.Where(e => e.Level == (int)level.Value);

        if (status.HasValue)
            query = query.Where(e => e.Status == (int)status.Value);

        if (fromDate.HasValue)
            query = query.Where(e => e.CreatedAt >= fromDate.Value);

        if (toDate.HasValue)
            query = query.Where(e => e.CreatedAt <= toDate.Value);

        query = query.OrderByDescending(e => e.CreatedAt);

        if (offset.HasValue)
            query = query.Skip(offset.Value);

        if (limit.HasValue)
            query = query.Take(limit.Value);

        return await query.ToListAsync(ct);
    }
}