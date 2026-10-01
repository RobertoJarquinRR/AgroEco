using System;
using System.Collections.Generic;
using System.Text;

namespace AgroEco.Core.Hardware
{
    public interface IActivable
    {
        Task<Result> Activate();
    }
}
