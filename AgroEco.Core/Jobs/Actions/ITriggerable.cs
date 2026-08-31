using System;
using System.Collections.Generic;
using System.Text;

namespace AgroEco.Core.Jobs.Actions
{
    public interface ITriggerable
    {
        Task<Result> Ontrigger();
    }
}
