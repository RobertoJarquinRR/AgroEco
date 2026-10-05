using System;
using System.Collections.Generic;
using System.Text;

namespace AgroEco.Core.Log_in
{
    public class User
    {
            public int Id { get; set; }
            public string Username { get; set; } = string.Empty;
            public string PasswordHash { get; set; } = string.Empty;
            public bool IsActive { get; set; }
    }
}
