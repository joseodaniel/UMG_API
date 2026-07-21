using System;
using UMG_API.Models.DTO;
using UMG_API.Repositories;

namespace UMG_API.Services
{
    public class AuthService
    {
        private readonly UsuarioRepository _repository = new UsuarioRepository();
        private readonly LogService _logService = new LogService();

        public LoginResponseDto Login(string correo, string contrasena)
        {
            if (string.IsNullOrWhiteSpace(correo.ToUpper()) || string.IsNullOrWhiteSpace(contrasena))
            {
                throw new ArgumentException("Debe ingresar usuario y contraseña.");
            }

            var usuario = _repository.ValidarCredenciales(correo.Trim().ToUpper(), contrasena);

            if (usuario == null)
            {
                _logService.Registrar(null, "LOGIN_FALLIDO", "Autenticación",
                    $"Intento de inicio de sesión fallido para el correo '{correo.ToUpper()}'.");

                throw new InvalidOperationException("Usuario o contraseña incorrectos.");
            }

            _repository.ActualizarUltimoAcceso(usuario.UMG_ID);

            _logService.Registrar(usuario.UMG_ID, "LOGIN", "Autenticación",
                $"El usuario '{usuario.UMG_Usuario}' inició sesión correctamente.");

            return usuario;
        }

        public void CambiarContrasena(int userId, string nuevaContrasena)
        {
            if (!_repository.ExisteUsuarioActivo(userId))
            {
                throw new ArgumentException("El usuario especificado no existe o está inactivo.");
            }

            if (string.IsNullOrWhiteSpace(nuevaContrasena) || nuevaContrasena.Length < 6)
            {
                throw new ArgumentException("La nueva contraseña debe tener al menos 6 caracteres.");
            }

            _repository.CambiarContrasena(userId, nuevaContrasena);

            _logService.Registrar(userId, "CAMBIO_CONTRASENA", "Autenticación",
                $"El usuario con ID {userId} actualizó su contraseña de primer ingreso.");
        }
    }
}