using AgroEco.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace AgroEco.Core.Jobs.Triggers.Persistence
{
    public class DeleteTrigger
    {
        private readonly IRepository<Trigger> _repository;

        public DeleteTrigger(IRepository<Trigger> repository)
        {
            _repository = repository;
        }

        public async Task<Result> HandleAsync(int id)
        {
            var trigger = await _repository.GetByIdAsync(id);
            if (trigger == null)
            {
                return Result.CreateFailure($"Trigger with id '{id}' not found");
            }

            await _repository.DeleteAsync(id);

            return Result.CreateSuccess($"Trigger '{trigger.Name}' deleted successfully");
        }
    }
}