using AgroEco.Core.Interfaces;
using AgroEco.Core.Log_in;
using System;
using System.Collections.Generic;
using System.Text;

namespace AgroEco.Core.Services
{
    public class UserSession : IUserSession
    {
        public User? CurrentUser { get; private set; }
        public bool IsAuthenticated => CurrentUser != null;

        public void StartSession(User user)
        {
            // Asigna el usuario a la sesión activa en RAM
            CurrentUser = user;
        }

        public void ClearSession()
        {
            // Limpia los datos de sesión
            CurrentUser = null;
        }
    }
}
