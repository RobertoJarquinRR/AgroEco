using System;
using System.Collections.Generic;
using System.Text;

namespace AgroEco.Core.Interfaces
{
    public interface ISerialConexion
    {
        bool connected { get; }
        Task<Result> OpenConnexion();

        Task<Result> CloseConexion();

       

  
    }
}
