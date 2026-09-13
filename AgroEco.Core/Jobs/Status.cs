using System;
using System.Collections.Generic;
using System.Text;

namespace AgroEco.Core.Jobs
{
    public enum Status
    {
        Created = 1,
        Enqueued = 2,
        Running = 3,
        Succeeded = 4,
        Faulted = 5,
        Canceled = 6
    }
}
