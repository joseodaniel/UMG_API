using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace UMG_API.Models.DTO
{
    public class ReservaListDto
    {
        public int UMG_ID { get; set; }
        public int UMG_User_ID { get; set; }
        public string UMG_Docente_Nombre { get; set; }
        public string UMG_Docente_Correo { get; set; }
        public int UMG_Lab_ID { get; set; }
        public string UMG_Lab_Nombre { get; set; }
        public DateTime UMG_Fecha_Reserva { get; set; }
        public TimeSpan UMG_Hora_Inicio { get; set; }
        public TimeSpan UMG_Hora_Fin { get; set; }
        public string UMG_Motivo { get; set; }
        public string UMG_Estado { get; set; }
        public DateTime UMG_Fecha_Registro { get; set; }
    }
}