using AgroEco.Core.Interfaces;
using AgroEco.Core.Jobs.Actions;
using Action = AgroEco.Core.Jobs.Actions.Action;
using AgroEco.Core.Jobs.Triggers;
using System;
using System.Collections.Generic;
using System.Net.NetworkInformation;
using System.Text;

namespace AgroEco.Core.Jobs.UseCase
{
    public class CreateJob
    {
        private readonly IRepository<Job> _repository;
        private readonly GetAllJob _getall;

        public CreateJob(IRepository<Job> repository, GetAllJob getall){
            _repository = repository;
            _getall = getall;

        }

        public async Task<Result> HandleAsync(string name,
    string? description,
    int? priority,
    DateTime? date,
    List<Action> action,
    Trigger trigger)
        {
            var result = await _getall.HandleAsync();

            if (!result.Success)
            {
                return Result.CreateFailure(result.Message ?? "Can't create the jobs");
            }

            bool exists = result.Value.Any(j => string.Equals(j.Name.Trim(), name.Trim(), StringComparison.OrdinalIgnoreCase));

            if (exists)
            {
                return Result.CreateFailure("This job already exists");
            }

            Result<Job> createtask = await Job.CreateJob(name, description, Status.Created, priority, date, action, trigger);

            if (!createtask.Success)
            {
                return Result.CreateFailure(createtask.Message ?? "Can't create the jobs");
            }

            await _repository.AddAsync(createtask.Value);

            return Result.CreateSuccess();
        }
    }
}
