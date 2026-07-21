using System;
using System.Net;
using System.Web.Http;
using UMG_API.Models.DTO;
using UMG_API.Services;

namespace UMG_API.Controllers

{
    [RoutePrefix("api/usuarios")]
    public class UsuariosController : ApiController
    {
        private readonly UsuarioService _service = new UsuarioService();

        // POST api/usuarios
        [HttpPost]
        [Route("")]
        public IHttpActionResult Post([FromBody] UsuarioCreateDto dto)
        {
            if (dto == null)
            {
                return BadRequest("El cuerpo de la solicitud no puede estar vacío.");
            }

            try
            {
                var usuario = _service.CrearUsuario(
                    dto.UMG_Usuario, dto.UMG_Contrasena, dto.UMG_Nombre, dto.UMG_Apellido, dto.UMG_Rol_ID);

                return Content(HttpStatusCode.Created, usuario);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                return Content(HttpStatusCode.Conflict, new { mensaje = ex.Message });
            }
            catch (Exception)
            {
                return InternalServerError();
            }
        }

        [HttpPatch]
        [Route("{id:int}/inactivar")]
        public IHttpActionResult Inactivar(int id)
        {
            try
            {
                _service.InactivarUsuario(id);
                return Content(HttpStatusCode.OK, new { mensaje = "Usuario inactivado correctamente." });
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

        [HttpPatch]
        [Route("{id:int}/resetear-contrasena")]
        public IHttpActionResult ResetearContrasena(int id, [FromBody] ResetearContrasenaDto dto)
        {
            if (dto == null)
            {
                return BadRequest("El cuerpo de la solicitud no puede estar vacío.");
            }

            try
            {
                _service.ResetearContrasena(id, dto.ContrasenaTemporal);
                return Content(HttpStatusCode.OK, new { mensaje = "Contraseña reseteada. El usuario deberá cambiarla en su próximo ingreso." });
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

        [HttpGet]
        [Route("")]
        public IHttpActionResult Get()
        {
            var usuarios = _service.ObtenerTodos();
            return Content(HttpStatusCode.OK, usuarios);
        }

    }
}