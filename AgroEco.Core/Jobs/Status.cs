using System;
using System.Collections.Generic;
using System.Text;

namespace AgroEco.Core.Jobs
{
    public enum Status
    {
        Created,
        Running,
        Succeeded,
        Faulted,
        Canceled
    }
}
