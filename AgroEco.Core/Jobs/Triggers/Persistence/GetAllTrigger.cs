using AgroEco.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace AgroEco.Core.Jobs.Triggers.Persistence
{
    public class GetAllTrigger
    {
        private readonly IRepository<Trigger> _repository;

        public GetAllTrigger(IRepository<Trigger> repository)
        {
            _repository = repository;
        }

        public async Task<Result<List<Trigger>>> HandleAsync()
        {
            var result = await _repository.GetAllAsync();

            if (result == null)
            {
                return Result<List<Trigger>>.CreateFailure("There are no triggers yet");
            }

            return Result<List<Trigger>>.CreateSuccess(result);
        }
    }
}