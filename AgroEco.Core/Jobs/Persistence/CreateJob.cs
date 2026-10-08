using AgroEco.Core.Interfaces;
using AgroEco.Core.Jobs.Actions;
using Action = AgroEco.Core.Jobs.Actions.Action;
using AgroEco.Core.Triggers;

namespace AgroEco.Core.Jobs.Persistence
{
    public class CreateJob
    {
        private readonly IRepository<Job> _repository;
        private readonly GetAllJob _getall;
        private readonly IUnitOfWork _unitOfWork;

        public CreateJob(
            IRepository<Job> repository,
            GetAllJob getall,
            IUnitOfWork unitOfWork)
        {
            _repository = repository;
            _getall = getall;
            _unitOfWork = unitOfWork;
        }

        public async Task<Result> HandleAsync(
            string name,
            string? description,
            int? priority,
            List<Action> actions,
            Trigger trigger)
        {
            Result<List<Job>> existingJobsResult = await _getall.HandleAsync();

            if (!existingJobsResult.Success)
            {
                return Result.CreateFailure(
                    $"Could not create job: {existingJobsResult.Message}");
            }

            bool exists = existingJobsResult.Value.Any(
                job => string.Equals(
                    job.Name.Trim(),
                    name.Trim(),
                    StringComparison.OrdinalIgnoreCase));

            if (exists)
            {
                return Result.CreateFailure($"A job with name '{name}' already exists");
            }

            Result<Job> createResult = await Job.CreateJob(
                name,
                description,
                Status.Created,
                priority,
                actions,
                trigger);

            if (!createResult.Success)
            {
                return Result.CreateFailure(
                    $"Error creating job '{name}': {createResult.Message}");
            }

            await _repository.AddAsync(createResult.Value);
            await _unitOfWork.SaveChangesAsync();

            return Result.CreateSuccess($"Job '{name}' created successfully");
        }
    }
}
