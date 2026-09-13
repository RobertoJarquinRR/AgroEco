using AgroEco.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace AgroEco.Core.Jobs.Actions.Persistence
{
    public class GetAllAction
    {
        private readonly IRepository<Action> _repository;

        public GetAllAction(IRepository<Action> repository)
        {
            _repository = repository;
        }

        public async Task<Result<List<Action>>> HandleAsync()
        {
            var result = await _repository.GetAllAsync();

            if (result == null)
            {
                return Result<List<Action>>.CreateFailure("There are no actions yet");
            }

            return Result<List<Action>>.CreateSuccess(result);
        }
    }
}