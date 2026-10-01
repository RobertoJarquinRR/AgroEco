using AgroEco.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace AgroEco.Core.Jobs.Actions.Persistence
{
    public class DeleteAction
    {
        private readonly IRepository<Action> _repository;
        private readonly IUnitOfWork _unitOfWork;

        public DeleteAction(IRepository<Action> repository, IUnitOfWork unitOfWork)
        {
            _repository = repository;
            _unitOfWork = unitOfWork;
        }

        public async Task<Result> HandleAsync(int id)
        {
            var action = await _repository.GetByIdAsync(id);
            if (action == null)
            {
                return Result.CreateFailure($"Action with id '{id}' not found");
            }

            await _repository.DeleteAsync(id);
            await _unitOfWork.SaveChangesAsync();

            return Result.CreateSuccess($"Action '{action.Name}' deleted successfully");
        }
    }
}