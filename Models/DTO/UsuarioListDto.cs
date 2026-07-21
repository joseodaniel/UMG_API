using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace UMG_API.Models.DTO
{
    public class UsuarioListDto
    {
        public int UMG_ID { get; set; }
        public string UMG_Usuario { get; set; }
        public string UMG_Nombre { get; set; }
        public string UMG_Apellido { get; set; }
        public int UMG_Rol_ID { get; set; }
        public string UMG_Rol_Nombre { get; set; }
        public int UMG_Estado { get; set; }
        public int UMG_Ingreso { get; set; }
        public DateTime UMG_Fecha_Creacion { get; set; }
        public DateTime? UMG_Ultimo_Acceso { get; set; }
    }

}