using AgroEco.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace AgroEco.Core.Jobs.Actions.Persistence
{
    public class CreateAction
    {
        private readonly IRepository<Action> _repository;
        private readonly GetAllAction _getAll;

        public CreateAction(IRepository<Action> repository, GetAllAction getAll)
        {
            _repository = repository;
            _getAll = getAll;
        }

        public async Task<Result> HandleAsync(Action action)
        {
            if (action == null || string.IsNullOrWhiteSpace(action.Name))
            {
                return Result.CreateFailure("Action name can't be empty");
            }

            var result = await _getAll.HandleAsync();

            if (!result.Success)
            {
                return Result.CreateFailure($"Could not create action: {result.Message}");
            }

            bool exists = result.Value.Any(a => string.Equals(a.Name.Trim(), action.Name.Trim(), StringComparison.OrdinalIgnoreCase));

            if (exists)
            {
                return Result.CreateFailure($"An action with name '{action.Name}' already exists");
            }

            await _repository.AddAsync(action);

            return Result.CreateSuccess($"Action '{action.Name}' created successfully");
        }
    }
}