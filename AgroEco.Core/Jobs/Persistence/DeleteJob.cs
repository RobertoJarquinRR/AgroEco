using AgroEco.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace AgroEco.Core.Jobs.Persistence
{
    public class DeleteJob
    {
        private readonly IRepository<Job> _repository;

        public DeleteJob(IRepository<Job> repository)
        {
            _repository = repository;
        }

        public async Task<Result> HandleAsync(int id)
        {
            var job = await _repository.GetByIdAsync(id);
            if (job == null)
            {
                return Result.CreateFailure($"Job with id '{id}' not found");
            }

            await _repository.DeleteAsync(id);

            return Result.CreateSuccess($"Job '{job.Name}' deleted successfully");
        }
    }
}