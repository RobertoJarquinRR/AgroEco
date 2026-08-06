using System;
using System.Collections.Generic;
using System.Text;

namespace AgroEco.Core.Jobs.Triggers
{
    public interface ITrigger
    {
        public event Func<Task>? OnTriggerFired;
        Task probar();
    }
}
