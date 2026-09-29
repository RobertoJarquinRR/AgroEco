using AgroEco.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace AgroEco.Core.Jobs.Triggers.Persistence
{
    public class DeleteTrigger
    {
        private readonly IRepository<Trigger> _repository;
        private readonly IUnitOfWork _unitOfWork;

        public DeleteTrigger(IRepository<Trigger> repository, IUnitOfWork unitOfWork)
        {
            _repository = repository;
            _unitOfWork = unitOfWork;
        }

        public async Task<Result> HandleAsync(int id)
        {
            var trigger = await _repository.GetByIdAsync(id);
            if (trigger == null)
            {
                return Result.CreateFailure($"Trigger with id '{id}' not found");
            }

            await _repository.DeleteAsync(id);
            await _unitOfWork.SaveChangesAsync();

            return Result.CreateSuccess($"Trigger '{trigger.Name}' deleted successfully");
        }
    }
}