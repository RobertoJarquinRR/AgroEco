using AgroEco.Core.Log_in;
using System;
using System.Collections.Generic;
using System.Text;

namespace AgroEco.Core.Interfaces
{
    public interface IUserSession
    {
        User? CurrentUser { get; }
        bool IsAuthenticated { get; }
        void StartSession(User user);
        void ClearSession();
    }
}