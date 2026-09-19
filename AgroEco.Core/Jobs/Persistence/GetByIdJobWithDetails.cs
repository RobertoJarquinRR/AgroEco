using AgroEco.Core.Jobs;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace AgroEco.Core.Jobs.Persistence
{
    public class GetByIdJobWithDetails
    {
        private readonly IJobRepository _jobRepository;

        public GetByIdJobWithDetails(IJobRepository jobRepository)
        {
            _jobRepository = jobRepository;
        }

        public async Task<Result<Job>> HandleAsync(int id, CancellationToken ct = default)
        {
            var job = await _jobRepository.GetByIdWithDetailsAsync(id, ct);

            if (job == null)
            {
                return Result<Job>.CreateFailure($"Job with ID {id} not found");
            }

            return Result<Job>.CreateSuccess(job);
        }
    }
}