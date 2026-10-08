using AgroEco.Core.Triggers.Events;
using Microsoft.EntityFrameworkCore;

namespace AgroEco.Data.Repositories
{
    public class TriggerEventRepository : RepositoryBase<TriggerEvent, DataContext>
    {
        public TriggerEventRepository(DataContext context) : base(context) { }

        protected override void ApplyChanges(TriggerEvent existingEntity, TriggerEvent newEntity)
        {
            existingEntity.TriggerId = newEntity.TriggerId;
            existingEntity.OccurredAt = newEntity.OccurredAt;
            existingEntity.EventType = newEntity.EventType;
            existingEntity.Message = newEntity.Message;
            existingEntity.JobRunId = newEntity.JobRunId;
        }
    }
}