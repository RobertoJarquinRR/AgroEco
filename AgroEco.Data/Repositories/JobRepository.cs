using AgroEco.Core;
using AgroEco.Core.Jobs;
using AgroEco.Core.Jobs.Runs;
using AgroEco.Core.Triggers.Configuration;
using AgroEco.Core.Triggers.Implementations;
using AgroEco.Core.Jobs.Persistence.Queries;
using Microsoft.EntityFrameworkCore;
namespace AgroEco.Data.Repositories
{
    public class JobRepository : RepositoryBase<Job, DataContext>, IJobRepository
    {
        private readonly IServiceProvider? _services;
        private readonly IJobRunRepository _jobRunRepository;

        public JobRepository(DataContext context, IServiceProvider? services = null, IJobRunRepository? jobRunRepository = null) : base(context) 
        { 
            _services = services;
            _jobRunRepository = jobRunRepository!;
        }

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
            var jobRuns = await _jobRunRepository.GetByJobIdAsync(jobId);

            var records = new List<JobExecutionRecord>();

            foreach (var jobRun in jobRuns.OrderByDescending(r => r.StartedAt))
            {
                var actionRecords = jobRun.Actions.Select(a => new ActionExecutionRecord(
                    a.ActionName,
                    a.Status,
                    a.Message,
                    jobRun.StartedAt.DateTime,
                    jobRun.FinishedAt?.DateTime)).ToList();

                records.Add(new JobExecutionRecord(
                    jobRun.JobId,
                    "", // JobName not stored in JobRun, would need to join
                    jobRun.StartedAt.DateTime,
                    jobRun.Status,
                    jobRun.Message,
                    actionRecords));
            }

            return records;
        }
    }
}