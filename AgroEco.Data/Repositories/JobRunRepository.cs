using AgroEco.Core.Jobs.Runs;
using Microsoft.EntityFrameworkCore;

namespace AgroEco.Data.Repositories
{
    public class JobRunRepository : RepositoryBase<JobRun, DataContext>, IJobRunRepository
    {
        public JobRunRepository(DataContext context) : base(context) { }

        protected override void ApplyChanges(JobRun existingEntity, JobRun newEntity)
        {
            existingEntity.Status = newEntity.Status;
            existingEntity.FinishedAt = newEntity.FinishedAt;
            existingEntity.Message = newEntity.Message;
            existingEntity.Error = newEntity.Error;
        }

        public async Task<List<JobRun>> GetByJobIdAsync(
            int jobId,
            CancellationToken ct = default)
        {
            return await _dbSet
                .AsNoTracking()
                .Where(run => run.JobId == jobId)
                .Include(run => run.Actions)
                .OrderByDescending(run => run.StartedAt)
                .ToListAsync(ct);
        }
    }
}
