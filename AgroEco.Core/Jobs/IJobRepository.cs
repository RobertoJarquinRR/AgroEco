using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using AgroEco.Core.Jobs.Persistence.Queries;

namespace AgroEco.Core.Jobs
{
    public interface IJobRepository
    {
        Task<List<Job>> GetRunningJobsAsync(
            CancellationToken ct = default);

        Task<Job?> GetByIdWithDetailsAsync(
            int id,
            CancellationToken ct = default);

        Task<(List<Job> Jobs, int TotalCount)> GetFilteredAsync(
            JobFilter filter,
            CancellationToken ct = default);

        Task<List<JobExecutionRecord>> GetExecutionHistoryAsync(
            int jobId,
            CancellationToken ct = default);
    }
}
