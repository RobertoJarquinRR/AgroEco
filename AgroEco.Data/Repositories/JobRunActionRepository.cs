using AgroEco.Core.Jobs.Runs;
using Microsoft.EntityFrameworkCore;

namespace AgroEco.Data.Repositories
{
    public class JobRunActionRepository : RepositoryBase<JobRunAction, DataContext>
    {
        public JobRunActionRepository(DataContext context) : base(context) { }

        protected override void ApplyChanges(JobRunAction existingEntity, JobRunAction newEntity)
        {
            existingEntity.ActionId = newEntity.ActionId;
            existingEntity.ActionName = newEntity.ActionName;
            existingEntity.ActionType = newEntity.ActionType;
            existingEntity.Status = newEntity.Status;
            existingEntity.Message = newEntity.Message;
            existingEntity.Error = newEntity.Error;
            existingEntity.DurationMs = newEntity.DurationMs;
        }
    }
}