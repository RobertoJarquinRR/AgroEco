using AgroEco.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace AgroEco.Core.Triggers.Persistence
{
    public class GetByIdTrigger
    {
        private readonly IRepository<Trigger> _repository;

        public GetByIdTrigger(IRepository<Trigger> repository)
        {
            _repository = repository;
        }

        public async Task<Result<Trigger>> HandleAsync(
            int id,
            CancellationToken cancellationToken = default)
        {
            var result = await _repository.GetByIdAsync(id, cancellationToken);
            if (result == null)
            {
                return Result<Trigger>.CreateFailure("Trigger not found");
            }
            return Result<Trigger>.CreateSuccess(result);
        }
    }
}