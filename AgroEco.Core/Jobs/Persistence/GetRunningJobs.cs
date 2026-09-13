using System;
using System.Collections.Generic;
using System.Text;

namespace AgroEco.Core.Jobs.Persistence
{
    public class GetRunningJobs
    {
        private readonly IJobRepository _jobRepository;

        public GetRunningJobs(IJobRepository jobRepository)
        {
            _jobRepository = jobRepository;
        }

        public async Task<Result<List<Job>>> HandleAsync()
        {
            var jobs = await _jobRepository.GetRunningJobsAsync();

            if (jobs == null || jobs.Count == 0)
            {
                return Result<List<Job>>.CreateSuccess(jobs ?? new List<Job>(), "there are no running jobs");
            }

            return Result<List<Job>>.CreateSuccess(jobs, "running jobs retrieved successfully");
        }
    }
}