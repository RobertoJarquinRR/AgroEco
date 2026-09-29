using AgroEco.Core.Interfaces;
using AgroEco.Core.Jobs.Actions;
using Action = AgroEco.Core.Jobs.Actions.Action;
using AgroEco.Core.Triggers;
using System;
using System.Collections.Generic;
using System.Net.NetworkInformation;
using System.Text;

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

        public async Task<Result> HandleAsync(string name,
    string? description,
    int? priority,
    List<Action> action,
    Trigger trigger)
        {
            var result = await _getall.HandleAsync();

            if (!result.Success)
            {
                return Result.CreateFailure($"Could not create job: {result.Message}");
            }

            bool exists = result.Value.Any(j => string.Equals(j.Name.Trim(), name.Trim(), StringComparison.OrdinalIgnoreCase));

            if (exists)
            {
                return Result.CreateFailure($"A job with name '{name}' already exists");
            }

            Result<Job> createtask = await Job.CreateJob(name, description, Status.Created, priority,  action, trigger);

            if (!createtask.Success)
            {
                return Result.CreateFailure($"Error creating job '{name}': {createtask.Message}");
            }

            await _repository.AddAsync(createtask.Value);
            await _unitOfWork.SaveChangesAsync();

            return Result.CreateSuccess($"Job '{name}' created successfully");
        }
    }
}
