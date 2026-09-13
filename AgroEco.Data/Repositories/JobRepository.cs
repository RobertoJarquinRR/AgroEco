using AgroEco.Core.Jobs;
using AgroEco.Data;
using Microsoft.EntityFrameworkCore;

namespace AgroEco.Data.Repositories
{
    public class JobRepository : RepositoryBase<Job, DataContext> ,IJobRepository
    {
        public JobRepository(DataContext context) : base(context) { }

      

        protected override void ApplyChanges(Job existingEntity, Job newEntity)
        {      
            existingEntity.ChangeStatus(newEntity.Status);
        }

        public async Task<List<Job>> GetRunningJobsAsync()
        {
            return await _context.Jobs
            .Where(j => j.Status == Status.Running)
            .Include(j => j.Trigger)
            .Include(j => j.Action)
            .ToListAsync();
        }
    }
}