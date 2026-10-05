using AgroEco.Core.DTOs;
using AgroEco.Core.Interfaces;
using AgroEco.Core.Log_in;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace AgroEco.Core.Services
{
    public class AuthService : IAuthService
    {
        private readonly IUserRepository _userRepository;
        private readonly IUserSession _userSession;

        public AuthService(IUserRepository userRepository, IUserSession userSession)
        {
            _userRepository = userRepository;
            _userSession = userSession;
        }

        public async Task<Result<User>> AuthenticateAsync(LoginRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Username) || string.IsNullOrWhiteSpace(request.Password))
            {
                return Result<User>.CreateFailure("Las credenciales son obligatorias.");
            }

            var user = await _userRepository.GetByUsernameAsync(request.Username);
            if (user == null || !user.IsActive)
            {
                return Result<User>.CreateFailure("Credenciales inválidas o cuenta inactiva.");
            }

            // Uso de sintaxis/concepto de comparación de contraseñas
            bool isValid = VerifyPassword(request.Password, user.PasswordHash);
            if (!isValid)
            {
                return Result<User>.CreateFailure("Credenciales inválidas.");
            }

            // Guardar estado en el Singleton de sesión
            _userSession.StartSession(user);

            return Result<User>.CreateSuccess(user);
        }

        private bool VerifyPassword(string password, string hash)
        {
            // Marcador de posición para la lógica de hash (p. ej. BCrypt)
            return password == hash;
        }
    }
}