using System;
using System.Collections.Generic;
using System.Text;

namespace AgroEco.Core.Triggers
{
    public interface ITriggerable
    {
        Task<Result> OnTrigger();
    }
}
