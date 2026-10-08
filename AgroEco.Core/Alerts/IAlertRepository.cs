using AgroEco.Core.Alerts;
using AgroEco.Core.Interfaces;

namespace AgroEco.Core.Alerts;

public interface IAlertRepository : IRepository<AlertEntity>
{
    Task<AlertEntity?> GetByIdWithDeliveriesAsync(int id, CancellationToken ct = default);
    Task<List<AlertEntity>> GetAllWithDeliveriesAsync(
        int? limit = null,
        int? offset = null,
        AlertLevel? level = null,
        AlertStatus? status = null,
        DateTime? fromDate = null,
        DateTime? toDate = null,
        CancellationToken ct = default);
}