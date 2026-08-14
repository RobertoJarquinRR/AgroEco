using AgroEco.Core.Jobs;
using AgroEco.Data;
using Microsoft.EntityFrameworkCore;

namespace AgroEco.Data.Repositories
{
    public class JobRepository : RepositoryBase<Job, DataContext> 
    {
        public JobRepository(DataContext context) : base(context) { }

      
        protected override void ApplyChanges(Job existingEntity, Job newEntity)
        {      
            existingEntity.ChangeStatus(newEntity.Status);
        }
    }
}