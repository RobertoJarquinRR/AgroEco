using AgroEco.Core;
using AgroEco.Core.Jobs;
using Microsoft.EntityFrameworkCore;
namespace AgroEco.Data.Repositories
{
    public class JobRepository : RepositoryBase<Job, DataContext> ,IJobRepository
    {
        public JobRepository(DataContext context) : base(context) { }

      

        protected override void ApplyChanges(Job existingEntity, Job newEntity)
        {
            Result detailsResult = existingEntity.UpdateDetails(
                newEntity.Name,
                newEntity.Description,
                newEntity.Priority,
                newEntity.Date);

            if (!detailsResult.Success)
            {
                throw new InvalidOperationException(detailsResult.Message);
            }

            if (existingEntity.Status != newEntity.Status)
            {
                Result statusResult = existingEntity.ChangeStatus(newEntity.Status);
                if (!statusResult.Success)
                {
                    throw new InvalidOperationException(statusResult.Message);
                }
            }
        }

        public async Task<List<Job>> GetRunningJobsAsync()
        {
            return await _context.Jobs
            .Where(j => j.Status == Status.Running)
            .Include(j => j.Trigger)
            .Include(j => j.Actions)
            .ToListAsync();
        }

        public async Task<Job?> GetByIdWithDetailsAsync(int id, CancellationToken ct = default)
        {
            IQueryable<Job> query = _dbSet;

            query = query.Include(j => j.Trigger)
            .Include(j => j.Actions);

            return await query.FirstOrDefaultAsync(j => j.Id == id, ct);
        }
    }
}