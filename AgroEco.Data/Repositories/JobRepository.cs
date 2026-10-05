using AgroEco.Core;
using AgroEco.Core.Jobs;
using AgroEco.Core.Triggers.Configuration;
using AgroEco.Core.Triggers.Implementations;
using Microsoft.EntityFrameworkCore;
namespace AgroEco.Data.Repositories
{
    public class JobRepository : RepositoryBase<Job, DataContext> ,IJobRepository
    {
        public JobRepository(DataContext context) : base(context) { }

      

        protected override void ApplyChanges(Job existingEntity, Job newEntity)
        {
            _context.Entry(existingEntity)
                .Reference(job => job.Trigger)
                .Load();

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

            if (existingEntity.Trigger is null || newEntity.Trigger is null)
            {
                throw new InvalidOperationException(
                    $"Job '{existingEntity.Name}' must have a trigger.");
            }

            if (existingEntity.Trigger is not DateTimeTrigger existingDateTime
                || newEntity.Trigger is not DateTimeTrigger newDateTime)
            {
                throw new InvalidOperationException(
                    "The trigger type cannot be updated through the current repository.");
            }

            Result triggerResult = existingDateTime.UpdateConfiguration(
                new DateTimeTriggerConfiguration(newDateTime.TargetTime));
            if (!triggerResult.Success)
            {
                throw new InvalidOperationException(triggerResult.Message);
            }
        }

        public async Task<List<Job>> GetRunningJobsAsync(
            CancellationToken ct = default)
        {
            return await _context.Jobs
            .Where(j => j.Status == Status.Running)
            .AsNoTracking()
            .Include(j => j.Trigger)
            .Include(j => j.Actions)
            .ToListAsync(ct);
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