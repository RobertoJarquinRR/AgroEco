using AgroEco.Core;
using AgroEco.Core.Jobs;
using AgroEco.Core.Triggers.Configuration;
using AgroEco.Core.Triggers.Implementations;
using AgroEco.Core.Jobs.Persistence.Queries;
using Microsoft.EntityFrameworkCore;
namespace AgroEco.Data.Repositories
{
    public class JobRepository : RepositoryBase<Job, DataContext>, IJobRepository
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

        public async Task<(List<Job> Jobs, int TotalCount)> GetFilteredAsync(
            JobFilter filter,
            CancellationToken ct = default)
        {
            IQueryable<Job> query = _context.Jobs
                .AsNoTracking()
                .Include(j => j.Trigger)
                .Include(j => j.Actions);

            if (!string.IsNullOrWhiteSpace(filter.SearchTerm))
            {
                string term = filter.SearchTerm.Trim().ToLower();
                query = query.Where(j => 
                    j.Name.ToLower().Contains(term) || 
                    (j.Description != null && j.Description.ToLower().Contains(term)));
            }

            if (filter.Status.HasValue)
            {
                query = query.Where(j => j.Status == filter.Status.Value);
            }

            if (filter.Priority.HasValue)
            {
                query = query.Where(j => j.Priority == filter.Priority.Value);
            }

            if (filter.DateFrom.HasValue)
            {
                query = query.Where(j => j.Date >= filter.DateFrom.Value.ToDateTime(TimeOnly.MinValue));
            }

            if (filter.DateTo.HasValue)
            {
                query = query.Where(j => j.Date <= filter.DateTo.Value.ToDateTime(TimeOnly.MaxValue));
            }

            int totalCount = await query.CountAsync(ct);

            query = filter.SortBy.ToLower() switch
            {
                "name" => filter.SortDirection == "asc" 
                    ? query.OrderBy(j => j.Name) 
                    : query.OrderByDescending(j => j.Name),
                "priority" => filter.SortDirection == "asc" 
                    ? query.OrderBy(j => j.Priority) 
                    : query.OrderByDescending(j => j.Priority),
                "status" => filter.SortDirection == "asc" 
                    ? query.OrderBy(j => j.Status) 
                    : query.OrderByDescending(j => j.Status),
                _ => filter.SortDirection == "asc" 
                    ? query.OrderBy(j => j.Date) 
                    : query.OrderByDescending(j => j.Date)
            };

            int skip = (filter.Page - 1) * filter.PageSize;
            var jobs = await query
                .Skip(skip)
                .Take(filter.PageSize)
                .ToListAsync(ct);

            return (jobs, totalCount);
        }

        public async Task<List<JobExecutionRecord>> GetExecutionHistoryAsync(
            int jobId,
            CancellationToken ct = default)
        {
            var job = await _context.Jobs
                .AsNoTracking()
                .Include(j => j.Trigger)
                .Include(j => j.Actions)
                .FirstOrDefaultAsync(j => j.Id == jobId, ct);

            if (job == null)
                return new List<JobExecutionRecord>();

            var records = new List<JobExecutionRecord>();
            var executedAt = job.Date ?? DateTime.MinValue;

            if (job.Results.Any())
            {
                foreach (var result in job.Results)
                {
                    var actionRecords = new List<ActionExecutionRecord>();
                    foreach (var action in job.Actions)
                    {
                        actionRecords.Add(new ActionExecutionRecord(
                            action.Name,
                            action.Status,
                            null,
                            executedAt,
                            action.Status != Status.Created && action.Status != Status.Enqueued 
                                ? executedAt 
                                : null));
                    }

                    records.Add(new JobExecutionRecord(
                        job.Id,
                        job.Name,
                        executedAt,
                        job.Status,
                        result.Message,
                        actionRecords));
                }
            }
            else if (job.Status != Status.Created && job.Status != Status.Enqueued)
            {
                var actionRecords = job.Actions.Select(a => new ActionExecutionRecord(
                    a.Name,
                    a.Status,
                    null,
                    executedAt,
                    a.Status != Status.Created && a.Status != Status.Enqueued 
                        ? executedAt 
                        : null)).ToList();

                records.Add(new JobExecutionRecord(
                    job.Id,
                    job.Name,
                    executedAt,
                    job.Status,
                    $"Job {job.Status}",
                    actionRecords));
            }

            return records.OrderByDescending(r => r.ExecutedAt).ToList();
        }
    }
}