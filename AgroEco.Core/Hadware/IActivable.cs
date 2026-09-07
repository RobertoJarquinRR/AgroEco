using System;
using System.Collections.Generic;
using System.Text;

namespace AgroEco.Core.Hadware
{
    public interface IActivable
    {
        Task<Result> activar();
    }
}
