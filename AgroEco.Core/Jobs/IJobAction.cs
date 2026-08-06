using System;
using System.Collections.Generic;
using System.Text;

namespace AgroEco.Core.Jobs
{
    public interface IJobAction
    {

        Task<Result> Execute();
    }
}
