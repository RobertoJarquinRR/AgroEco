using AgroEco.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace AgroEco.Core.Jobs.Triggers.Persistence
{
    public class CreateTrigger
    {
        private readonly IRepository<Trigger> _repository;
        private readonly GetAllTrigger _getAll;

        public CreateTrigger(IRepository<Trigger> repository, GetAllTrigger getAll)
        {
            _repository = repository;
            _getAll = getAll;
        }

        public async Task<Result> HandleAsync(Trigger trigger)
        {
            if (trigger == null || string.IsNullOrWhiteSpace(trigger.Name))
            {
                return Result.CreateFailure("Trigger name can't be empty");
            }

            var result = await _getAll.HandleAsync();

            if (!result.Success)
            {
                return Result.CreateFailure($"Could not create trigger: {result.Message}");
            }

            bool exists = result.Value.Any(t => string.Equals(t.Name?.Trim(), trigger.Name.Trim(), StringComparison.OrdinalIgnoreCase));

            if (exists)
            {
                return Result.CreateFailure($"A trigger with name '{trigger.Name}' already exists");
            }

            await _repository.AddAsync(trigger);

            return Result.CreateSuccess($"Trigger '{trigger.Name}' created successfully");
        }
    }
}