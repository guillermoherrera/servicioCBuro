using System;
using System.Collections.Generic;
using System.Text;

namespace VR2ConfiaShopLocalService
{
    public class RespuestaAutorizacion
    { 
        public Respuesta[] Respuestas { get; set; }
    }

    public class Respuesta
    {
        public int regresa { get; set; }
        public string msj { get; set; }
        public string validacion { get; set; }
    }

}
