using System;
using System.Net;
using System.Web.Http;
using UMG_API.Models.DTO;
using UMG_API.Services;

namespace UMG_API.Controllers
{
    [RoutePrefix("api/reservas")]
    public class ReservasController : ApiController
    {
        private readonly ReservaService _service = new ReservaService();

        // GET api/reservas?labId=1&fecha=2026-07-21&userId=3
        [HttpGet]
        [Route("")]
        public IHttpActionResult Get(int? labId = null, DateTime? fecha = null, int? userId = null)
        {
            var reservas = _service.ObtenerTodas(labId, fecha, userId);
            return Content(HttpStatusCode.OK, reservas);
        }

        // GET api/reservas/{id}
        [HttpGet]
        [Route("{id:int}")]
        public IHttpActionResult GetPorId(int id)
        {
            try
            {
                var reserva = _service.ObtenerPorId(id);
                return Content(HttpStatusCode.OK, reserva);
            }
            catch (ArgumentException ex)
            {
                return Content(HttpStatusCode.NotFound, new { mensaje = ex.Message });
            }
        }

        // PATCH api/reservas/{id}/cancelar?userId=3
        [HttpPatch]
        [Route("{id:int}/cancelar")]
        public IHttpActionResult Cancelar(int id, int? userId = null)
        {
            try
            {
                _service.CancelarReserva(id, userId);
                return Content(HttpStatusCode.OK, new { mensaje = "Reserva cancelada correctamente." });
            }
            catch (ArgumentException ex)
            {
                return Content(HttpStatusCode.NotFound, new { mensaje = ex.Message });
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

        // POST api/reservas
        [HttpPost]
        [Route("")]
        public IHttpActionResult Post([FromBody] ReservaCreateDto dto)
        {
            if (dto == null)
            {
                return BadRequest("El cuerpo de la solicitud no puede estar vacío.");
            }

            try
            {
                var reserva = _service.CrearReserva(
                    dto.UMG_User_ID, dto.UMG_Lab_ID, dto.UMG_Fecha_Reserva,
                    dto.UMG_Hora_Inicio, dto.UMG_Hora_Fin, dto.UMG_Motivo);

                return Content(HttpStatusCode.Created, reserva);
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
    }
}