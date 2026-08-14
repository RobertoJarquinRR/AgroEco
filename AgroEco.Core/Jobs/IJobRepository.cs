using System;
using System.Collections.Generic;
using System.Text;

namespace AgroEco.Core.Jobs
{
    public interface IJobRepository<T>
    {
          Task<Job> GetJobWithDetailsAsync(int id);
    }
}
