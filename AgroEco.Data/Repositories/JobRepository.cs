using AgroEco.Core;
using AgroEco.Core.Jobs;
using AgroEco.Core.Triggers.Configuration;
using AgroEco.Core.Triggers.Implementations;
using Microsoft.EntityFrameworkCore;
namespace AgroEco.Data.Repositories
{
    public class JobRepository : RepositoryBase<Job, DataContext> ,IJobRepository
    {
        private readonly IServiceProvider? _services;

        public JobRepository(DataContext context, IServiceProvider? services = null) : base(context) { _services = services; }

        private void AttachActionServices(Job job)
        {
            if (_services is null)
            {
                return;
            }

            foreach (AgroEco.Core.Jobs.Actions.Action action in job.Actions)
            {
                action.AttachServices(_services);
            }
        }



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

            _context.Entry(existingDateTime)
                .Property(trigger => trigger.TargetTime)
                .CurrentValue = newDateTime.TargetTime;
        }

        public async Task<List<Job>> GetRunningJobsAsync(
            CancellationToken ct = default)
        {
            List<Job> jobs = await _context.Jobs
            .Where(j => j.Status == Status.Running)
            .AsNoTracking()
            .Include(j => j.Trigger)
            .Include(j => j.Actions)
            .ToListAsync(ct);

            foreach (Job job in jobs)
            {
                AttachActionServices(job);
            }

            return jobs;
        }

        public async Task<Job?> GetByIdWithDetailsAsync(int id, CancellationToken ct = default)
        {
            IQueryable<Job> query = _dbSet;

            query = query.Include(j => j.Trigger)
            .Include(j => j.Actions);

            Job? job = await query.FirstOrDefaultAsync(j => j.Id == id, ct);

            if (job is not null)
            {
                AttachActionServices(job);
            }

            return job;
        }
    }
}