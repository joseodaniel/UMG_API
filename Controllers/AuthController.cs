using System;
using System.Net;
using System.Web.Http;
using UMG_API.Models.DTO;
using UMG_API.Services;

namespace UMG_API.Controllers
{
    [RoutePrefix("api/auth")]
    public class AuthController : ApiController
    {
        private readonly AuthService _service = new AuthService();

        // POST api/auth/login
        [HttpPost]
        [Route("login")]
        public IHttpActionResult Login([FromBody] LoginDto dto)
        {
            if (dto == null)
            {
                return BadRequest("El cuerpo de la solicitud no puede estar vacío.");
            }

            try
            {
                var resultado = _service.Login(dto.UMG_Usuario, dto.UMG_Contrasena);
                return Content(HttpStatusCode.OK, resultado);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                return Content(HttpStatusCode.Unauthorized, new { mensaje = ex.Message });
            }
            catch (Exception)
            {
                return InternalServerError();
            }
        }

        // POST api/auth/cambiar-contrasena
        [HttpPost]
        [Route("cambiar-contrasena")]
        public IHttpActionResult CambiarContrasena([FromBody] CambiarContrasenaDto dto)
        {
            if (dto == null)
            {
                return BadRequest("El cuerpo de la solicitud no puede estar vacío.");
            }

            try
            {
                _service.CambiarContrasena(dto.UMG_ID, dto.NuevaContrasena);
                return Content(HttpStatusCode.OK, new { mensaje = "Contraseña actualizada correctamente." });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception)
            {
                return InternalServerError();
            }
        }
    }
}