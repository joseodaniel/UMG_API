using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace UMG_API.Models.DTO
{
    public class CambiarContrasenaDto
    {
        public int UMG_ID { get; set; }
        public string NuevaContrasena { get; set; }
    }
}