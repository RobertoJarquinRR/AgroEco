using AgroEco.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace AgroEco.Core.Jobs.Actions.Persistence
{
    public class GetByIdAction
    {
        private readonly IRepository<Action> _repository;

        public GetByIdAction(IRepository<Action> repository)
        {
            _repository = repository;
        }

        public async Task<Result<Action>> HandleAsync(int id)
        {
            var result = await _repository.GetByIdAsync(id);
            if (result == null)
            {
                return Result<Action>.CreateFailure("Action not found");
            }
            return Result<Action>.CreateSuccess(result);
        }
    }
}