using AgroEco.Core.Log_in;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace AgroEco.Core.Interfaces
{
    public interface IUserRepository
    {
        Task<User?> GetByUsernameAsync(string username);
    }
}