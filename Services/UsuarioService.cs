using Org.BouncyCastle.Crypto.Generators;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Helpers;
using UMG_API.Models;
using UMG_API.Models.DTO;
using UMG_API.Repositories;

namespace UMG_API.Services
{
    public class UsuarioService
    {
        private readonly UsuarioRepository _repository = new UsuarioRepository();
        private readonly LogService _logService = new LogService();

        private static readonly Regex EmailRegex = new Regex(
            @"^[^@\s]+@[^@\s]+\.[^@\s]+$", RegexOptions.Compiled);
        public void InactivarUsuario(int userId)
        {
            if (!_repository.ExisteUsuarioActivo(userId))
            {
                throw new ArgumentException("El usuario no existe o ya se encuentra inactivo.");
            }

            bool exito = _repository.Inactivar(userId);

            if (!exito)
            {
                throw new InvalidOperationException("No se pudo inactivar el usuario.");
            }

            _logService.Registrar(userId, "INACTIVAR_USUARIO", "Usuarios",
                $"El usuario con ID {userId} fue inactivado por un administrador.");
        }

        public void ResetearContrasena(int userId, string contrasenaTemporal)
        {
            if (!_repository.ExisteUsuarioActivo(userId))
            {
                throw new ArgumentException("El usuario no existe o está inactivo.");
            }

            if (string.IsNullOrWhiteSpace(contrasenaTemporal) || contrasenaTemporal.Length < 6)
            {
                throw new ArgumentException("La contraseña temporal debe tener al menos 6 caracteres.");
            }

            bool exito = _repository.ResetearContrasena(userId, contrasenaTemporal);

            if (!exito)
            {
                throw new InvalidOperationException("No se pudo resetear la contraseña del usuario.");
            }

            _logService.Registrar(userId, "RESET_CONTRASENA", "Usuarios",
                $"Un administrador reseteó la contraseña del usuario con ID {userId}. Se requerirá cambio en el próximo ingreso.");
        }
        public List<UsuarioListDto> ObtenerTodos()
        {
            return _repository.ObtenerTodos();
        }
        public Usuario CrearUsuario(string correo, string contrasena, string nombre, string apellido, int rolId)
        {
            if (string.IsNullOrWhiteSpace(correo) || !EmailRegex.IsMatch(correo))
            {
                throw new ArgumentException("El correo electrónico no tiene un formato válido.");
            }

            if (string.IsNullOrWhiteSpace(contrasena) || contrasena.Length < 3)
            {
                throw new ArgumentException("La contraseña debe tener al menos 3 caracteres.");
            }

            if (string.IsNullOrWhiteSpace(nombre) || string.IsNullOrWhiteSpace(apellido))
            {
                throw new ArgumentException("El nombre y apellido son obligatorios.");
            }

            if (!_repository.ExisteRol(rolId))
            {
                throw new ArgumentException("El rol especificado no existe o está inactivo.");
            }

            if (_repository.ExisteCorreo(correo.Trim().ToUpper()))
            {
                throw new InvalidOperationException($"Ya existe un usuario registrado con el correo '{correo}'.");
            }

            var usuario = _repository.Crear(correo.Trim().ToUpper(), contrasena, nombre.Trim().ToUpper(), apellido.Trim().ToUpper(), rolId);

            _logService.Registrar(
                usuario.UMG_ID,
                "CREAR_USUARIO",
                "Usuarios",
                $"Se registró el usuario '{usuario.UMG_Usuario}' con ID {usuario.UMG_ID}."
            );

            return usuario;



            //  return _repository.Crear(correo.Trim().ToUpper(), contrasena, nombre.Trim().ToUpper(), apellido.Trim().ToUpper(), rolId);
        }
    }

}