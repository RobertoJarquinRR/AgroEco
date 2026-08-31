using AgroEco.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace AgroEco.Core.Jobs.UseCase
{
    public class GetByIdJob
    {
        private readonly IRepository<Job> _repository;

        public GetByIdJob(IRepository<Job> repository){
            _repository = repository;
        }

        public async Task<Result<Job>> HandleAsync(int id){
            var result = await _repository.GetByIdAsync(id);
            if(result == null){
                return Result<Job>.CreateFailure("Job not found");
            }
            return Result<Job>.CreateSuccess(result);
        }


    }
}
