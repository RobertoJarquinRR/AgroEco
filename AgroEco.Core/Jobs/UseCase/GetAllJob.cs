using AgroEco.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace AgroEco.Core.Jobs.UseCase
{
    public class GetAllJob
    {
        private readonly IRepository<Job> _repository;

        public GetAllJob(IRepository<Job> repository)
        {
            _repository = repository;
        }


        public async Task<Result<List<Job>>> HandleAsync()
        {

            var result = await _repository.GetAllAsync();

            if(result == null){
                return Result<List<Job>>.CreateFailure("Theres not job yet");
            }

            return Result<List<Job>>.CreateSuccess(result);
        }
    }
}
