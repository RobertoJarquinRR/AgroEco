using AgroEco.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace AgroEco.Core.Jobs.Persistence
{
    public class DeleteJob
    {
        private readonly IRepository<Job> _repository;
        private readonly IUnitOfWork _unitOfWork;

        public DeleteJob(IRepository<Job> repository, IUnitOfWork unitOfWork)
        {
            _repository = repository;
            _unitOfWork = unitOfWork;
        }

        public async Task<Result> HandleAsync(int id)
        {
            var job = await _repository.GetByIdAsync(id);
            if (job == null)
            {
                return Result.CreateFailure($"Job with id '{id}' not found");
            }

            await _repository.DeleteAsync(id);
            await _unitOfWork.SaveChangesAsync();

            return Result.CreateSuccess($"Job '{job.Name}' deleted successfully");
        }
    }
}